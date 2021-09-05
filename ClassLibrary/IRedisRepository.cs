using System.Collections.Generic;

namespace ClassLibrary
{
    public interface IRedisRepository
    {
        void SaveDataToDb(string key, string value, string shardKey);
        string GetDataFromDbByKey(string key, string shardKey);
        IEnumerable<string> GetKeysFromDbByPrefix(string prefix, string shardKey, string segmentId);
        bool IsKeyExistInDb(string key, string shardKey);
        string GetSegmentIdFromDb(string shardKey);
        void SaveShardKey(string key, string segmentId);
    }
}
