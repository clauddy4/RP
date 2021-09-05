using StackExchange.Redis;
using System;
using System.Collections.Generic;

namespace ClassLibrary
{
    public class RedisRepository : IRedisRepository
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IConnectionMultiplexer _redisRus;
        private readonly IConnectionMultiplexer _redisEu;
        private readonly IConnectionMultiplexer _redisOther;

        private string envVariableRus = Environment.GetEnvironmentVariable($"DB_{Constants.SegmentRus}");
        private string envVariableEu = Environment.GetEnvironmentVariable($"DB_{Constants.SegmentEu}");
        private string envVariableOther = Environment.GetEnvironmentVariable($"DB_{Constants.SegmentOther}");
        private const string Localhost = "localhost";

        public RedisRepository()
        {
            _redis = ConnectionMultiplexer.Connect(Localhost);
            _redisRus = ConnectionMultiplexer.Connect(envVariableRus);
            _redisEu = ConnectionMultiplexer.Connect(envVariableEu);
            _redisOther = ConnectionMultiplexer.Connect(envVariableOther);
        }

        public IEnumerable<string> GetKeysFromDbByPrefix(string prefix, string shardKey, string segmentId)
        {
            var serverList = new List<IServer> { _redisRus.GetServer(envVariableRus), _redisEu.GetServer(envVariableEu), _redisOther.GetServer(envVariableOther) };
            List<string> keys = new List<string>();
            foreach (var server in serverList)
            {
                foreach (var key in server.Keys(pattern: prefix + "*"))
                {
                    keys.Add(key);
                }
            }
            return keys;
        }

        public string GetDataFromDbByKey(string key, string shardKey)
        {
            var db = DefineConnection(shardKey).GetDatabase();
            if (!db.KeyExists(key))
            {
                return "";
            }
            return db.StringGet(key);
        }

        public string GetDataFromAllServers(string key)
        {
            var dbList = new List<IDatabase> { _redisRus.GetDatabase(), _redisEu.GetDatabase(), _redisOther.GetDatabase() };
            foreach (var db in dbList)
            {
                if (db.KeyExists(key))
                {
                    return db.StringGet(key);
                }
            }
            return "";
        }

        public void SaveDataToDb(string key, string value, string shardKey)
        {
            var db = DefineConnection(shardKey).GetDatabase();
            db.StringSet(key, value);
        }

        public bool IsKeyExistInDb(string key, string shardKey)
        {
            var db = DefineConnection(shardKey).GetDatabase();
            return db.KeyExists(key);
        }

        public string GetSegmentIdFromDb(string shardKey)
        {      
            if (!IsSegmentExists(shardKey))
            {
                return "";
            }
            return GetSegment(shardKey);
        }

        public void SaveShardKey(string key, string segmentId)
        {
            var db = _redis.GetDatabase();
            db.StringSet(key, segmentId);
        }

        private bool IsSegmentExists(string key)
        {
            var db = _redis.GetDatabase();
            return db.KeyExists(key);
        }

        private string GetSegment(string key)
        {
            var db = _redis.GetDatabase();
            return db.StringGet(key);
        }

        private IConnectionMultiplexer DefineConnection(string shardKey)
        {
            var shardId = GetSegment(shardKey);
            if (shardId == Constants.SegmentRus)
            {
                return _redisRus;
            }
            if (shardId == Constants.SegmentEu)
            {
                return _redisEu;
            }
            if (shardId == Constants.SegmentOther)
            {
                return _redisOther;
            }
            return _redis;
        }
    }
}
