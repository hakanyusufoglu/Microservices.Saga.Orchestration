using MongoDB.Driver;

namespace Stock.Api.Services
{
    public class MongoDbService
    {
        readonly IMongoDatabase _database;
        public MongoDbService(IConfiguration configuration)
        {
            MongoClient client = new(configuration.GetConnectionString("MongoDb"));
            _database = client.GetDatabase("StockDb"); //appsettings.jsondan alınması best practisedir.
        }

        public IMongoCollection<T> GetCollection<T>() => _database.GetCollection<T>(typeof(T).Name.ToLowerInvariant());
       
    }
}
