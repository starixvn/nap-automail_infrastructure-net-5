using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Fanex.MailDeliver.Utility
{
    public static class WebApiClientService
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly MediaTypeWithQualityHeaderValue media = new MediaTypeWithQualityHeaderValue("application/json");
        public static string AppMailAddress { get; set; }

        public static T PostAsync<T, P>(string action, P inputParams)
        {
            string rootUrl = NormalizeBaseUrl(AppMailAddress);
            if (client.BaseAddress == null)
            {
                client.BaseAddress = new Uri(rootUrl);
            }

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(media);

            using (var content = new StringContent(JsonConvert.SerializeObject(inputParams), Encoding.UTF8, "application/json"))
            {
                var responseResult = client.PostAsync(action, content).Result;
                if (responseResult.IsSuccessStatusCode)
                {
                    var data = responseResult.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<T>(data.Result);
                }
            }

            return default(T);
        }

        private static string NormalizeBaseUrl(string url)
        {
            return url.EndsWith("/") ? url : url + "/";
        }
    }
}