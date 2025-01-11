using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;

namespace MultiTenantStripeAPI.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CacheController : ControllerBase
    {
        private readonly IMemoryCache _cache;

        public CacheController(IMemoryCache cache)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        /// <summary>
        /// Get all keys currently in the cache.
        /// </summary>
        [HttpGet("all")]
        public IActionResult GetAllCaches()
        {
            // IMemoryCache does not provide a direct way to list keys
            // Use a convention to track keys or implement a wrapper.
            if (_cache.TryGetValue("CacheKeys", out List<string> keys))
            {
                return Ok(keys);
            }

            return Ok(new List<string>()); // No keys found
        }

        /// <summary>
        /// Get the value of a specific cache by key.
        /// </summary>
        /// <param name="key">The cache key.</param>
        [HttpGet("{key}")]
        public IActionResult GetCacheByKey(string key)
        {
            if (_cache.TryGetValue(key, out object value))
            {
                return Ok(value);
            }

            return NotFound($"Cache with key '{key}' not found.");
        }

        /// <summary>
        /// Set a cache entry with a key-value pair.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The value to store in the cache.</param>
        /// <param name="expirationMinutes">The expiration time in minutes (default: 10).</param>
        [HttpPost("set")]
        public IActionResult SetCache(string key, string value, int expirationMinutes = 10)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
            {
                return BadRequest("Key and value must be provided.");
            }

            // Track the cache key for listing all caches
            if (_cache.TryGetValue("CacheKeys", out List<string> keys))
            {
                if (!keys.Contains(key))
                {
                    keys.Add(key);
                }
            }
            else
            {
                keys = new List<string> { key };
            }

            _cache.Set("CacheKeys", keys, new MemoryCacheEntryOptions
            {
                Size = 1 // Specify the size of the cache entry
            });

            // Set the cache value with expiration
            _cache.Set(key, value, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expirationMinutes),
                Priority = CacheItemPriority.Normal,
                Size = 1 // Specify the size of the cache entry
            });

            return Ok($"Cache set for key '{key}' with expiration in {expirationMinutes} minutes.");
        }
    }
}
