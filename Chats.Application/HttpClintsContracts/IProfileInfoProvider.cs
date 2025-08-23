using Chats.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chats.Application.HttpClintsContracts
{
    public interface IProfileInfoProvider
    {
        Task<Result<IEnumerable<ProfileDTO>>> GetProfilesInfoAsync(IEnumerable<Guid> usersId);
    }
}
