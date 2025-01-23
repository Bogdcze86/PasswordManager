using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PasswordManager
{
    using System.Text.Json.Serialization;

    public class PasswordEntry
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("site")]
        public string Site { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }

        [JsonPropertyName("key_id")]
        public string KeyId { get; set; }
    }


    public class ApiClient
    {
        private static readonly HttpClient client = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };

        public async Task<string> AddPassword(string site, string username, string encryptedPassword, string keyId)
        {
            var json = JsonSerializer.Serialize(new
            {
                site,
                username,
                password = encryptedPassword,
                key_id = keyId
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/password", content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }


        public async Task<List<PasswordEntry>> GetPasswordsAsync()
        {
            var response = await client.GetAsync("/passwords");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<PasswordEntry>>(json);
        }

    }
}