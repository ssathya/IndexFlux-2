using Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MongoReadWrite.Utils
{
	public class DBConnectionHandler<T> : IDBConnectionHandler<T> where T : IBaseModel
	{

		#region Private Fields

		private readonly string _connectionString;
		private readonly string _dbName;
		private IMongoCollection<T> collection;

		#endregion Private Fields


		#region Public Constructors

		public DBConnectionHandler()
		{
			_connectionString = Environment.GetEnvironmentVariable("MongoUri", EnvironmentVariableTarget.Process);
			_dbName = Environment.GetEnvironmentVariable("DbName", EnvironmentVariableTarget.Process);
			if (string.IsNullOrWhiteSpace(_dbName))
			{
				_dbName = "DropMe";
			}
		}

		#endregion Public Constructors


		#region Public Methods

		public IMongoCollection<T> ConnectToDatabase(string collectionName)
		{
			var client = new MongoClient(MongoUrl.Create(_connectionString));
			var db = client.GetDatabase(_dbName);
			collection = db.GetCollection<T>(collectionName);
			return collection;
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

		public async Task<bool> Create(List<T> records)
		{
			if (records == null || records.Count == 0)
			{
				return false;
			}
			foreach (var record in records)
			{
				UpdateIdIfNeeded(record);
			}
			try
			{
				await collection.InsertManyAsync(records);
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Exception while inserting a new records\n{ex.Message}");
				return false;
			}
		}

		public IEnumerable<T> Get()
		{
			return collection.Find(all => true).ToEnumerable();
		}

		public T Get(string id)
		{
			return Get().Where(r => r.Id == id).FirstOrDefault();
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

		public async Task<bool> RemoveAll()
		{
			try
			{
				await collection.DeleteManyAsync(all => true);
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Exception while deleting all records\n{ex.Message}");
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
		public async Task<bool> UpdateMultipe(List<T> updateRecords)
		{
			var objsWithoutId = new List<T>();
			var updates = new List<WriteModel<T>>();
			var filterBuilder = Builders<T>.Filter;
			foreach (var updateRecord in updateRecords)
			{

				ExtractId(updateRecord, out Type serviceInterface, out string id);
				if (!string.IsNullOrWhiteSpace(id))
				{
					var filter = filterBuilder.Where(x => x.Id == updateRecord.Id);
					updates.Add(new ReplaceOneModel<T>(filter, updateRecord));
				}
				else
				{
					objsWithoutId.Add(updateRecord);
				}
			}
			try
			{
				var operation1Result = true;
				var operation2Result = true;
				if (updates.Count != 0)
				{
					var result1 = await collection.BulkWriteAsync(updates);
					operation1Result = result1.ModifiedCount != 0;
				}
				if (objsWithoutId.Count != 0)
				{
					operation2Result = await Create(objsWithoutId);
				}
				return operation1Result && operation2Result;

			}
			catch (Exception ex)
			{
				Console.WriteLine($"Exception while Bulk Updating a record\n{ex.Message}");
				return false;
			}

		}

		#endregion Public Methods


		#region Private Methods

		private static void UpdateIdIfNeeded(T newRecord)
		{
			ExtractId(newRecord, out Type serviceInterface, out string id);
			if (string.IsNullOrWhiteSpace(id))
			{
				serviceInterface.GetProperty("Id").SetValue(newRecord, Guid.NewGuid().ToString());
			}
		}

		private static void ExtractId(T newRecord, out Type serviceInterface, out string id)
		{
			serviceInterface = typeof(T);
			id = (string)serviceInterface.GetProperty("Id").GetValue(newRecord, null);
		}

		#endregion Private Methods
	}
}