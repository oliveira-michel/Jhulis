using Jhulis.Core;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Jhulis.Core.Helpers.Extensions;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text.RegularExpressions;

namespace Jhulis.Core.Rules
{
    public class HealthCheckRule : RuleBase
    {
        private const string ruleName = "HealthCheck";
        private static Regex healthCheckRegex;
        //No supressions

        /// <summary>
        /// Validate if the Health Check endpoint is present.
        /// Supressions: Rule
        /// </summary>
        public HealthCheckRule(OpenApiDocument contract, Supressions supressions,
            IOptions<RuleSettings> ruleSettings, OpenApiDocumentCache cache) :
            base(contract, supressions, ruleSettings, cache, ruleName, Severity.Hint)
        {
            healthCheckRegex = new Regex($@"{ruleSettings.Value.HealthCheck.Regex}", RegexOptions.Compiled);
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
                string[] pathSplited =
                    path.Key.StartsWith('/')
                        ? path.Key.Substring(1).Split('/')
                        : path.Key.Split('/');

                foreach (string pathSegment in pathSplited)
                {
                    if (pathSegment.Contains("{") || string.IsNullOrEmpty(pathSegment)) continue;

                    foreach (string word in pathSegment.SplitCompositeWord())
                    {
                        if (healthCheckRegex.IsMatch(word))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}