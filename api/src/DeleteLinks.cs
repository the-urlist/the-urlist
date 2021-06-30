using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Azure.Documents.Linq;

namespace Api
{
    public partial class LinkOperations
    {
        [FunctionName(nameof(DeleteLink))]
        public async Task<IActionResult> DeleteLink(
            [HttpTrigger(AuthorizationLevel.Function, "DELETE", Route = "links/{vanityUrl}")] HttpRequest req,
            [CosmosDB(
                databaseName: "linkylinkdb",
                collectionName: "linkbundles",
                ConnectionStringSetting = "DB_CONNECTION_STRING",
                SqlQuery = "SELECT * FROM linkbundles lb WHERE lb.vanityUrl = {vanityUrl}"
            )] IEnumerable<Document> documents,
            [CosmosDB(ConnectionStringSetting = "DB_CONNECTION_STRING")] DocumentClient docClient,
            string vanityUrl,
            ILogger log)
        {


            var claimsPrincipal = StaticWebAppsAuth.Parse(req);

            //not logged in? Bye...
            if (string.IsNullOrEmpty(claimsPrincipal.Identity.Name)) return new UnauthorizedResult();

            if (!documents.Any())
            {
                log.LogInformation($"Bundle for {vanityUrl} not found.");
                return new NotFoundResult();
            }

            Document doc = documents.Single();

            try
            {
                string userId = doc.GetPropertyValue<string>("userId");

                if (!claimsPrincipal.Identity.Name.Equals(userId, StringComparison.InvariantCultureIgnoreCase))
                {
                    log.LogWarning($"{userId} is trying to delete {vanityUrl} but is not the owner.");
                    return new StatusCodeResult(StatusCodes.Status403Forbidden);
                }

                RequestOptions reqOptions = new RequestOptions { PartitionKey = new PartitionKey(vanityUrl) };
                await docClient.DeleteDocumentAsync(doc.SelfLink, reqOptions);
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
            return new NoContentResult();
        }

        [FunctionName(nameof(DeleteLinks))]
        public async Task<IActionResult> DeleteLinks(
           [HttpTrigger(AuthorizationLevel.Function, "DELETE", Route = "links")] HttpRequest req,
           [CosmosDB(ConnectionStringSetting = "DB_CONNECTION_STRING")] DocumentClient docClient,
           Binder binder,
           ILogger log)
        {
            var claimsPrincipal = StaticWebAppsAuth.Parse(req);

            //not logged in? Bye...
            if (claimsPrincipal.Identity == null) return new UnauthorizedResult();

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            IEnumerable<string> vanityUrls = JsonConvert.DeserializeObject<IEnumerable<string>>(requestBody);
            string queryValues = string.Join(",", vanityUrls.Select(url => $"\"{url}\""));

            log.LogInformation($"Request to remove the following collections: {queryValues}");
            string sql = $"SELECT c._self, c.userId c.vanityUrl from c WHERE c.vanityUrl IN ({queryValues}) ";

            int deleteCount = 0;
            string resultMessage = string.Empty;

            try
            {
                FeedOptions feedOpts = new FeedOptions { EnableCrossPartitionQuery = true };
                Uri collUri = UriFactory.CreateDocumentCollectionUri("linkylinkdb", "linkbundles");
                var docQuery = docClient.CreateDocumentQuery(collUri, sql, feedOpts).AsDocumentQuery();

                while (docQuery.HasMoreResults)
                {
                    var docs = await docQuery.ExecuteNextAsync();
                    foreach (var doc in docs)
                    {
                        string userId = doc.GetPropertyValue<string>("userId");
                        string vanityUrl = doc.GetPropertyValue<string>("vanityUrl");

                        if (!claimsPrincipal.Identity.Name.Equals(userId, StringComparison.InvariantCultureIgnoreCase))
                        {
                            log.LogWarning($"{userId} is trying to delete {vanityUrl} but is not the owner.");
                            log.LogWarning($"Skipping deletion of collection: {vanityUrl}.");
                            continue;
                        }
                        RequestOptions reqOptions = new RequestOptions { PartitionKey = new PartitionKey(doc.vanityUrl) };
                        await docClient.DeleteDocumentAsync(doc._self, reqOptions);
                        deleteCount++;
                    }
                }
                resultMessage = (deleteCount == vanityUrls.Count()) ? "All collections removed" : "Some colletions were not removed";
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
            return new OkObjectResult(new { deleted = deleteCount, message = resultMessage });
        }
    }
}