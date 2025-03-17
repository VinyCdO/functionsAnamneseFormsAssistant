using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace fnCreateAnamneseForm
{
    public class fnGetAnamneseForms
    {
        private readonly ILogger<fnGetAnamneseForms> _logger;

        public fnGetAnamneseForms(ILogger<fnGetAnamneseForms> logger)
        {
            _logger = logger;
        }

        [Function("fnGetAnamneseForms")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("Função fnGetAnamneseForms HTTP GET acionada.");

            string connectionString = System.Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING");
            string databaseName = System.Environment.GetEnvironmentVariable("MONGODB_DATABASE_NAME");
            string collectionName = System.Environment.GetEnvironmentVariable("MONGODB_COLLECTION_NAME");

            try
            {
                var client = new MongoClient(connectionString);
                var database = client.GetDatabase(databaseName);
                var collection = database.GetCollection<BsonDocument>(collectionName);

                string id = req.Query["id"];
                string nome = req.Query["nome"];

                FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.Empty; 

                if (!string.IsNullOrEmpty(id))
                {
                    filter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(id));
                }
                else
                if (!string.IsNullOrEmpty(nome))
                {                    
                    var regex = new BsonRegularExpression(new Regex(nome, RegexOptions.IgnoreCase));
                    filter = Builders<BsonDocument>.Filter.Regex("nome", regex);
                } else
                {
                    filter = Builders<BsonDocument>.Filter.Empty;
                }

                var documents = await collection.Find(filter).ToListAsync();

                return new OkObjectResult(documents.ToJson());
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Erro ao obter documentos: {ex.Message}");
                return new BadRequestObjectResult($"Erro ao obter documentos: {ex.Message}");
            }
        }
    }
}
