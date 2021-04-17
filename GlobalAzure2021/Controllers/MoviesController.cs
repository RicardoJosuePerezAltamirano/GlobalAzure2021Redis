using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace GlobalAzure2021.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        Model Model;
        IConfiguration Configuration;
        public MoviesController(Model model,IConfiguration config)
        {
            Model = model;
            Configuration = config;
        }
        [HttpGet]
        public async Task<List<Movie>>  Get(int id)
        {
            // Demo cache 1 consultar y si no existe agregarlo
            IDatabase Cache = CacheServices.Connection.GetDatabase();
            var valueCache = Cache.StringGet(new RedisKey(id.ToString()));
            if (valueCache.HasValue)
            {
                System.Diagnostics.Debug.WriteLine("+++++++USO DE CACHE+++++++");
                var item=  JsonSerializer.Deserialize<Movie>(valueCache.ToString());
                return new List<Movie>() { item };
            }
            else
            {
                await Task.Delay(3000);
                var Movie= Model.Movies.Where(o => o.MovieId == id).ToList();
                foreach(var item in Movie)
                {
                    await Cache.StringSetAsync(new RedisKey(item.MovieId.ToString()), new RedisValue(JsonSerializer.Serialize(item))).ConfigureAwait(false);
                }
                return Movie;
                
            }
            
        }
        [HttpPost("add")]
        public async Task<Movie> Add(Movie movie)
        {
            //Demo cache 2 agregar con expiracion
            Model.Movies.Add(movie);
            IDatabase Cache = CacheServices.Connection.GetDatabase();
            await Cache.StringSetAsync(new RedisKey(movie.MovieId.ToString()), new RedisValue(JsonSerializer.Serialize(movie)),TimeSpan.FromSeconds(60)).ConfigureAwait(false);
            return movie;
        }
        [HttpGet("counter")]
        public int GetCounter()
        {
            return Model.Movies.Count;
            
        }
        [HttpGet("exists")]
        public async Task<bool> Exists(int id)
        {
            //Demo cache 3 consultar si existe una key
            IDatabase Cache = CacheServices.Connection.GetDatabase();
            return await Cache.KeyExistsAsync(new RedisKey(id.ToString()));
            

        }
        [HttpGet("delete")]
        public async Task<bool> Delete(int id)
        {
            //Demo cache 4 elimnar un registro
            IDatabase Cache = CacheServices.Connection.GetDatabase();
            return await Cache.KeyDeleteAsync(new RedisKey(id.ToString()));
            

        }
        [HttpGet("flush")]
        public async Task flush()
        {
            //Demo cache 5 limpiar db
            var Cache = CacheServices.Connection.GetServer(Configuration.GetValue<string>("Cachedb"));
            if(Cache.IsConnected)
            {
                System.Diagnostics.Debug.WriteLine("Conectado, liberando cache");
                await Cache.FlushDatabaseAsync();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("no Conectado");
            }
            

        }
    }
}
