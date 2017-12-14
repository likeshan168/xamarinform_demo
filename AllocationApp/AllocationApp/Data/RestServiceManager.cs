using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AllocationApp.Models;

namespace AllocationApp
{
    public class RestServiceManager
    {
        private IRestService restService;

        public RestServiceManager(IRestService service)
        {
            restService = service;
        }

        public Task<LoginResponse> LoginAsync(User user)
        {
            return restService.LoginAsync(user);
        }

        public Task<GetListResponse<AllocationData>> GetListAsync()
        {
            return restService.GetListAsync();
        }

        public Task<ServiceResponse> UpdateDataAsync(IList<AllocationData> allocations)
        {
            return restService.UpdateDataAsync(allocations);
        }
    }
}
