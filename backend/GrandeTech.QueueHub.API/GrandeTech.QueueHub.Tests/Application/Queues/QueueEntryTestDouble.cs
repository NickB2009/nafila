using System;
using Grande.Fila.API.Domain.Queues;

namespace Grande.Fila.API.Tests.Application.Queues
{
    public class QueueEntryTestDouble : QueueEntry
    {
        private bool _completeThrowsException = false;
        private bool _checkInThrowsException = false;

        public QueueEntryTestDouble(Guid queueId, Guid customerId, string customerName, int position)
            : base(queueId, customerId, customerName, position)
        {
        }

        public QueueEntryTestDouble(Guid queueId, Guid customerId, string customerName, int position, QueueEntryStatus status)
            : base(queueId, customerId, customerName, position)
        {
            SetStatusForTest(status);
        }

        public void SetStatusForTest(QueueEntryStatus status)
        {
            var statusField = typeof(QueueEntry).GetField("<Status>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            statusField?.SetValue(this, status);
        }

        public void SetCompleteThrowsException(bool throws)
        {
            _completeThrowsException = throws;
        }

        public void SetCheckInThrowsException(bool throws)
        {
            _checkInThrowsException = throws;
        }

        public new void Complete(int serviceDurationMinutes)
        {
            if (_completeThrowsException)
            {
                throw new InvalidOperationException($"Cannot complete service for a customer with status {Status}");
            }
            base.Complete(serviceDurationMinutes);
        }

        public new void CheckIn()
        {
            if (_checkInThrowsException)
            {
                throw new InvalidOperationException("CheckIn operation failed");
            }
            base.CheckIn();
        }
    }
} 