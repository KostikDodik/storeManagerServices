using Microsoft.Extensions.Caching.Memory;

namespace Data;

// Create a static or instance MemoryCache
internal static class Cache
{
    private static readonly MemoryCacheEntryOptions Options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromDays(1));
    private static MemoryCache InternalCache = new MemoryCache(new MemoryCacheOptions());
    
    /// <summary>
    /// Sets a value in the cache with the default options
    /// </summary>
    /// <typeparam name="T">Type of the value to cache</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="value">Value to cache</param>
    public static T Set<T>(string key, T value)
    {
        return InternalCache.Set(key, value, Options);
    }
    
    /// <summary>
    /// Gets a value from the cache
    /// </summary>
    /// <typeparam name="T">Type of the value to retrieve</typeparam>
    /// <param name="key">Cache key</param>
    /// <returns>The cached value or default(T) if not found</returns>
    public static T Get<T>(string key)
    {
        return InternalCache.TryGetValue(key, out var value) ? (T)value : default;
    }
    
    /// <summary>
    /// Gets a value from the cache or creates and adds it if not found
    /// </summary>
    /// <typeparam name="T">Type of the value to retrieve</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="factory">Function to create the value if not found</param>
    /// <returns>The cached or newly created value</returns>
    public static T GetOrCreate<T>(string key, Func<T> factory)
    {
        return InternalCache.GetOrCreate(key, entry =>
        {
            entry.SetOptions(Options);
            return factory();
        });
    }
    
    /// <summary>
    /// Removes a value from the cache
    /// </summary>
    /// <param name="key">Cache key</param>
    public static void Remove(string key)
    {
        InternalCache.Remove(key);
    }
    
    /// <summary>
    /// Clears all entries from the cache
    /// </summary>
    public static void Clear()
    {
        InternalCache.Dispose();
        // Recreate the cache
        var newCache = new MemoryCache(new MemoryCacheOptions());
        Interlocked.Exchange(ref InternalCache, newCache);
    }
}