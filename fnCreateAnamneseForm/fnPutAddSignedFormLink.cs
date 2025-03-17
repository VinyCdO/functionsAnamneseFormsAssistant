using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.Json;

namespace fnCreateAnamneseForm
{
    public class fnPutAddSignedFormLink
    {
        private readonly ILogger<fnPostAnamneseForm> _logger;

        public fnPutAddSignedFormLink(ILogger<fnPostAnamneseForm> logger)
        {
            _logger = logger;
        }

        [Function("fnPutAddSignedFormLink")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "{id}")] HttpRequest req, string id)
        {
            _logger.LogInformation("Função fnPutAddSignedFormLink HTTP PUT acionada.");

            string connectionString = System.Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING");
            string databaseName = System.Environment.GetEnvironmentVariable("MONGODB_DATABASE_NAME");
            string collectionName = System.Environment.GetEnvironmentVariable("MONGODB_COLLECTION_NAME");

            try
            {
                var client = new MongoClient(connectionString);
                var database = client.GetDatabase(databaseName);
                var collection = database.GetCollection<BsonDocument>(collectionName);

                // Ler o corpo da requisição
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var updateDocument = BsonDocument.Parse(requestBody);

                // Criar um filtro para encontrar o documento pelo id
                var filter = Builders<BsonDocument>.Filter.Eq("_id", new ObjectId(id));

                // Atualizar o documento
                var updateResult = await collection.UpdateOneAsync(filter, new BsonDocument("$set", updateDocument));

                if (updateResult.MatchedCount == 0)
                {
                    return new NotFoundObjectResult($"Documento com id {id} não encontrado.");
                }

                return new OkObjectResult($"Documento com id {id} atualizado com sucesso.");
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Erro ao atualizar documento: {ex.Message}");
                return new BadRequestObjectResult($"Erro ao atualizar documento: {ex.Message}");
            }
        }
    }
}
