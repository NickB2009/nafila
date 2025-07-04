using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Domain.Organizations;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Domain.Staff;
using Grande.Fila.API.Domain.Locations;

namespace Grande.Fila.API.Application.Organizations
{
    /// <summary>
    /// Service for handling UC-TRACKQ: Admin/Owner track live activity use case
    /// </summary>
    public class TrackLiveActivityService
    {
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IQueueRepository _queueRepository;
        private readonly IStaffMemberRepository _staffRepository;
        private readonly ILocationRepository _locationRepository;

        public TrackLiveActivityService(
            IOrganizationRepository organizationRepository,
            IQueueRepository queueRepository,
            IStaffMemberRepository staffRepository,
            ILocationRepository locationRepository)
        {
            _organizationRepository = organizationRepository;
            _queueRepository = queueRepository;
            _staffRepository = staffRepository;
            _locationRepository = locationRepository;
        }

        public async Task<TrackLiveActivityResult> GetLiveActivityAsync(
            TrackLiveActivityRequest request, 
            string userId, 
            string userRole = "Admin", 
            CancellationToken cancellationToken = default)
        {
            var result = new TrackLiveActivityResult
            {
                Success = false,
                FieldErrors = new Dictionary<string, string>(),
                Errors = new List<string>()
            };

            // Authorization check
            if (userRole != "Admin" && userRole != "Owner")
            {
                result.Errors.Add("Forbidden: Only Admin/Owner can track live activity.");
                return result;
            }

            // Validation
            if (string.IsNullOrWhiteSpace(request.OrganizationId))
            {
                result.FieldErrors["OrganizationId"] = "Organization ID is required.";
                return result;
            }

            if (!Guid.TryParse(request.OrganizationId, out var organizationId))
            {
                result.FieldErrors["OrganizationId"] = "Invalid organization ID format.";
                return result;
            }

            try
            {
                // Get organization
                var organization = await _organizationRepository.GetByIdAsync(organizationId, cancellationToken);
                if (organization == null)
                {
                    result.Errors.Add("Organization not found.");
                    return result;
                }

                // Get all locations for the organization
                var locations = await _locationRepository.GetByOrganizationAsync(organizationId, cancellationToken);

                var locationActivities = new List<LocationActivityDto>();
                var organizationSummary = new OrganizationSummaryDto();

                foreach (var location in locations)
                {
                    var locationActivity = await BuildLocationActivityAsync(location, cancellationToken);
                    locationActivities.Add(locationActivity);

                    // Aggregate organization-level metrics
                    organizationSummary.TotalLocations++;
                    organizationSummary.TotalActiveQueues += locationActivity.Queues.Count(q => q.IsActive);
                    organizationSummary.TotalCustomersWaiting += locationActivity.TotalCustomersWaiting;
                    organizationSummary.TotalStaffMembers += locationActivity.TotalStaffMembers;
                    organizationSummary.TotalAvailableStaff += locationActivity.AvailableStaffMembers;
                    organizationSummary.TotalBusyStaff += locationActivity.BusyStaffMembers;
                }

                // Calculate average wait time across all locations
                if (locationActivities.Count > 0)
                {
                    organizationSummary.AverageWaitTimeMinutes = locationActivities
                        .Where(l => l.AverageWaitTimeMinutes > 0)
                        .Average(l => l.AverageWaitTimeMinutes);
                }

                result.LiveActivity = new LiveActivityDto
                {
                    OrganizationId = organization.Id.ToString(),
                    OrganizationName = organization.Name,
                    LastUpdated = DateTime.UtcNow,
                    Summary = organizationSummary,
                    Locations = locationActivities
                };

                result.Success = true;
                return result;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Unable to retrieve live activity: {ex.Message}");
                return result;
            }
        }

