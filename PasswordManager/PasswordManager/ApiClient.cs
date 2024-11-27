using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PasswordManager
{
    public class ApiClient
    {
        private static readonly HttpClient client = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };

        public async Task<string> AddPassword(string site, string username, string password)
        {
            var json = JsonSerializer.Serialize(new { site, username, password });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/password", content);
            return await response.Content.ReadAsStringAsync();
        }
    }
}