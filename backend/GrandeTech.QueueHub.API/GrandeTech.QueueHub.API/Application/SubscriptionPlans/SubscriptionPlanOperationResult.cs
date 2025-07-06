using System;
using System.Collections.Generic;

namespace Grande.Fila.API.Application.SubscriptionPlans
{
    public class SubscriptionPlanOperationResult
    {
        public bool Success { get; set; }
        public Dictionary<string, string> FieldErrors { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }
} 