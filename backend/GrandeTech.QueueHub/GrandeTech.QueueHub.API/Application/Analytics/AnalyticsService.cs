using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Domain.Organizations;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Domain.Locations;
using Grande.Fila.API.Domain.Staff;
using Grande.Fila.API.Domain.AuditLogs;
using Grande.Fila.API.Domain.Common;
using Grande.Fila.API.Domain.Users;

namespace Grande.Fila.API.Application.Analytics
{
    public class AnalyticsService
    {
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IQueueRepository _queueRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly IStaffMemberRepository _staffMemberRepository;
        private readonly IAuditLogRepository _auditLogRepository;

        public AnalyticsService(
            IOrganizationRepository organizationRepository,
            IQueueRepository queueRepository,
            ILocationRepository locationRepository,
            IStaffMemberRepository staffMemberRepository,
            IAuditLogRepository auditLogRepository)
        {
            _organizationRepository = organizationRepository;
            _queueRepository = queueRepository;
            _locationRepository = locationRepository;
            _staffMemberRepository = staffMemberRepository;
            _auditLogRepository = auditLogRepository;
        }

        public async Task<CrossBarbershopAnalyticsResult> GetCrossBarbershopAnalyticsAsync(
            CrossBarbershopAnalyticsRequest request, 
            string userId, 
            string userRole, 
            CancellationToken cancellationToken)
        {
            var result = new CrossBarbershopAnalyticsResult();

            // Authorization check - only platform admin can access cross-barbershop analytics
            if (userRole != UserRoles.PlatformAdmin)
            {
                result.Success = false;
                result.Errors.Add("Forbidden: Only platform administrators can access cross-barbershop analytics.");
                return result;
            }

            // Validate request
            if (!ValidateAnalyticsRequest(request, result))
            {
                return result;
            }

            try
            {
                // Get organizations that share analytics data
                var organizations = await _organizationRepository.GetOrganizationsWithAnalyticsSharingAsync(cancellationToken);
                
                if (!organizations.Any())
                {
                    result.AnalyticsData = new CrossBarbershopAnalyticsData
                    {
                        TotalOrganizations = 0,
                        PeriodStart = request.StartDate,
                        PeriodEnd = request.EndDate
                    };
                    return result;
                }

                var organizationIds = organizations.Select(o => o.Id).ToList();

                // Get analytics data
                var queues = await _queueRepository.GetQueuesByDateRangeAsync(request.StartDate, request.EndDate, cancellationToken);
                var locations = await _locationRepository.GetLocationsByOrganizationIdsAsync(organizationIds, cancellationToken);
                var staffMembers = await _staffMemberRepository.GetStaffMembersByLocationIdsAsync(locations.Select(l => l.Id).ToList(), cancellationToken);

                // Calculate metrics
                var analyticsData = new CrossBarbershopAnalyticsData
                {
                    TotalOrganizations = organizations.Count,
                    TotalActiveQueues = queues.Count(q => q.IsActive),
                    TotalCompletedServices = CalculateCompletedServices(queues),
                    AverageWaitTimeMinutes = CalculateAverageWaitTime(queues),
                    StaffUtilizationPercentage = CalculateStaffUtilization(staffMembers, queues),
                    TotalLocations = locations.Count,
                    TotalStaffMembers = staffMembers.Count,
                    PeriodStart = request.StartDate,
                    PeriodEnd = request.EndDate
                };

                if (request.IncludeActiveQueues || request.IncludeCompletedServices)
                {
                    analyticsData.TopOrganizations = CalculateTopOrganizations(organizations, queues, locations, staffMembers);
                }

                result.AnalyticsData = analyticsData;

                // Log analytics access
                await LogAnalyticsAccess(userId, "CrossBarbershopAnalytics", request.StartDate, request.EndDate, cancellationToken);

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add($"An error occurred while generating analytics: {ex.Message}");
                return result;
            }
        }

