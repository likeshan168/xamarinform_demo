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
        Task<LoginResponse> LoginAsync(User user);

        Task<List<AllocationData>> GetListAsync();
    }
}
