using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace MongoDBPractices.Models
{
    public class Person
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("last_name")]
        public string LastName { get; set; }
        [BsonElement("quote")]
        public string Quote { get; set; }
        [BsonElement("job")]
        public string Job { get; set; }

        [BsonElement("ssn")]
        public string Ssn { get; set; }
        [BsonElement("address")]
        public Address Address { get; set; }
        [BsonElement("first_name")]
        public string FirstName { get; set; }
        [BsonElement("company_id")]
        public ObjectId CompanyId { get; set; }
        [BsonElement("employer")]
        public string Employer { get; set; }
        [BsonElement("birthday")]
        public DateTime Birthay { get; set; }
        [BsonElement("email")]
        public string Email { get; set; }
    }

    public class Address
    {
        [BsonElement("city")]
        public string City { get; set; }
        [BsonElement("street")]
        public string Street { get; set; }
        [BsonElement("zip")]
        public string Zip { get; set; }
        [BsonElement("state")]
        public string State { get; set; }
    }




    public class PersonGroup
    {
        [BsonId]
        public string City { get; set; }
        public IEnumerable<PersonInfo> Infos { get; set; }
    }

    public class PersonInfo
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string Street { get; set; }

    }
}
