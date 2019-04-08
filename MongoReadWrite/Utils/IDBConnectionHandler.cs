using System.Collections.Generic;
using System.Threading.Tasks;
using Models;
using MongoDB.Driver;

namespace MongoReadWrite.Utils
{
	public interface IDBConnectionHandler<T> where T : IBaseModel
	{
		IMongoCollection<T> ConnectToDatabase(string collectionName);
		Task<bool> Create(List<T> records);
		Task<T> Create(T newRecord);
		IEnumerable<T> Get();
		T Get(string id);
		Task<bool> Remove(string id);
		Task<bool> RemoveAll();
		Task<bool> Update(string id, T record);
		Task<bool> UpdateMultipe(List<T> updateRecords);
	}
}