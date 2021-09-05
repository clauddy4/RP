using StackExchange.Redis;
using System.Collections.Generic;

namespace Valuator.Data.Repositories
{
    public class RedisRepository : IRedisRepository
    {
        private readonly IConnectionMultiplexer _redis;
        private const int Port = 6379;
        private const string Localhost = "localhost";

        public RedisRepository()
        {
            _redis = ConnectionMultiplexer.Connect(Localhost);
        }

        public IEnumerable<string> GetAllFromDbByPrefix(string prefix)
        {
             var server = _redis.GetServer(Localhost, Port);
             List<string> keys = new List<string>();
             foreach (var key in server.Keys(pattern: prefix + "*"))
             {
                 keys.Add(key);
             }
             return keys;
        }

        public string GetDataFromDbByKey(string key)
        {
            var db = _redis.GetDatabase();
            return db.StringGet(key);
        }

        public void SaveDataToDb(string key, string value)
        {
            var db = _redis.GetDatabase();
            db.StringSet(key, value);
        }
    }
}
