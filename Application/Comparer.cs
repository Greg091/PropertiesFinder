using Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Application.Sample
{
    public class Comparer : IEqualityComparer<Entry>
    {
        public bool Equals(Entry x, Entry y)
        {
            if (GetHashCode(x) == GetHashCode(y))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int GetHashCode([DisallowNull] Entry obj)
        {
            var hashCode = $"{obj.PropertyDetails.NumberOfRooms}" +
                $"{obj.PropertyAddress.City}" +
                $"{obj.PropertyAddress.StreetName}" +
                $"{obj.PropertyDetails.Area}" +
                $"{obj.PropertyPrice.TotalGrossPrice}";
            return hashCode.GetHashCode();
        }
    }
}