        public async Task<OrganizationAnalyticsResult> GetOrganizationAnalyticsAsync(
            OrganizationAnalyticsRequest request, 
            string userId, 
            string userRole, 
            CancellationToken cancellationToken)
        {
            var result = new OrganizationAnalyticsResult();

            // Validate request
            if (!Guid.TryParse(request.OrganizationId, out var organizationId))
            {
                result.Success = false;
                result.FieldErrors.Add("OrganizationId", "Invalid organization ID format.");
                return result;
            }

            if (!ValidateAnalyticsRequest(request, result))
            {
                return result;
            }

            try
            {
                // Get organization
                var organization = await _organizationRepository.GetByIdAsync(organizationId, cancellationToken);
                if (organization == null)
                {
                    result.Success = false;
                    result.Errors.Add("Organization not found.");
                    return result;
                }

                // Get related data
                var queues = await _queueRepository.GetQueuesByOrganizationIdAsync(organizationId, request.StartDate, request.EndDate, cancellationToken);
                var locations = await _locationRepository.GetLocationsByOrganizationIdAsync(organizationId, cancellationToken);
                var staffMembers = await _staffMemberRepository.GetStaffMembersByOrganizationIdAsync(organizationId, cancellationToken);

                // Calculate metrics
                var analyticsData = new OrganizationAnalyticsData
                {
                    OrganizationId = organizationId,
                    OrganizationName = organization.Name,
                    TotalLocations = locations.Count,
                    TotalStaffMembers = staffMembers.Count,
                    TotalCompletedServices = CalculateCompletedServices(queues),
                    TotalActiveQueues = queues.Count(q => q.IsActive),
                    AverageWaitTimeMinutes = CalculateAverageWaitTime(queues),
                    StaffUtilizationPercentage = CalculateStaffUtilization(staffMembers, queues),
                    PeriodStart = request.StartDate,
                    PeriodEnd = request.EndDate
                };

                result.AnalyticsData = analyticsData;

                // Log analytics access
                await LogAnalyticsAccess(userId, "OrganizationAnalytics", request.StartDate, request.EndDate, cancellationToken);

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add($"An error occurred while generating organization analytics: {ex.Message}");
                return result;
            }
        }

        public async Task<TopPerformingOrganizationsResult> GetTopPerformingOrganizationsAsync(
            TopPerformingOrganizationsRequest request, 
            string userId, 
            string userRole, 
            CancellationToken cancellationToken)
        {
            var result = new TopPerformingOrganizationsResult();

            // Authorization check - only platform admin can access top organizations analytics
            if (userRole != UserRoles.PlatformAdmin)
            {
                result.Success = false;
                result.Errors.Add("Forbidden: Only platform administrators can access top organizations analytics.");
                return result;
            }

            // Validate request
            if (request.StartDate >= request.EndDate)
            {
                result.Success = false;
                result.FieldErrors.Add("StartDate", "Start date must be before end date.");
                return result;
            }

            if (request.MaxResults <= 0 || request.MaxResults > 100)
            {
                result.Success = false;
                result.FieldErrors.Add("MaxResults", "Max results must be between 1 and 100.");
                return result;
            }

            try
            {
                // Get organizations that share analytics data
                var organizations = await _organizationRepository.GetOrganizationsWithAnalyticsSharingAsync(cancellationToken);
                var queues = await _queueRepository.GetQueuesByDateRangeAsync(request.StartDate, request.EndDate, cancellationToken);

                var topOrganizations = CalculateTopOrganizations(organizations, queues, null, null)
                    .OrderByDescending(o => GetMetricValue(o, request.MetricType))
                    .Take(request.MaxResults)
                    .ToList();

                result.TopOrganizations = topOrganizations;
                result.MetricType = request.MetricType;
                result.PeriodStart = request.StartDate;
                result.PeriodEnd = request.EndDate;

                // Log analytics access
                await LogAnalyticsAccess(userId, "TopPerformingOrganizations", request.StartDate, request.EndDate, cancellationToken);

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add($"An error occurred while generating top performing organizations: {ex.Message}");
                return result;
            }
        }

        private bool ValidateAnalyticsRequest(CrossBarbershopAnalyticsRequest request, CrossBarbershopAnalyticsResult result)
        {
            var isValid = true;

            if (request.StartDate >= request.EndDate)
            {
                if (!result.FieldErrors.ContainsKey("StartDate"))
                    result.FieldErrors.Add("StartDate", "Start date must be before end date.");
                if (!result.FieldErrors.ContainsKey("EndDate"))
                    result.FieldErrors.Add("EndDate", "End date must be after start date.");
                isValid = false;
            }

            if (request.StartDate > DateTime.UtcNow)
            {
                if (!result.FieldErrors.ContainsKey("StartDate"))
                    result.FieldErrors.Add("StartDate", "Start date cannot be in the future.");
                isValid = false;
            }

            if (request.EndDate > DateTime.UtcNow)
            {
                if (!result.FieldErrors.ContainsKey("EndDate"))
                    result.FieldErrors.Add("EndDate", "End date cannot be in the future.");
                isValid = false;
            }

            if (!isValid)
            {
                result.Success = false;
            }

            return isValid;
        }

