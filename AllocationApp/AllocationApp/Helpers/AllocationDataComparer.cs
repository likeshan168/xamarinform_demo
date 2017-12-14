using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AllocationApp.Models;

namespace AllocationApp.Helpers
{
    public class AllocationDataComparer : IEqualityComparer<AllocationData>
    {
        public bool Equals(AllocationData x, AllocationData y)
        {
            return x.MasterAwb == y.MasterAwb;
        }

        public int GetHashCode(AllocationData obj)
        {
            if (ReferenceEquals(obj, null)) return 0;
            return obj.MasterAwb.GetHashCode();
        }
    }
}
