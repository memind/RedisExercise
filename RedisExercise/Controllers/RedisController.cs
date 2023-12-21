using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using RedisExercise.Services;
using StackExchange.Redis;

namespace RedisExercise.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RedisController : ControllerBase
    {
        private IMemoryCache _memoryCache;
        private IDistributedCache _distributedCache;

        public RedisController(IMemoryCache memoryCache, IDistributedCache distributedCache)
        {
            _memoryCache = memoryCache;
            _distributedCache = distributedCache;
        }

        [HttpGet("/set/{userName}/{name}")]
        public void Set(string userName, string name)
        {
            _memoryCache.Set(CacheConsts.UserName, userName, options: new()
            {
                AbsoluteExpiration = DateTime.Now.AddMinutes(1),
                SlidingExpiration = TimeSpan.FromSeconds(5)
            });

            _memoryCache.Set(CacheConsts.Name, name, options: new()
            {
                AbsoluteExpiration = DateTime.Now.AddMinutes(1),
                SlidingExpiration = TimeSpan.FromSeconds(5)
            });


            _distributedCache.SetString(CacheConsts.UserName, userName, options: new()
            {
                AbsoluteExpiration = DateTime.Now.AddMinutes(1),
                SlidingExpiration = TimeSpan.FromSeconds(5)
            });

            _distributedCache.SetString(CacheConsts.Name, name, options: new()
            {
                AbsoluteExpiration = DateTime.Now.AddMinutes(1),
                SlidingExpiration = TimeSpan.FromSeconds(5)
            });
        }


        [HttpGet("/get")]
        public List<string> Get()
        {
            List<string> list = new List<string>();

            if (_memoryCache.TryGetValue(CacheConsts.Name, out string nameValue))
                list.Add(nameValue);

            if (_memoryCache.TryGetValue(CacheConsts.UserName, out string userNameValue))
                list.Add(userNameValue);

            if (_distributedCache.GetString(CacheConsts.Name) is not null)
                list.Add(_distributedCache.GetString(CacheConsts.Name));

            if (_distributedCache.GetString(CacheConsts.UserName) is not null)
                list.Add(_distributedCache.GetString(CacheConsts.UserName));

            return list;
        }


        [HttpGet("/pubSub")]
        public async Task PubSub()
        {
            ConnectionMultiplexer redis = await ConnectionMultiplexer.ConnectAsync("localhost:1453");
            ISubscriber subscriber = redis.GetSubscriber();

            // Publisher
            await subscriber.PublishAsync("crazyChannel","MESSAAGGGGEEEEEE");

            // Subscriber
            await subscriber.SubscribeAsync("crazyChannel", (channel, message) =>
            {
                Console.WriteLine(message);
            });
        }


        [HttpGet("setValue/{key}")]
        public async void SetValue(string key, string value)
        {
            var redis = await RedisServices.RedisMasterDatabase();
            await redis.StringSetAsync(key, value);
        }


        [HttpGet("getValue/{key}")]
        public async Task<string> GetValue(string key)
        {
            var redis = await RedisServices.RedisMasterDatabase();
            var value = await redis.StringGetAsync(key);

            return value.ToString();
        }
    }
}
