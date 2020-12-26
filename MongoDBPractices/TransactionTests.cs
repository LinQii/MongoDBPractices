using MongoDB.Bson;
using MongoDB.Driver;
using MongoDBPractices.Models;
using System;
using Xunit;

namespace MongoDBPractices
{
    public class TransactionTests
    {
        private readonly IMongoCollection<Movie> _moviesCollection;
        private readonly IMongoCollection<Comment> _commentsCollection;
        private readonly MongoClient _client;

        public TransactionTests()
        {
            _client = new MongoClient("mongodb://localhost:27018,localhost:27019,localhost:27020/?replicaSet=rstest");
            var db = _client.GetDatabase("test");

            _moviesCollection = db.GetCollection<Movie>("movies");
            _commentsCollection = db.GetCollection<Comment>("comments");
        }

        [Fact]
        public void Transaction()
        {
            using (var session = _client.StartSession())
            {
                try
                {
                    session.StartTransaction();

                    // update movie
                    var title = Guid.NewGuid().ToString();
                    var movieFilter = Builders<Movie>.Filter.Eq(x => x.Id, new ObjectId("573a1390f29313caabcd4135"));
                    var movieUpdate = Builders<Movie>.Update.Set(x => x.Title, title);
                    _moviesCollection.UpdateOne(session, movieFilter, movieUpdate);

                    // insert comment
                    var name = Guid.NewGuid().ToString();
                    var comment = new Comment
                    {
                        Name = name,
                        Date = DateTime.Now,
                        MovieId = new ObjectId("573a1390f29313caabcd4135"),
                        Text = name
                    };

                    _commentsCollection.InsertOne(session, comment);

                    session.CommitTransaction();

                    // Assert
                    var updatedMovie = _moviesCollection.Find(movieFilter).FirstOrDefault();
                    Assert.Equal(title, updatedMovie.Title);

                    var commentFilter = Builders<Comment>.Filter.Eq(x => x.Name, name);
                    var insetedComment = _commentsCollection.Find(commentFilter).FirstOrDefault();
                    Assert.NotNull(insetedComment);
                } 
                catch (Exception ex)
                {
                    Assert.True(false);
                }

            }
        }
    }
}
