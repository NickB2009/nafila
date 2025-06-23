using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Domain.Staff;

namespace Grande.Fila.API.Application.Services
{
    public class EstimatedWaitTimeService
    {
        private readonly IQueueRepository _queueRepository;
        private readonly IStaffMemberRepository _staffMemberRepository;

        public EstimatedWaitTimeService(
            IQueueRepository queueRepository,
            IStaffMemberRepository staffMemberRepository)
        {
            _queueRepository = queueRepository;
            _staffMemberRepository = staffMemberRepository;
        }

        public async Task<int> CalculateAsync(Guid queueId, Guid entryId, CancellationToken cancellationToken = default)
        {
            var queue = await _queueRepository.GetByIdAsync(queueId, cancellationToken);
            if (queue == null)
                return -1;

            var entry = queue.Entries.FirstOrDefault(e => e.Id == entryId);
            if (entry == null)
                return -1;

            // Get all active staff members for this location
            var staffMembers = await _staffMemberRepository.GetByLocationAsync(queue.LocationId, cancellationToken);
            var activeStaffCount = staffMembers.Count(s => s.IsActive);

            if (activeStaffCount == 0)
                return -1; // No staff available

            // Calculate average service time (default to 30 minutes if no data available)
            var averageServiceTimeMinutes = 30;

            // Count customers ahead in the queue
            var customersAhead = queue.Entries
                .Where(e => e.Status == QueueEntryStatus.Waiting || e.Status == QueueEntryStatus.Called)
                .Where(e => e.Position < entry.Position)
                .Count();

            // Calculate estimated wait time
            var estimatedWaitTime = (customersAhead / activeStaffCount) * averageServiceTimeMinutes;

            return Math.Max(0, estimatedWaitTime);
        }
    }
} 