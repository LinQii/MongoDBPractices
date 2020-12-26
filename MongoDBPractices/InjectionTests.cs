using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDBPractices.Models;
using Xunit;

namespace MongoDBPractices
{
    public class InjectionTests
    {
        private readonly IMongoCollection<User> _usersCollection;

        public InjectionTests()
        {
            var client = new MongoClient("mongodb://localhost:27018,localhost:27019,localhost:27020/?replicaSet=rstest");
            var db = client.GetDatabase("test");
            _usersCollection = db.GetCollection<User>("users");
        }


        [Fact]
        public async Task Injection()
        {
            var email = "{ $gt: '1' }";
            var password = "{ $gt: '$' }, name: 'Ned Stark'";

            var json = $"{{ email: {email}, password: {password}}}";

            // if using { email: 'a', email: 'b' }. Mongo shell exclude first one. 

            var result = await _usersCollection.Find(json).FirstOrDefaultAsync();

            Assert.Null(result);

        }
    }
}
