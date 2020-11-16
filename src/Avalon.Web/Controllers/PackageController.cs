using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Avalon.Common.Interfaces;
using Avalon.Common.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.Data.Sqlite;
using Dapper;
using System.Text;
using Argus.Extensions;

namespace Avalon.Web.Controllers
{
    [Route("api/package")]
    public class PackageController : ControllerBase
    {
        private readonly ILogger<PackageController> _logger;

        private readonly IMemoryCache _cache;

        private readonly IWebHostEnvironment _webHostEnvironment;

        private List<IPackage> PackgeList { get; set; }

        private List<IPackage> PacakgeListMetadata { get; set; }

        public PackageController(ILogger<PackageController> logger, IMemoryCache cache, IWebHostEnvironment env)
        {
            _logger = logger;
            _cache = cache;
            _webHostEnvironment = env;
        }

        /// <summary>
        /// Returns the latest version of a package.
        /// </summary>
        /// <param name="id">The unique ID of the package.</param>
        [HttpGet("get")]
        public async Task<IPackage> Get([FromQuery] string id)
        {
            await RequestIncrement();
            this.LoadCache();
            return this.PackgeList.Find(x => x.Id.Equals(id, StringComparison.Ordinal)) ?? new Package();
        }

        /// <summary>
        /// Returns the latest version of all of the packages for a given game as specified by the IP address.
        /// </summary>
        /// <param name="ip"></param>
        [HttpGet("get-all")]
        public async Task<IEnumerable<IPackage>> GetAll([FromQuery] string ip)
        {
            await RequestIncrement();
            this.LoadCache();
            return this.PackgeList.FindAll(x => x.GameAddress.Equals(ip, StringComparison.Ordinal) || x.GameAddress.IsNullOrEmptyOrWhiteSpace()) ?? new List<IPackage>();
        }

        /// <summary>
        /// Returns all packages but only the metadata for the Package Manager search screen.
        /// </summary>
        /// <param name="ip"></param>
        [HttpGet("get-all-metadata")]
        public async Task<IEnumerable<IPackage>> GetAllMetadata([FromQuery] string ip)
        {
            await RequestIncrement();
            this.LoadCache();
            return this.PacakgeListMetadata.FindAll(x => x.GameAddress.Equals(ip, StringComparison.Ordinal) || x.GameAddress.IsNullOrEmptyOrWhiteSpace()) ?? new List<IPackage>();
        }

        [HttpGet("count-all")]
        public async Task<int> CountAll()
        {
            await RequestIncrement();
            this.LoadCache();
            return this.PackgeList.Count();
        }

        [HttpGet("count")]
        public async Task<int> Count([FromQuery] string ip)
        {

            await RequestIncrement();
            this.LoadCache();
            return this.PackgeList.Count(x => x.GameAddress.Equals(ip, StringComparison.Ordinal));
        }

