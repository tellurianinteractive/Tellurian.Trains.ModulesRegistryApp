using ModulesRegistry.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ModulesRegistry.Services
{
    public interface IUserService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectId">The Azure AD object ID.</param>
        /// <returns>The database id of the user.</returns>
        Task<User?> FindAsync(string? objectId);
        /// <summary>
        /// Tries to find an existing <see cref="User"/>. If not found, a new <see cref="User"/> is created.
        /// </summary>
        /// <param name="objectId">The Azure AD object ID. This is used to find an existing <see cref="User"/> or is assigned to a new <see cref="User"/>.</param>
        /// <param name="emailAddress">The email used as user identity in Azure AD. This must match the email for an existing <see cref="Person"/></param>
        /// <returns></returns>
        Task<User?> FindOrCreateAsync(string? objectId, string? emailAddress);

        Task<IEnumerable<User>> GetUsersAsync();
        Task<User> AcceptCookieConsent(string? objectId);
    }
}
