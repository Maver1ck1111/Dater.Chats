using Amazon.Runtime.Internal.Util;
using Chats.Application;
using Chats.Application.DTOs;
using Chats.Application.HttpClintsContracts;
using Microsoft.Extensions.Logging;
using System.Text.Json;


namespace Chats.Infrastructure
{
    public class ProfileInfoProvider : IProfileInfoProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProfileInfoProvider> _logger;

        public ProfileInfoProvider(HttpClient httpClient, ILogger<ProfileInfoProvider> logger)
        {
            _httpClient = httpClient;
        }

        public async Task<Result<IEnumerable<ProfileDTO>>> GetProfilesInfoAsync(IEnumerable<Guid> usersId)
        {
            List<ProfileDTO> profiles = new List<ProfileDTO>();
                 
            foreach (var id in usersId)
            {
                var result = await _httpClient.GetAsync($"profile/{id}");

                if (!result.IsSuccessStatusCode)
                {
                    _logger.LogError("Can not find profile with {id}", id);
                    continue;
                }
                else
                {
                    var body = await result.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(body))
                    {
                        _logger.LogError("Profile with {id} has empty body", id);
                        continue;
                    }

                    var profileResponse = JsonSerializer.Deserialize<ProfileDTO>(body);

                    profiles.Add(new ProfileDTO
                    {
                        AccountID = profileResponse.AccountID,
                        Name = profileResponse.Name
                    });
                }
            }

            _logger.LogInformation("Profiles info retrieved successfully for {Count} users", profiles.Count);
            return Result<IEnumerable<ProfileDTO>>.Success(profiles);
        }
    }
}
