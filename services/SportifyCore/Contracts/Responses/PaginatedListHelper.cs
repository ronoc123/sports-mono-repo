using Contracts.Responses;

public static class PaginatedListFactory
{
  public static PaginatedList<T> Create<T>(IQueryable<T> source, int pageNumber, int pageSize)
  {
    var count = source.Count();
    var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

    return new PaginatedList<T>(items, count, pageNumber, pageSize);
  }

  public static async Task<PaginatedList<T>> CreateAsync<T>(IQueryable<T> source, int pageNumber, int pageSize)
  {
    var count = await Task.Run(() => source.Count());
    var items = await Task.Run(() => source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList());

    return new PaginatedList<T>(items, count, pageNumber, pageSize);
  }

  public static PaginatedList<T> Create<T>(List<T> source, int pageNumber, int pageSize)
  {
    var count = source.Count;
    var items = source
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToList();

    return new PaginatedList<T>(items, count, pageNumber, pageSize);
  }

}
