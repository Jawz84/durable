using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace KBB.Corrigeer
{
    public partial class Durable
    {
        [FunctionName(ActionCheckCsvIntegrity)]
        public static string CheckCsvIntegrity([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"Checking Csv integrity {name}.");
            Thread.Sleep(2000);
            return $"Csv integrity {name}!";
        }

        [FunctionName(ActionValidateSqlConstraints)]
        public static string ValidateSqlConstraints([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"Validating SQL constraints {name}.");
            Thread.Sleep(5000);

            // throw new Exception("SQL constraints validation failed!");
            // SimulateRandomErrorOrSuccess(name);
            return $"Validate constraints {name}!";
        }

        private static void SimulateRandomErrorOrSuccess(string name)
        {
            var rnd = new Random();
            var shouldThrow = rnd.Next(0, 2) == 0;
            if (shouldThrow)
            {
                throw new Exception($"SQL constraints {name} failed!");
            }
        }

        [FunctionName(ActionPerformSqlCorrection)]
        public static string PerformSqlCorrection([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"Performing corrections on SQL database {name}.");
            return $"Perform corrections {name}!";
        }

        [FunctionName(SubOrchestratorStartSynapsePipeline)]
        public static async Task RunSubOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            string subscriptionId = "mySubId";
            string resourceGroup = "myRG";
            string factoryName = "mySynapse";
            string apiVersion = "2021-06-01";
            string pipelineName = "myPipeline";

            log.LogInformation($"Starting Synapse pipeline {pipelineName}.");

            // Automatically fetches an Azure AD token for resource = https://management.core.windows.net/.default
            // and attaches it to the outgoing Azure Resource Manager API call.
            var uri = new Uri(
                $"https://management.azure.com/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.DataFactory/factories/{factoryName}/pipelines/{pipelineName}/createRun?api-version={apiVersion}");
            var tokenSource = new ManagedIdentityTokenSource(
                "https://management.core.windows.net/.default");
            var startRequest = new DurableHttpRequest(
                HttpMethod.Post,
                uri,
                tokenSource: tokenSource);

            DurableHttpResponse startResponse = await context.CallHttpAsync(startRequest);

            if (startResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new ArgumentException($"Failed to start Synapse pipeline '{pipelineName}': {startResponse.StatusCode}: {startResponse.Content}");
            }
        }
    }
}