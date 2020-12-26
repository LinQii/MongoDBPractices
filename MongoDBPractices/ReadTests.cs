using MongoDB.Bson;
using MongoDB.Driver;
using MongoDBPractices.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MongoDBPractices
{
    public class ReadTests
    {
        private readonly IMongoCollection<Person> _peopleCollection;

        public ReadTests()
        {
            var client = new MongoClient("mongodb://localhost:27018,localhost:27019,localhost:27020/?replicaSet=rstest");
            var db = client.GetDatabase("test");
            _peopleCollection = db.GetCollection<Person>("people");
        }
        
        [Fact]
        public async Task Find()
        {
            var builderFilter = Builders<Person>.Filter;
            var filter = builderFilter.Eq(x => x.LastName, "Pham");

            var results = await _peopleCollection.Find(filter)
                .ToListAsync();

            Assert.True(results.All(x => x.LastName.Equals("Pham", StringComparison.CurrentCultureIgnoreCase)));
        }

        [Fact]
        public async Task Find_Projection()
        {
            var builderFilter = Builders<Person>.Filter;
            var filter = builderFilter.Eq(x => x.LastName, "Pham");

            // project { first_name: 1, last_name: 1, _id: 0 }
            var projection = Builders<Person>.Projection
                .Include(x => x.FirstName)
                .Include(x => x.LastName)
                .Exclude(x => x.Id);

            var results = await _peopleCollection.Find(filter)
                .Project<Person>(projection)
                .Limit(10)
                .ToListAsync();

            var firstPerson = results.First();

            Assert.Equal("Pham", firstPerson.LastName);
            Assert.NotNull(firstPerson.LastName);
            Assert.Null(firstPerson.Job);
            Assert.Equal(ObjectId.Empty, firstPerson.Id);
        }

        [Fact]
        public async Task Find_SortAndPaging()
        {
            var builderFilter = Builders<Person>.Filter;
            var filter = builderFilter.Eq(x => x.LastName, "Pham");

            var results = await _peopleCollection.Find(filter)
                .Sort(Builders<Person>.Sort.Ascending(x => x.Job))
                .Skip(0)
                .Limit(10)
                .ToListAsync();

            Assert.True(results.All(x => x.LastName.Equals("Pham", StringComparison.CurrentCultureIgnoreCase)));
            Assert.Equal(10, results.Count);
        }
    }
}
