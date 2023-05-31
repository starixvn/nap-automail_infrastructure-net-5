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
            var rootUrl = NormalizeBaseUrl(AppMailAddress);
            if (client.BaseAddress == null)
            {
                client.BaseAddress = new Uri(rootUrl);
            }

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(media);

            using (var content = new StringContent(JsonConvert.SerializeObject(inputParams, Formatting.Indented, new MemoryStreamJsonConverter()), Encoding.UTF8, "application/json"))
            {
                var responseResult = client.PostAsync(action, content).GetAwaiter().GetResult();
                if (responseResult.IsSuccessStatusCode)
                {
                    var data = responseResult.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<T>(data.Result, new MemoryStreamJsonConverter());
                }

                var message = $"{responseResult.StatusCode} - {responseResult.ReasonPhrase} - {rootUrl}";
                throw new Exception(message);
            }
        }

        private static string NormalizeBaseUrl(string url)
        {
            return url.EndsWith("/") ? url : url + "/";
        }
    }
}