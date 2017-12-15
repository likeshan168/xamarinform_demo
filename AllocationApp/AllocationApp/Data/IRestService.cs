using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AllocationApp.Models;

namespace AllocationApp
{
    public interface IRestService
    {
        Task<ServiceResponse> LoginAsync(User user);

        Task<GetListResponse<AllocationData>> GetListAsync();

        Task<ServiceResponse> UpdateDataAsync(IList<AllocationData> allocations);
    }
}
