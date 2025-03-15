using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.Json;

namespace fnCreateAnamneseForm
{
    public class fnPostAnamneseForm
    {
        private readonly ILogger<fnPostAnamneseForm> _logger;

        public fnPostAnamneseForm(ILogger<fnPostAnamneseForm> logger)
        {
            _logger = logger;
        }

        [Function("fnPostAnamneseForm")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("Função C# HTTP POST acionada.");

            string connectionString = System.Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING");
            string databaseName = System.Environment.GetEnvironmentVariable("MONGODB_DATABASE_NAME");
            string collectionName = System.Environment.GetEnvironmentVariable("MONGODB_COLLECTION_NAME");

            try
            {
                var client = new MongoClient(connectionString);
                var database = client.GetDatabase(databaseName);
                var collection = database.GetCollection<BsonDocument>(collectionName);

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var document = BsonDocument.Parse(requestBody);

                await collection.InsertOneAsync(document);

                return new CreatedResult("", document["_id"].AsObjectId.ToString());
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Erro ao criar documento: {ex.Message}");
                return new BadRequestObjectResult($"Erro ao criar documento: {ex.Message}");
            }
        }
    }
}
