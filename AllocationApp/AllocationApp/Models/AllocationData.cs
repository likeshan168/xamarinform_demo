﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Realms;

namespace AllocationApp.Models
{
    public class AllocationData 
    {
        public Int64 Id { get; set; }
        public StateKind IsChecked { get; set; }
        //public String TenantName { get; set; }
        public String Flight { get; set; }
        public String MasterAwb { get; set; }
        public String SubAwb { get; set; }
        public Int32 Amount { get; set; }
        public Double Weight { get; set; }
        public String Description { get; set; }
        public String Status { get; set; }
        public int TenantId { get; set; }
    }

    public class AllocationData2 : RealmObject
    {
        public Int64 Id { get; set; }
        public int IsChecked { get; set; }
        //public String TenantName { get; set; }
        public String Flight { get; set; }
        public String MasterAwb { get; set; }
        public String SubAwb { get; set; }
        public Int32 Amount { get; set; }
        public Double Weight { get; set; }
        public String Description { get; set; }
        public String Status { get; set; }

        public int TenantId { get; set; }
    }
}
