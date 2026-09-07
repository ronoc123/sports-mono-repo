using SportifyCore.Domain;

namespace Application.Common.Interfaces;

public interface IPostRecordRepository : IRepository<Domain.Records.PostRecord, string>
{
    Task<List<Domain.Records.PostRecord>> GetRecentByChannelIdAsync(
        string channelId,
        int count,
        CancellationToken cancellationToken = default);

    Task<(List<Domain.Records.PostRecord> Records, long TotalCount)> GetPagedByChannelIdAsync(
        string channelId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
