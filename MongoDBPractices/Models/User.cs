using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDBPractices.Models
{
    [BsonIgnoreExtraElements]
    public class User
    {
        [BsonElement("email")]
        public string Email { get; set; }
        [BsonElement("password")]
        public string Password { get; set; }
    }
}
