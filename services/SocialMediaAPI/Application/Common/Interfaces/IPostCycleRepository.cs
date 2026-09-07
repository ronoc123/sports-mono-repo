using Domain.PostCycle;
using SportifyCore.Domain;

namespace Application.Common.Interfaces;

public interface IPostCycleRepository : IRepository<PostCycleJob, string>
{
    Task<List<PostCycleJob>> GetStaleRunningJobsAsync(
        DateTime olderThan,
        CancellationToken cancellationToken = default);
}
