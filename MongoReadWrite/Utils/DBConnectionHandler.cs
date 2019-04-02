using Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoReadWrite.Utils
{
    public class DBConnectionHandler<T> where T: IBaseModel
	{
		private readonly string _connectionString;
		private readonly string _dbName;
		private IMongoCollection<T> collection;
		public DBConnectionHandler()
		{
			_connectionString = Environment.GetEnvironmentVariable("MongoUri", EnvironmentVariableTarget.Process);
			_dbName = Environment.GetEnvironmentVariable("DbName", EnvironmentVariableTarget.Process);
			if (string.IsNullOrWhiteSpace(_dbName))
			{
				_dbName = "DropMe";
			}
		}
		public IMongoCollection<T> ConnectToDatabase(string collectionName)
		{
			var client = new MongoClient(MongoUrl.Create(_connectionString));
			var db = client.GetDatabase(_dbName);
			collection = db.GetCollection<T>(collectionName);
			return collection;
		}
		public IEnumerable<T> Get()
		{			
			return collection.Find(all => true).ToEnumerable();
		}
		public T Get(string id)
		{
			return Get().Where(r => r.Id == id).FirstOrDefault();			
		}
		
		public async Task<T> Create(T newRecord)
		{
			try
			{
				UpdateIdIfNeeded(newRecord);
				await collection.InsertOneAsync(newRecord);
				return newRecord;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Exception while inserting a new record\n{ex.Message}");
				return default(T);
			}
		}

		private static void UpdateIdIfNeeded(T newRecord)
		{
			var serviceInterface = typeof(T);
			var id = (string)serviceInterface.GetProperty("Id").GetValue(newRecord, null);
			if (string.IsNullOrWhiteSpace(id))
			{
				serviceInterface.GetProperty("Id").SetValue(newRecord, Guid.NewGuid().ToString());
			}
		}

		
		public async Task<bool> Remove(string id)
		{
			try
			{
				var record = Get(id);
				await collection.DeleteOneAsync(r => r.Id == record.Id);
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Exception while deleting a record\n{ex.Message}");
				return false;
			}
		}
		public async Task<bool> Update(string id, T record)
		{
			try
			{
				UpdateIdIfNeeded(record);
				await collection.ReplaceOneAsync(rcd => rcd.Id == id, record, new UpdateOptions { IsUpsert = true });
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Exception while Updating a record\n{ex.Message}");
				return false;
			}
		}

	}
}
