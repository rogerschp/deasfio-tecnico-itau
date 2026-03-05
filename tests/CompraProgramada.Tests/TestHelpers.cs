namespace CompraProgramada.Tests;
internal static class AsyncEnumerableHelpers
{
    public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(IEnumerable<T> source)
    {
        await Task.CompletedTask;
        foreach (var item in source)
            yield return item;
    }
}
