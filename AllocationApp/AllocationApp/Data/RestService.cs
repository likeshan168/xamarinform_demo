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
            httpClient = new HttpClient();
            httpClient.MaxResponseContentBufferSize = 256000;
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
                    //JavaScriptSerializer JSserializer = new JavaScriptSerializer();
                    //loginResponse = JsonConvert.DeserializeObject<LoginResponse>(result);
                }

                return loginResponse;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Login Error:{ex.Message}");
                loginResponse.IsSuccess = false;
                loginResponse.Result = "登录发生异常";
                return loginResponse;
            }
        }

        public async Task<List<AllocationData>> GetListAsync()
        {
            var list = new List<AllocationData>();
            try
            {
                var uri = new Uri(string.Format(Constants.LoginUrl, string.Empty));
                var response = await httpClient.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    list = await response.Content.ReadAsAsync<List<AllocationData>>();
                    return list;
                }
                return list;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Login Error:{ex.Message}");
                return list;
            }
        }
    }
}
;