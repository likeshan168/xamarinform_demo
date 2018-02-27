using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AllocationApp.Models;
using Newtonsoft.Json;

namespace AllocationApp
{
    public class RestService : IRestService
    {
        HttpClient httpClient;

        public RestService()
        {
            httpClient = new HttpClient { MaxResponseContentBufferSize = 256000 };
            //httpClient.DefaultRequestHeaders.Add("httpClient")
            //httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeaderValue);
        }
        public async Task<ServiceResponse> LoginAsync(User user)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            var uri = new Uri(string.Format(Constants.LoginUrl, string.Empty));

            try
            {
                var json = JsonConvert.SerializeObject(user);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(uri, content);
                if (response.IsSuccessStatusCode)
                {
                    serviceResponse = await response.Content.ReadAsAsync<ServiceResponse>();
                    //JavaScriptSerializer JSserializer = new JavaScriptSerializer();
                    //loginResponse = JsonConvert.DeserializeObject<LoginResponse>(result);
                }

                return serviceResponse;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Login Error:{ex.Message}");
                serviceResponse.IsSuccess = false;
                serviceResponse.Result = "登录发生异常";
                return serviceResponse;
            }
        }

        public async Task<GetListResponse<AllocationData>> GetListAsync()
        {
            var listResponse = new GetListResponse<AllocationData>();
            try
            {
                var uri = new Uri(string.Format(Constants.LoginUrl, string.Empty));
                httpClient.MaxResponseContentBufferSize = 25600000000;
                var response = await httpClient.GetAsync(uri);
                listResponse = await response.Content.ReadAsAsync<GetListResponse<AllocationData>>();
                
                return listResponse;
            }
            catch (Exception ex)
            {
                listResponse.IsSuccess = false;
                listResponse.Message = "解析数据出现异常";
                return listResponse;
            }
        }

        public async Task<ServiceResponse> UpdateDataAsync(IList<AllocationData> allocations)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            var uri = new Uri(string.Format(Constants.LoginUrl, string.Empty));

            try
            {
                var json = JsonConvert.SerializeObject(allocations);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PutAsync(uri, content);
                if (response.IsSuccessStatusCode)
                {
                    serviceResponse = await response.Content.ReadAsAsync<ServiceResponse>();
                }

                return serviceResponse;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Login Error:{ex.Message}");
                serviceResponse.IsSuccess = false;
                serviceResponse.Result = "更新发生异常";
                return serviceResponse;
            }
        }
    }
}
;