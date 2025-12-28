using Application.Common.Interfaces;
using Application.Common.Models;
using System.Net.Http.Json;

namespace Infrastructure.Integrations.Identity
{
    public class UserDirectoryClient : IUserDirectory
    {
        private readonly HttpClient _httpClient;

        public UserDirectoryClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<UserInfo> GetUserByIdAsync(string userId, CancellationToken ct)
        {
            var response = await _httpClient.GetAsync(
                $"api/auth/get?userId={userId}",
                ct
            );


            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Unable to retrieve user {userId}");
            }

            return await response.Content.ReadFromJsonAsync<UserInfo>(ct)
                   ?? throw new InvalidOperationException("User response was null");
        }

    }
}
