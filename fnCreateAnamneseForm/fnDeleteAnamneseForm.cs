using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.Json;

namespace fnCreateAnamneseForm
{
    public class fnDeleteAnamneseForm
    {
        private readonly ILogger<fnDeleteAnamneseForm> _logger;

        public fnDeleteAnamneseForm(ILogger<fnDeleteAnamneseForm> logger)
        {
            _logger = logger;
        }

        [Function("fnDeleteAnamneseForm")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "fnDeleteAnamneseForm/{id}")] HttpRequest req, string id)
        {
            _logger.LogInformation("Função fnDeleteAnamneseForm HTTP POST acionada.");

            string connectionString = System.Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING");
            string databaseName = System.Environment.GetEnvironmentVariable("MONGODB_DATABASE_NAME");
            string collectionName = System.Environment.GetEnvironmentVariable("MONGODB_COLLECTION_NAME");

            try
            {
                var client = new MongoClient(connectionString);
                var database = client.GetDatabase(databaseName);
                var collection = database.GetCollection<BsonDocument>(collectionName);
                
                // Criar um filtro para encontrar o documento pelo id
                var filter = Builders<BsonDocument>.Filter.Eq("_id", new ObjectId(id));

                var deleteResult = await collection.DeleteOneAsync(filter);

                if (deleteResult.DeletedCount == 0)
                {
                    return new NotFoundResult();
                }

                return new NoContentResult();
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Erro ao deletar documento: {ex.Message}");
                return new BadRequestObjectResult($"Erro ao deletar documento: {ex.Message}");
            }
        }
    }
}
