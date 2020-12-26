using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace MongoDBPractices.Models
{
    public class Comment
    {
        [BsonId]
        public ObjectId Id { get; set; }
        [BsonElement("name")]
        public string Name { get; set; }
        [BsonElement("email")]
        public string Email { get; set; }
        [BsonElement("movie_id")]
        public ObjectId MovieId { get; set; }
        [BsonElement("text")]
        public string Text { get; set; }
        [BsonElement("date")]
        public DateTime Date { get; set; }
    }
}
