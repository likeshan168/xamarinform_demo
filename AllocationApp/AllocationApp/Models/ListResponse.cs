﻿using System.Collections.Generic;

namespace AllocationApp.Models
{
    public class GetListResponse<T> where T : class
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public IEnumerable<T> Entities { get; set; }
    }
}
