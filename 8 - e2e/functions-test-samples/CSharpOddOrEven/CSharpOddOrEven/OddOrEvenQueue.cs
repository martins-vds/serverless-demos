using System;
using System.Net.Http;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace CSharpOddOrEven
{
    public static class OddOrEvenQueue
    {
        public static HttpClient client = new HttpClient();
		private static string apiEndpoint = "endpoint";

        [FunctionName("OddOrEvenQueue")]
        public static async Task RunAsync(
            [QueueTrigger("numbers", Connection = "AzureWebJobsStorage")]string myNumber, 
            ILogger log)
        {
            log.LogInformation($"Odd or even trigger fired - Queue");

            if (BigInteger.TryParse(myNumber, out BigInteger number))
            {
                if (number % 2 == 0)
                {
                    log.LogInformation("Was even");
                    await client.PostAsync(apiEndpoint, new StringContent("Even"));
                }
                else
                {
                    log.LogInformation("Was odd");
                    await client.PostAsync(apiEndpoint, new StringContent("Odd"));
                }
            }
            else
            {
                throw new ArgumentException($"Unable to parse the queue message. Got value: {myNumber}");
            }
        }
    }
}
