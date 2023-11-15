using System.Text;

namespace libhyperion
{
    public class HttpUtils
    {
        private readonly HttpClient _httpClient;

        public HttpUtils()
        {
            _httpClient = new HttpClient {};
        }

        // GET 请求，可自定义请求头
        public async Task<string> GetAsync(string endpoint, Dictionary<string, string> customHeaders = null)
        {
            if (customHeaders != null)
            {
                foreach (var header in customHeaders)
                {
                    _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }

            HttpResponseMessage response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        // POST 请求，可自定义请求头，支持 JSON 内容
        public async Task<string> PostJsonAsync(string endpoint, string jsonContent, Dictionary<string, string> customHeaders = null)
        {
            HttpContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            if (customHeaders != null)
            {
                foreach (var header in customHeaders)
                {
                    content.Headers.Add(header.Key, header.Value);
                }
            }

            HttpResponseMessage response = await _httpClient.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        // POST 请求，支持表单数据
        public async Task<string> PostFormAsync(string endpoint, Dictionary<string, string> formData, Dictionary<string, string> customHeaders = null)
        {
            var formContent = new FormUrlEncodedContent(formData);

            if (customHeaders != null)
            {
                foreach (var header in customHeaders)
                {
                    formContent.Headers.Add(header.Key, header.Value);
                }
            }

            HttpResponseMessage response = await _httpClient.PostAsync(endpoint, formContent);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }

    // class Program
    // {
    //     static async Task Main(string[] args)
    //     {
    //         string baseAddress = "https://jsonplaceholder.typicode.com"; // 示例基地址
    //         var client = new HttpRequestWrapper(baseAddress);
    //
    //         // 示例：使用自定义请求头
    //         var customHeaders = new Dictionary<string, string>
    //         {
    //             { "Authorization", "Bearer your_access_token" },
    //             { "User-Agent", "MyCustomUserAgent" }
    //         };
    //
    //         // 示例：表单数据
    //         var formData = new Dictionary<string, string>
    //         {
    //             { "username", "myUsername" },
    //             { "password", "myPassword" }
    //             // Add more key-value pairs as needed
    //         };
    //
    //         string formEndpoint = "login";
    //         string formResult = await client.PostFormAsync(formEndpoint, formData, customHeaders);
    //         Console.WriteLine($"Form response:\n{formResult}");
    //     }
    // }
}
