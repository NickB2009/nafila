using System;
using System.Collections.Generic;
using System.Linq;

namespace GrandeTech.QueueHub.API.Domain.Common
{
    /// <summary>
    /// Base record for all value objects providing basic equality behavior
    /// </summary>
    public abstract record ValueObject
    {
        protected abstract IEnumerable<object> GetEqualityComponents();

        public override int GetHashCode()
        {
            return GetEqualityComponents()
                .Select(x => x != null ? x.GetHashCode() : 0)
                .Aggregate((x, y) => x ^ y);
        }

        public virtual bool Equals(ValueObject? other)
        {
            if (other == null || GetType() != other.GetType())
            {
                return false;
            }

            return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
        }
    }
}
