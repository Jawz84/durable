using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace KBB.Corrigeer
{
    public partial class Durable
    {
        private const string OrchestrateCorrigeerActions = "OrchestrateCorrigeerActions";
        private const string StartCorrectionOrchestrator = "StartCorrectionOrchestrator";

        private const string ActionCheckCsvIntegrity = "ActionCheckCsvIntegrity";
        private const string ActionValidateSqlConstraints = "ActionValidateSqlConstraints";
        private const string ActionPerformSqlCorrection = "ActionPerformSqlCorrection";
        private const string EventApproveCorrection = "EventApproveCorrection";
        private const string instanceId = "single-instance";

        [FunctionName(StartCorrectionOrchestrator)]
        public static async Task<HttpResponseMessage> CorrectionOrchestrator(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = StartCorrectionOrchestrator+"/{fileName}")] HttpRequestMessage req,
            // [BlobTrigger("files/{name}", Connection = "AzureWebJobsStorage")]Stream myBlob, string name,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log,
            string fileName)
        {
            // Check if an instance with the specified ID already exists or an existing one stopped running(completed/failed/terminated).
            var existingInstance = await starter.GetStatusAsync(instanceId);
            if (existingInstance == null
            || existingInstance.RuntimeStatus == OrchestrationRuntimeStatus.Completed
            || existingInstance.RuntimeStatus == OrchestrationRuntimeStatus.Failed
            || existingInstance.RuntimeStatus == OrchestrationRuntimeStatus.Terminated)
            {
                // An instance with the specified ID doesn't exist or an existing one stopped running, create one.
                // dynamic eventData = await req.Content.ReadAsAsync<object>() ?? null;
                // if eventData == empty, then check blob trigger info
                string eventData = fileName;
                await starter.StartNewAsync(OrchestrateCorrigeerActions, instanceId, eventData);
                log.LogInformation($"Started orchestration with ID = '{instanceId}'.");
                return starter.CreateCheckStatusResponse(req, instanceId);
            }
            else
            {
                // An instance with the specified ID exists or an existing one still running, don't create one.
                return new HttpResponseMessage(HttpStatusCode.Conflict)
                {
                    Content = new StringContent($"An instance with ID '{instanceId}' already exists."),
                };
            }
        }

        [FunctionName(OrchestrateCorrigeerActions)]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();
            context.SetCustomStatus("Checking Csv integrity");
            outputs.Add(await context.CallActivityAsync<string>(ActionCheckCsvIntegrity, context.GetInput<string>()));

            context.SetCustomStatus("Validating SQL constraints");
            // var retry = new RetryOptions(TimeSpan.FromSeconds(1), 1);
            // outputs.Add(await context.CallActivityWithRetryAsync<string>(ActionValidateSqlConstraints, retry, context.GetInput<string>()));
            outputs.Add(await context.CallActivityAsync<string>(ActionValidateSqlConstraints, context.GetInput<string>()));

            context.SetCustomStatus("Awaiting user approval");
            await context.WaitForExternalEvent(EventApproveCorrection);

            context.SetCustomStatus("Performing corrections");
            outputs.Add(await context.CallActivityAsync<string>(ActionPerformSqlCorrection, context.GetInput<string>()));

            context.SetCustomStatus($"Correction {context.InstanceId} completed");
            return outputs;
        }
    }
}