        private async Task<LocationActivityDto> BuildLocationActivityAsync(Location location, CancellationToken cancellationToken)
        {
            // Get queues for this location
            var queues = await _queueRepository.GetAllByLocationIdAsync(location.Id, cancellationToken);
            
            // Get staff for this location
            var staff = await _staffRepository.GetActiveStaffMembersAsync(location.Id, cancellationToken);

            var queueActivities = queues.Select(BuildQueueActivity).ToList();
            var staffActivities = staff.Select(BuildStaffActivity).ToList();

            var totalCustomersWaiting = queueActivities.Sum(q => q.CustomersWaiting);
            var availableStaff = staffActivities.Count(s => s.Status == "available");
            var busyStaff = staffActivities.Count(s => s.Status == "busy");
            
            var averageWaitTime = queueActivities.Where(q => q.AverageWaitTimeMinutes > 0)
                                                 .DefaultIfEmpty()
                                                 .Average(q => q?.AverageWaitTimeMinutes ?? 0);

            return new LocationActivityDto
            {
                LocationId = location.Id.ToString(),
                LocationName = location.Name,
                IsOpen = IsLocationOpen(location),
                TotalCustomersWaiting = totalCustomersWaiting,
                TotalStaffMembers = staffActivities.Count,
                AvailableStaffMembers = availableStaff,
                BusyStaffMembers = busyStaff,
                AverageWaitTimeMinutes = averageWaitTime,
                Queues = queueActivities,
                Staff = staffActivities
            };
        }

        private QueueActivityDto BuildQueueActivity(Queue queue)
        {
            var waitingCustomers = queue.Entries.Count(e => e.Status == QueueEntryStatus.Waiting);
            var beingServedCustomers = queue.Entries.Count(e => e.Status == QueueEntryStatus.CheckedIn);
            var totalTodayCustomers = queue.Entries.Count(e => e.EnteredAt.Date == DateTime.Today);
            
            // Simple average wait time calculation based on entries
            var averageWaitTime = CalculateAverageWaitTime(queue);

            return new QueueActivityDto
            {
                QueueId = queue.Id.ToString(),
                QueueDate = queue.QueueDate,
                IsActive = queue.IsActive,
                CustomersWaiting = waitingCustomers,
                CustomersBeingServed = beingServedCustomers,
                TotalCustomersToday = totalTodayCustomers,
                AverageWaitTimeMinutes = averageWaitTime,
                MaxSize = queue.MaxSize
            };
        }

        private StaffActivityDto BuildStaffActivity(StaffMember staff)
        {
            var customersServedToday = CalculateCustomersServedToday(staff);
            var averageServiceTime = CalculateAverageServiceTime(staff);

            return new StaffActivityDto
            {
                StaffId = staff.Id.ToString(),
                StaffName = staff.Name,
                Status = staff.StaffStatus,
                LastActivity = staff.LastModifiedAt,
                CustomersServedToday = customersServedToday,
                AverageServiceTimeMinutes = averageServiceTime,
                IsOnBreak = staff.IsOnBreak(),
                BreakStartTime = staff.GetCurrentBreak()?.StartedAt
            };
        }

        private bool IsLocationOpen(Location location)
        {
            return location.IsOpen();
        }

        private double CalculateAverageWaitTime(Queue queue)
        {
            var completedEntries = queue.Entries.Where(e => e.Status == QueueEntryStatus.Completed).ToList();
            
            if (!completedEntries.Any())
                return 0;

            var totalWaitTime = completedEntries.Sum(e => 
                e.CalledAt.HasValue ? (e.CalledAt.Value - e.EnteredAt).TotalMinutes : 0);

            return totalWaitTime / completedEntries.Count;
        }

        private int CalculateCustomersServedToday(StaffMember staff)
        {
            // This would typically query a service history or queue entries table
            // For now, return a placeholder value
            return 0;
        }

        private double CalculateAverageServiceTime(StaffMember staff)
        {
            // This would typically calculate based on completed services
            // For now, return a placeholder value
            return 0;
        }
    }
}