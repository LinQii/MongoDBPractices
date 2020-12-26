using MongoDB.Bson;
using MongoDB.Driver;
using MongoDBPractices.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MongoDBPractices
{
    public class WriteTests
    {
        private readonly IMongoCollection<Person> _peopleCollection;

        public WriteTests()
        {
            var client = new MongoClient("mongodb://localhost:27018,localhost:27019,localhost:27020/?replicaSet=rstest");
            var db = client.GetDatabase("test");
            _peopleCollection = db.GetCollection<Person>("people");
        }

        [Fact]
        public async Task InsertOne()
        {
            var guid = Guid.NewGuid().ToString();
            var person = new Person
            {
                FirstName = guid,
                LastName = guid,
                Birthay = DateTime.UtcNow,
                Email = guid,
                Job = guid,
                Quote = guid,
                Ssn = guid
            };

            await _peopleCollection.InsertOneAsync(person);


            var filter = Builders<Person>.Filter.Eq(x => x.Email, guid);
            var insertedPerson = await _peopleCollection.Find(filter).FirstOrDefaultAsync();

            Assert.Equal(insertedPerson.Email, guid);
        }

        [Fact]
        public async Task Replace()
        {
            var person = await _peopleCollection.Find(Builders<Person>.Filter.Empty).FirstOrDefaultAsync();

            var replacedGuid = Guid.NewGuid().ToString();
            person.Email = replacedGuid;

            var filter = Builders<Person>.Filter.Eq(x => x.Id, person.Id);

            await _peopleCollection.ReplaceOneAsync(filter, person);


            var updatedPerson = await _peopleCollection.Find(filter).FirstOrDefaultAsync();

            Assert.Equal(updatedPerson.Email, replacedGuid);
        }

        [Fact]
        public async Task Update()
        {
            var filter = Builders<Person>.Filter.Eq(x => x.Id, new ObjectId("57d7a121fa937f710a7d4874"));

            var guid = Guid.NewGuid().ToString();
            var update = Builders<Person>.Update
                .Set(x => x.Email, guid);

            await _peopleCollection.UpdateOneAsync(filter, update);

            var updatedPerson = await _peopleCollection.Find(filter).FirstOrDefaultAsync();

            Assert.Equal(updatedPerson.Email, guid);
        }

        [Fact]
        public async Task BulkWrite()
        {
            var guid = Guid.NewGuid().ToString();
            var newPerson = new Person
            {
                FirstName = guid,
                LastName = guid,
                Quote = guid,
                Job = guid,
                Ssn = guid,
                CompanyId = new ObjectId(),
                Employer = guid,
                Email = guid,
                Birthay = DateTime.Now
            };

            var filter = Builders<Person>.Filter.Eq(x => x.Id, new ObjectId("57d7a121fa937f710a7d486e"));
            var update = Builders<Person>.Update.Set(x => x.Birthay, DateTime.Now);

            var writeModels = new List<WriteModel<Person>>
            {
                new InsertOneModel<Person>(newPerson),
                new UpdateOneModel<Person>(filter, update)
            };

            var result = await _peopleCollection.BulkWriteAsync(writeModels);

            Assert.Equal(1, result.InsertedCount);
            Assert.Equal(1, result.ModifiedCount);
        }

        [Fact]
        public async Task FindAndModify()
        {
            var guid = Guid.NewGuid().ToString();
            var filter = Builders<Person>.Filter.Eq(x => x.Id, new ObjectId("57d7a121fa937f710a7d4873"));
            var update = Builders<Person>.Update.Set(x => x.Email, guid);
            
            var result = await _peopleCollection.FindOneAndUpdateAsync(filter, update);

            Assert.NotEqual(guid, result.Email);


            guid = Guid.NewGuid().ToString();
            update = Builders<Person>.Update.Set(x => x.Email, guid);

            var options = new FindOneAndUpdateOptions<Person>() { ReturnDocument = ReturnDocument.After };
            result = await _peopleCollection.FindOneAndUpdateAsync(filter, update, options);
            Assert.Equal(guid, result.Email);
        }
    }
}
