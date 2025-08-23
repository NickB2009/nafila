using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Grande.Fila.API.Application.Queues.Models
{
    /// <summary>
    /// Request to transfer a queue entry to another salon, service, or time slot
    /// </summary>
    public class QueueTransferRequest
    {
        [Required]
        public string QueueEntryId { get; set; } = string.Empty;

        [Required]
        public string TransferType { get; set; } = string.Empty; // "salon", "service", "time"

        // For salon transfers
        public string? TargetSalonId { get; set; }

        // For service transfers
        public string? TargetServiceType { get; set; }

        // For time transfers
        public DateTime? PreferredTime { get; set; }

        // Optional reason for transfer
        public string? Reason { get; set; }

        // Whether to keep current position or go to end of new queue
        public bool MaintainPosition { get; set; } = false;
    }

    /// <summary>
    /// Response for queue transfer operation
    /// </summary>
    public class QueueTransferResponse
    {
        public bool Success { get; set; }
        public string? NewQueueEntryId { get; set; }
        public string? NewSalonId { get; set; }
        public string? NewSalonName { get; set; }
        public int NewPosition { get; set; }
        public int EstimatedWaitMinutes { get; set; }
        public DateTime TransferredAt { get; set; }
        public string? TransferType { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }

    /// <summary>
    /// Transfer suggestion for better queue experience
    /// </summary>
    public class QueueTransferSuggestion
    {
        public string SuggestionId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "salon", "service", "time"
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string TargetSalonId { get; set; } = string.Empty;
        public string TargetSalonName { get; set; } = string.Empty;
        public int EstimatedWaitMinutes { get; set; }
        public int PositionImprovement { get; set; } // How many positions better
        public int TimeImprovement { get; set; } // How many minutes saved
        public double DistanceKm { get; set; }
        public string Priority { get; set; } = "medium"; // "high", "medium", "low"
        public double ConfidenceScore { get; set; } // 0.0 to 1.0
        public DateTime ValidUntil { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Request for transfer suggestions
    /// </summary>
    public class TransferSuggestionsRequest
    {
        [Required]
        public string QueueEntryId { get; set; } = string.Empty;

        // Optional filters
        public double? MaxDistanceKm { get; set; }
        public int? MaxWaitTimeMinutes { get; set; }
        public List<string>? PreferredServiceTypes { get; set; }
        public bool IncludeSalonTransfers { get; set; } = true;
        public bool IncludeServiceTransfers { get; set; } = true;
        public bool IncludeTimeTransfers { get; set; } = false; // Future feature
    }

    /// <summary>
    /// Response with transfer suggestions
    /// </summary>
    public class TransferSuggestionsResponse
    {
        public bool Success { get; set; }
        public List<QueueTransferSuggestion> Suggestions { get; set; } = new();
        public string CurrentSalonId { get; set; } = string.Empty;
        public string CurrentSalonName { get; set; } = string.Empty;
        public int CurrentPosition { get; set; }
        public int CurrentEstimatedWaitMinutes { get; set; }
        public DateTime GeneratedAt { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    /// <summary>
    /// Transfer analytics for tracking success rates
    /// </summary>
    public class QueueTransferAnalytics
    {
        public string TransferId { get; set; } = string.Empty;
        public string OriginalQueueEntryId { get; set; } = string.Empty;
        public string NewQueueEntryId { get; set; } = string.Empty;
        public string TransferType { get; set; } = string.Empty;
        public string OriginalSalonId { get; set; } = string.Empty;
        public string TargetSalonId { get; set; } = string.Empty;
        public int OriginalPosition { get; set; }
        public int NewPosition { get; set; }
        public int OriginalWaitMinutes { get; set; }
        public int NewWaitMinutes { get; set; }
        public DateTime TransferredAt { get; set; }
        public DateTime? ServiceCompletedAt { get; set; }
        public bool WasSuccessful { get; set; }
        public string? Reason { get; set; }
        public int CustomerSatisfactionRating { get; set; } // 1-5
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Transfer eligibility check
    /// </summary>
    public class TransferEligibilityRequest
    {
        [Required]
        public string QueueEntryId { get; set; } = string.Empty;
        
        [Required]
        public string TransferType { get; set; } = string.Empty;
        
        public string? TargetSalonId { get; set; }
        public string? TargetServiceType { get; set; }
    }

    /// <summary>
    /// Transfer eligibility response
    /// </summary>
    public class TransferEligibilityResponse
    {
        public bool IsEligible { get; set; }
        public List<string> Requirements { get; set; } = new();
        public List<string> Restrictions { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public int? EstimatedNewPosition { get; set; }
        public int? EstimatedNewWaitMinutes { get; set; }
        public double? TransferFee { get; set; } // Future: transfer fees
        public DateTime? EarliestTransferTime { get; set; }
        public DateTime? LatestTransferTime { get; set; }
    }

    /// <summary>
    /// Bulk transfer request for multiple queue entries
    /// </summary>
    public class BulkTransferRequest
    {
        [Required]
        public List<string> QueueEntryIds { get; set; } = new();
        
        [Required]
        public string TargetSalonId { get; set; } = string.Empty;
        
        public string? Reason { get; set; }
        public bool MaintainRelativePositions { get; set; } = true;
    }

    /// <summary>
    /// Bulk transfer response
    /// </summary>
    public class BulkTransferResponse
    {
        public bool Success { get; set; }
        public int SuccessfulTransfers { get; set; }
        public int FailedTransfers { get; set; }
        public List<QueueTransferResponse> TransferResults { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public DateTime ProcessedAt { get; set; }
    }
}
