using System;
using System.Threading;
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
    }
}