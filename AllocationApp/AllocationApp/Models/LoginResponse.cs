﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllocationApp.Models
{
    public class LoginResponse : ServiceResponse
    {
        public int TenantId { get; set; }
        public bool IsAdmin { get; set; }
    }
}
