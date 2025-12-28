using Application.Common.Models;

namespace Application.Common.Interfaces
{
    public interface IUserDirectory
    {
        Task<UserInfo> GetUserByIdAsync(string userId, CancellationToken ct);
    }
}
