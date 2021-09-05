using System.Collections.Generic;

namespace Valuator.Data.Repositories
{
    public interface IRedisRepository
    {
        void SaveDataToDb(string key, string value);
        string GetDataFromDbByKey(string key);
        IEnumerable<string> GetKeysFromDbByPrefix(string prefix);
        bool IsKeyExistInDb(string key);
    }
}
