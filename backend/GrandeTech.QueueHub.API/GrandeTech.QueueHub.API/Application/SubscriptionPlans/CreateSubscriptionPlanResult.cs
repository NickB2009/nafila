using System;
using System.Collections.Generic;

namespace Grande.Fila.API.Application.SubscriptionPlans
{
    public class CreateSubscriptionPlanResult
    {
        public bool Success { get; set; }
        public string SubscriptionPlanId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Dictionary<string, string> FieldErrors { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }
} 