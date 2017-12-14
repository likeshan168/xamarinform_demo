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
        public async Task<LoginResponse> LoginAsync(User user)
        {
            LoginResponse loginResponse = new LoginResponse();
            var uri = new Uri(string.Format(Constants.LoginUrl, string.Empty));

            try
            {
                var json = JsonConvert.SerializeObject(user);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(uri, content);
                if (response.IsSuccessStatusCode)
                {
                    loginResponse = await response.Content.ReadAsAsync<LoginResponse>();
                }
                else
                {
                    loginResponse = await response.Content.ReadAsAsync<LoginResponse>();
                }

                return loginResponse;
            }
            catch (HttpRequestException ex)
            {
                loginResponse.IsSuccess = false;
                loginResponse.Result = "网络发生异常，请检查网络是否正常";
            }
            catch (Exception ex)
            {
                //Debug.WriteLine($"Login Error:{ex.Message}");
                loginResponse.IsSuccess = false;
                loginResponse.Result = "登录发生异常";

            }
            return loginResponse;
        }

        public async Task<GetListResponse<AllocationData>> GetListAsync()
        {
            var list = new GetListResponse<AllocationData>();
            try
            {
                var uri = new Uri(string.Format(Constants.LoginUrl, string.Empty));
                var response = await httpClient.GetAsync(uri);
                return await response.Content.ReadAsAsync<GetListResponse<AllocationData>>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Login Error:{ex.Message}");
                list.IsSuccess = false;
                list.Message = "解析服务器中的数据出现异常";
                return list;
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