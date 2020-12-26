using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace MongoDBPractices.Models
{
    [BsonIgnoreExtraElements]
    public class Movie
    {
        [BsonId]
        public ObjectId Id { get; set; }
        [BsonElement("title")]
        public string Title { get; set; }
        [BsonElement("year")]
        public int Year { get; set; }
        [BsonElement("genres")]
        public IEnumerable<string> Genres { get; set; }
        public IEnumerable<Comment> Comments { get; set; }
    }
}