        /// <summary>
        /// Returns the total number of API requests made.
        /// </summary>
        [HttpGet("request-count-all")]
        public async Task<IActionResult> RequestCountAll()
        {
            try
            {
                await RequestIncrement();

                using (var conn = new SqliteConnection($"Data Source={Path.Join(_webHostEnvironment.ContentRootPath, "/Data/avalon.db")}"))
                {
                    await conn.OpenAsync();
                    int counter = await conn.ExecuteScalarAsync<int>("select sum(hits) from usage");
                    return Ok(counter);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Returns the total number of API requests made.
        /// </summary>
        [HttpGet("request-count-by-route")]
        public async Task<IActionResult> RequestCountByRoute()
        {
            try
            {
                await RequestIncrement();

                using (var conn = new SqliteConnection($"Data Source={Path.Join(_webHostEnvironment.ContentRootPath, "/Data/avalon.db")}"))
                {
                    await conn.OpenAsync();
                    var results = await conn.QueryAsync(@"select route, sum(hits) as 'hits' from usage group by route order by 'hits'");
                    var sb = new StringBuilder();

                    foreach (var item in results)
                    {
                        sb.AppendFormat("{0},{1}\r\n", item.route, item.hits);
                    }

                    return Ok(sb.ToString());
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        //[HttpGet("request-count-all-debug")]
        //public async Task<IActionResult> RequestCountAllDebug()
        //{
        //    using (var conn = new SqliteConnection($"Data Source={Path.Join(_webHostEnvironment.ContentRootPath, "/Data/avalon.db")}"))
        //    {
        //        await conn.OpenAsync();
        //        var results = await conn.QueryAsync("select * from usage");
        //        var sb = new StringBuilder();

        //        foreach (var item in results)
        //        {
        //            sb.AppendFormat("{0},{1},{2}\r\n", item.route, item.dt, item.hits);
        //        }

        //        return Ok(sb.ToString());
        //    }
        //}

        /// <summary>
        /// Returns the number of requests that have been made to this site.
        /// </summary>
        /// <remarks>
        /// This will only log the statistic if it comes through one of the routes we've defined.
        /// </remarks>
        private async Task<int> RequestIncrement()
        {
            try
            {
                using (var conn = new SqliteConnection($"Data Source={Path.Join(_webHostEnvironment.ContentRootPath, "/Data/avalon.db")}"))
                {
                    await conn.OpenAsync();

                    int counter = await conn.ExecuteScalarAsync<int>("select hits from usage where route = @route and dt = @dt", new { route = Request.Path.Value, dt = DateTime.Now.ToShortDateString() });

                    if (counter == 0)
                    {
                        await conn.ExecuteAsync("insert into usage (route, hits, dt) values (@route, 1, @dt)", new { route = Request.Path.Value, dt = DateTime.Now.ToShortDateString() });
                        return 1;
                    }
                    else
                    {
                        counter++;
                        await conn.ExecuteAsync("update usage set hits = @counter where route = @route and dt = @dt", new { counter, route = Request.Path.Value, dt = DateTime.Now.ToShortDateString() });
                        return counter;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return -1;
        }

        /// <summary>
        /// Reloads the cache.
        /// </summary>
        [HttpGet("reload-cache")]
        public IActionResult ReloadCache()
        {
            _cache.Remove("PackageList");
            _cache.Remove("PackageListMetadata");
            this.LoadCache();
            return Ok("Ok");
        }

        /// <summary>
        /// Will load the cache with the contents of our packages if they do not exist.
        /// </summary>
        private void LoadCache()
        {
            List<IPackage> list;
            List<IPackage> metadataList;

            // Look for cache key.
            if (!_cache.TryGetValue("PackageList", out list))
            {
                list = new List<IPackage>();

                // Set cache options.
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromHours(6));

                foreach (var file in _webHostEnvironment.ContentRootFileProvider.GetDirectoryContents("/Packages").Where(x => x.Name.EndsWith(".json", StringComparison.OrdinalIgnoreCase)))
                {
                    System.Diagnostics.Trace.WriteLine(file.Name);
                    list.Add(DeserializePackage(file.CreateReadStream()));
                }

                // Sort the list by category.
                list = list.OrderBy(x => x.Category).ToList();

                // Save data in cache.
                _cache.Set("PackageList", list, cacheEntryOptions);
            }

            this.PackgeList = list;

            // Look for cache key.
            if (!_cache.TryGetValue("PackageListMetadata", out metadataList))
            {
                metadataList = new List<IPackage>();

                // Set cache options.
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromHours(6));

                // Only the metadata, not the content which can be large
                foreach (var item in list)
                {
                    var p = new Package
                    {
                        Name = item.Name,
                        Description = item.Description,
                        Author = item.Author,
                        Category = item.Category,
                        GameAddress = item.GameAddress,
                        Version = item.Version,
                        MinimumClientVersion = item.MinimumClientVersion,
                        Id = item.Id
                    };

                    metadataList.Add(p);
                }

                // Save data in cache.
                _cache.Set("PackageListMetadata", metadataList, cacheEntryOptions);
            }

            this.PacakgeListMetadata = metadataList;
        }

        /// <summary>
        /// Deserializes one <see cref="Package"/> from a <see cref="Stream"/>.
        /// </summary>
        /// <param name="s"></param>
        private static IPackage DeserializePackage(Stream s)
        {
            var serializer = new Newtonsoft.Json.JsonSerializer();

            using (var sr = new StreamReader(s))
            {
                using (var jsonTextReader = new Newtonsoft.Json.JsonTextReader(sr))
                {
                    return serializer.Deserialize<Package>(jsonTextReader);
                }
            }
        }
    }
}