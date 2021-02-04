using Jhulis.Core;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Jhulis.Core.Helpers.Extensions;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Jhulis.Core.Rules
{
    public class HealthCheckRule : RuleBase
    {
        private const string ruleName = "HealthCheck";
        private static string[] healthCheckPaths;
        //No supressions

        /// <summary>
        /// Validate if the Health Check endpoint is present.
        /// Supressions: Rule
        /// </summary>
        public HealthCheckRule(OpenApiDocument contract, Supressions supressions,
            IOptions<RuleSettings> ruleSettings, OpenApiDocumentCache cache) :
            base(contract, supressions, ruleSettings, cache, ruleName, Severity.Hint)
        {
            healthCheckPaths = ruleSettings.Value.HealthCheckPaths.Paths.Split(',');
        }

        private protected override void ExecuteRuleLogic()
        {
            if (!ContainsHealthCheck())
            {
                listResult.Add(new ResultItem(this));
            }
        }

        private bool ContainsHealthCheck()
        {
            foreach (KeyValuePair<string, OpenApiPathItem> path in Contract.Paths)
            {
                foreach (var healthPath in healthCheckPaths)
                {
                    if (path.Key.ToLowerInvariant().Contains(healthPath))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}