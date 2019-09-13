using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;

namespace ReviewEvaluationFunctionApp
{
    public class ApiKeyServiceClientCredentials : ServiceClientCredentials
    {
        public override Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Add("Ocp-Apim-Subscription-Key", "209b41ab6e344042bda931427ed3f7a2");
            return base.ProcessHttpRequestAsync(request, cancellationToken);
        }
    }

    public static class ReviewEvaluationFunction
    {
        [FunctionName("ReviewEvaluationFunction")]
        public static async Task Run([CosmosDBTrigger(
            databaseName: "EY",
            collectionName: "Reviews",
            ConnectionStringSetting = "CosmosDbConnectionString",
            LeaseCollectionName = "leases",
            CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<Document> documents,

            [CosmosDB(databaseName: "EY",
                collectionName: "Reviews",
                ConnectionStringSetting = "CosmosDbConnectionString",
                CreateIfNotExists = false)]IAsyncCollector<dynamic> results,
            ILogger log)
        {
            log.LogInformation($"Function triggered.");

            if (documents != null && documents.Count > 0)
            {
                log.LogInformation($"Documents modified  {documents.Count}");
                log.LogInformation($"First document Id { documents[0].Id}");

                ITextAnalyticsClient client = new TextAnalyticsClient(new ApiKeyServiceClientCredentials())
                {
                    Endpoint = "https://westcentralus.api.cognitive.microsoft.com"
                };

                string languageToAnalyze = "en";
                int cnt = 0;
                foreach (var document in documents)
                {
                    if (!string.IsNullOrEmpty(document.GetPropertyValue<string>("Satisfaction")))
                        continue;
                    var content = document.GetPropertyValue<string>("Content");
                    SentimentBatchResult result = await client.SentimentAsync(multiLanguageBatchInput:
                        new MultiLanguageBatchInput(
                            new List<MultiLanguageInput>
                            {
                                new MultiLanguageInput(languageToAnalyze, id: cnt.ToString(), text: content)
                            }
                        )
                    );
                    cnt++;
                    var evaluationResult = result.Documents[0].Score;
                    var newDocument = new
                    {
                        id = document.Id,
                        Content = content,
                        Satisfaction = evaluationResult
                    };

                    await results.AddAsync(newDocument);
                    log.LogInformation($"Review evaluated: {content}");
                    log.LogInformation($"Evaluation result: {evaluationResult}");
                }
            }
        }
    }
}