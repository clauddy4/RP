using System.Collections.Generic;

namespace Valuator.Data.Repositories
{
    public interface IRedisRepository
    {
        void SaveDataToDb(string key, string value);
        string GetDataFromDbByKey(string key);
        IEnumerable<string> GetAllFromDbByPrefix(string prefix);
    }
}
