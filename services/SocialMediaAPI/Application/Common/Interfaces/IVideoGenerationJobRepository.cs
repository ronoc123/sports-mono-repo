using Domain.VideoGenerationJob;
using SportifyCore.Domain;

namespace Application.Common.Interfaces;

public interface IVideoGenerationJobRepository : IRepository<VideoGenerationJob, string>
{
    Task<List<VideoGenerationJob>> GetStaleJobsAsync(
        DateTime olderThan,
        CancellationToken cancellationToken = default);
}
