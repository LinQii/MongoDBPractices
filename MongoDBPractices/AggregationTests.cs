using MongoDB.Driver;
using MongoDBPractices.Models;
using System.Threading.Tasks;
using Xunit;
using System.Linq;
using MongoDB.Bson;

namespace MongoDBPractices
{
    public class AggregationTests
    {
        private readonly IMongoCollection<Person> _peopleCollection;
        private readonly IMongoCollection<Movie> _moviesCollection;
        private readonly IMongoCollection<Comment> _commentsCollection;

        public AggregationTests()
        {
            var client = new MongoClient("mongodb://localhost:27018,localhost:27019,localhost:27020/?replicaSet=rstest");
            var db = client.GetDatabase("test");
            _peopleCollection = db.GetCollection<Person>("people");
            _moviesCollection = db.GetCollection<Movie>("movies");
            _commentsCollection = db.GetCollection<Comment>("comments");
        }

        [Fact]
        public async Task Aggregation_SimpleMatchGroup()
        {
            //var json = @"{
            //    $match:
            //        {
            //            job: 'Engineer, land'
            //        }
            //    },
            //    {
            //        $group:
            //        {
            //          _id: '$address.city',
            //          emails: {
            //            $push: {
            //              email: '$email',
            //              name: { $concat: ['$first_name', ' ', '$last_name'] },
            //              street: '$address.street'
            //            }
            //          }
            //        }
            //    }";



            //var matchStage = 
            //    new BsonDocument("$match",
            //        new BsonDocument("job", "Engineer, land"));

            //var groupStage = new BsonDocument("$group",
            //    new BsonDocument
            //        {
            //            { "_id", "$address.city" },
            //            { "infos",
            //    new BsonDocument("$push",
            //    new BsonDocument
            //                {
            //                    { "email", "$email" },
            //                    { "name",
            //    new BsonDocument("$concat",
            //    new BsonArray
            //                        {
            //                            "$first_name",
            //                            " ",
            //                            "$last_name"
            //                        }) },
            //                    { "street", "$address.street" }
            //                }) }
            //        });


            //var pipeline = PipelineDefinition<People, BsonDocument>.Create(json);
            //var results = await _peopleCollection.Aggregate(pipeline).ToListAsync();

            var filter = Builders<Person>.Filter.Eq(x => x.Job, "Engineer, land");

            var results = await _peopleCollection.Aggregate()
                .Match(filter)
                .Group(x => x.Address.City, g => new
                {
                    City = g.Key,
                    Infos = g.Select(i => new
                    {
                        i.Email,
                        Name = i.FirstName + " " + i.LastName,
                        i.Address.Street
                    })
                })
                .ToListAsync();

            Assert.NotEmpty(results);
        }

        [Fact]
        public async Task Aggregation_FacetPaging()
        {
            //var matchGroupStage = @"{
            //    $match:
            //        {
            //            job: 'Engineer, land'
            //        }
            //    },
            //    {
            //        $group:
            //        {
            //          _id: '$address.city',
            //          emails: {
            //            $push: {
            //              email: '$email',
            //              name: { $concat: ['$first_name', ' ', '$last_name'] },
            //              street: '$address.street'
            //            }
            //          }
            //        }
            //    }";

            //var facetStage = @"{
            //  data: [ { $skip: 0 }, { $limit: 10} ],
            //  pagingData: [ { $count: 'count' } ]
            //}";


            var pagingFacet = AggregateFacet.Create("data",
            PipelineDefinition<PersonGroup, PersonGroup>.Create(new[] {
                PipelineStageDefinitionBuilder.Skip<PersonGroup>(0),
                PipelineStageDefinitionBuilder.Limit<PersonGroup>(10),
                }));
            var countFacet = AggregateFacet.Create("pagingData",
                PipelineDefinition<PersonGroup, AggregateCountResult>.Create(new[]
                    {
                    PipelineStageDefinitionBuilder.Count<PersonGroup>()
                    }));

            var results = await _peopleCollection.Aggregate()
            .Match(x => x.Job == "Engineer, land")
            .Group(x => x.Address.City, g => new PersonGroup
            {
                City = g.Key,
                Infos = g.Select(i => new PersonInfo
                {
                    Email = i.Email,
                    Name = i.FirstName + " " + i.LastName,
                    Street = i.Address.Street
                })
            })
            .Facet(pagingFacet, countFacet)
            .ToListAsync();

            Assert.NotEmpty(results);
        }

        [Fact]
        public async Task Aggregate_SimpleLookup()
        {

            var filter = Builders<Movie>.Filter.Eq(x => x.Id, new ObjectId("573a13b7f29313caabd49171"));
            var result = await _moviesCollection.Aggregate()
            .Match(filter)
            .Lookup(
                _commentsCollection,
                m => m.Id,
                c => c.MovieId,
                (Movie m) => m.Comments
            ).ToListAsync();

            var movie = result.First();
            Assert.Equal(2, movie.Comments.Count());
        }

        [Fact]
        public async Task Aggregate_LookupWithPipeline()
        {
            //var lookupPipeline = new EmptyPipelineDefinition<Comment>()
            //    .Match(new BsonDocument("$expr",
            //        new BsonDocument("$and", new BsonArray
            //        {
            //            new BsonDocument("$eq", new BsonArray { "$movie_id", "$$movieId" }),
            //            new BsonDocument("$eq", new BsonArray { "$name", "Jamie Santana" })
            //        })));
           
            var commentPipeline = PipelineDefinition<Comment, Comment>.Create(new[] {
            PipelineStageDefinitionBuilder.Match<Comment>(new BsonDocument("$expr",
                new BsonDocument("$and", new BsonArray
                {
                    new BsonDocument("$eq", new BsonArray { "$movie_id", "$$movieId" }),
                    new BsonDocument("$eq", new BsonArray { "$name", "Jamie Santana" })
                })))
            });

            var result = await _moviesCollection.Aggregate()
            .Match(x => x.Id == new ObjectId("573a13b7f29313caabd49171"))
            .Lookup(
                _commentsCollection,
                new BsonDocument("movieId", "$_id"),
                commentPipeline,
                (Movie m) => m.Comments
            ).ToListAsync();

            var movie = result.First();
            Assert.Single(movie.Comments);
            Assert.Equal("Jamie Santana", movie.Comments.First().Name);
        }

        [Fact]
        public async Task Unwind()
        {
            var objId = new ObjectId("573a1395f29313caabce30e8");
            var filter = Builders<Movie>.Filter.Eq(x => x.Id, objId);
            var movie = await _moviesCollection.Find(filter).FirstOrDefaultAsync();

            var results = await _moviesCollection.Aggregate()
                .Match(x => x.Id == objId)
                .Unwind(x => x.Genres)
                .ToListAsync();

            Assert.Equal(movie.Genres.Count(), results.Count);
        }
    }
}
