using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Models
{
	public interface IBaseModel
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		string Id { get; set; }
	}
}