        private bool ValidateAnalyticsRequest(OrganizationAnalyticsRequest request, OrganizationAnalyticsResult result)
        {
            var isValid = true;

            if (request.StartDate >= request.EndDate)
            {
                if (!result.FieldErrors.ContainsKey("StartDate"))
                    result.FieldErrors.Add("StartDate", "Start date must be before end date.");
                if (!result.FieldErrors.ContainsKey("EndDate"))
                    result.FieldErrors.Add("EndDate", "End date must be after start date.");
                isValid = false;
            }

            if (request.StartDate > DateTime.UtcNow)
            {
                if (!result.FieldErrors.ContainsKey("StartDate"))
                    result.FieldErrors.Add("StartDate", "Start date cannot be in the future.");
                isValid = false;
            }

            if (request.EndDate > DateTime.UtcNow)
            {
                if (!result.FieldErrors.ContainsKey("EndDate"))
                    result.FieldErrors.Add("EndDate", "End date cannot be in the future.");
                isValid = false;
            }

            if (!isValid)
            {
                result.Success = false;
            }

            return isValid;
        }

        private int CalculateCompletedServices(IReadOnlyList<Queue> queues)
        {
            return queues.SelectMany(q => q.Entries)
                        .Count(e => e.Status == QueueEntryStatus.Completed);
        }

        private double CalculateAverageWaitTime(IReadOnlyList<Queue> queues)
        {
            var completedEntries = queues.SelectMany(q => q.Entries)
                                        .Where(e => e.Status == QueueEntryStatus.Completed && e.CheckedInAt.HasValue && e.CalledAt.HasValue)
                                        .ToList();

            if (!completedEntries.Any())
                return 0;

            var totalWaitTime = completedEntries.Sum(e => (e.CheckedInAt!.Value - e.CalledAt!.Value).TotalMinutes);
            return totalWaitTime / completedEntries.Count;
        }

        private double CalculateStaffUtilization(IReadOnlyList<StaffMember> staffMembers, IReadOnlyList<Queue> queues)
        {
            if (!staffMembers.Any())
                return 0;

            var activeStaff = staffMembers.Count(s => s.IsActive);
            var busyStaff = staffMembers.Count(s => s.StaffStatus == "busy");

            return activeStaff > 0 ? (double)busyStaff / activeStaff * 100 : 0;
        }

        private List<OrganizationMetrics> CalculateTopOrganizations(
            IReadOnlyList<Organization> organizations, 
            IReadOnlyList<Queue> queues, 
            IReadOnlyList<Location>? locations, 
            IReadOnlyList<StaffMember>? staffMembers)
        {
            var metrics = new List<OrganizationMetrics>();

            foreach (var org in organizations)
            {
                var orgQueues = queues.Where(q => locations?.Any(l => l.OrganizationId == org.Id) == true).ToList();
                var orgLocations = locations?.Where(l => l.OrganizationId == org.Id).ToList() ?? new List<Location>();
                var orgStaff = staffMembers?.Where(s => orgLocations.Any(l => l.Id == s.LocationId)).ToList() ?? new List<StaffMember>();

                var metric = new OrganizationMetrics
                {
                    OrganizationId = org.Id,
                    OrganizationName = org.Name,
                    CompletedServices = CalculateCompletedServices(orgQueues),
                    AverageWaitTimeMinutes = CalculateAverageWaitTime(orgQueues),
                    StaffUtilizationPercentage = CalculateStaffUtilization(orgStaff, orgQueues),
                    TotalLocations = orgLocations.Count,
                    TotalStaffMembers = orgStaff.Count
                };

                metrics.Add(metric);
            }

            return metrics;
        }

        private double GetMetricValue(OrganizationMetrics org, string metricType)
        {
            return metricType.ToLower() switch
            {
                "completedservices" => org.CompletedServices,
                "averagewaittime" => org.AverageWaitTimeMinutes,
                "staffutilization" => org.StaffUtilizationPercentage,
                _ => org.CompletedServices
            };
        }

        private async Task LogAnalyticsAccess(string userId, string analyticsType, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
        {
            var auditLogEntry = new AuditLogEntry
            {
                UserId = userId,
                Action = analyticsType,
                EntityType = "Analytics",
                EntityId = $"{startDate:yyyy-MM-dd}_{endDate:yyyy-MM-dd}",
                TimestampUtc = DateTime.UtcNow
            };

            await _auditLogRepository.LogAsync(auditLogEntry, cancellationToken);
        }
    }
} 