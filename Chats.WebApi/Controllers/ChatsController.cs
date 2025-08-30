using Chats.Application;
using Chats.Application.DTOs;
using Chats.Application.HttpClintsContracts;
using Chats.Application.RepositoryContracts;
using Microsoft.AspNetCore.Mvc;

namespace Chats.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatsController : ControllerBase
    {
        private readonly IProfileInfoProvider _profileInfoProvider;
        private readonly IChatRepository _chatsRepository;
        private readonly ILogger<ChatsController> _logger;
        public ChatsController(IProfileInfoProvider profileInfoProvider, ILogger<ChatsController> logger, IChatRepository chatRepository)
        {
            _profileInfoProvider = profileInfoProvider;
            _logger = logger;
            _chatsRepository = chatRepository;
        }

        [HttpGet("getChats/{userID}")]
        public async Task<ActionResult<IEnumerable<(Guid, ProfileDTO)>?>> GetChatsByUserID(Guid userID)
        {
            if(userID == Guid.Empty || userID == null)
            {
                _logger.LogError("Invalid user ID provided: {userID}", userID);
                return BadRequest("Invalid user id");
            }

            Result<IEnumerable<(Guid, Guid)>> getChatsResult = await _chatsRepository.FindCompanionsChatsByUserAsync(userID);
            
            if(!getChatsResult.IsSuccess)
            {
                _logger.LogError("Failed to retrieve chats for user ID: {userID}, Error: {Error}", userID, getChatsResult.ErrorMessage);
                return StatusCode(getChatsResult.StatusCode, getChatsResult.ErrorMessage);
            }

            if(getChatsResult.StatusCode == 404)
            {
                _logger.LogError("Chats not found.");
                return NotFound("Chats not found");
            }

            Result<IEnumerable<ProfileDTO>> getProfileInfoResponse = await _profileInfoProvider.GetProfilesInfoAsync(getChatsResult.Value.Select(x => x.Item2));

            if (!getProfileInfoResponse.IsSuccess)
            {
                _logger.LogError("Failed to retrieve profile information for user ID: {userID}, Error: {Error}", userID, getProfileInfoResponse.ErrorMessage);
                return StatusCode(getProfileInfoResponse.StatusCode, getProfileInfoResponse.ErrorMessage);
            }

            if(getProfileInfoResponse.StatusCode == 400)
            {
                _logger.LogError("Incorrect users id");
                return BadRequest("Incorrect users id");
            }

            var companions = getChatsResult.Value.ToList();
            var profiles = getProfileInfoResponse.Value.ToList();

            var result = companions
                .Join(
                    profiles,
                    c => c.Item2,
                    p => p.AccountID,
                    (c, p) => new { ChatID = c.Item1, Profile = p }
                )
                .ToList();

            return Ok(result);
        }

    }
}
