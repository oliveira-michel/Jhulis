using System.Collections.Generic;
using Jhulis.Core;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Jhulis.Core.Helpers.Extensions;
using System;

namespace Jhulis.Core.Rules
{
    public class PathPluralRule : RuleBase
    {
        private const string ruleName = "PathPlural";
        private readonly string[] exceptions;

        /// <summary>
        /// Validate if collections in path are plural (aka finishing with 's').
        /// Supressions: Rule,Path
        /// </summary>
        public PathPluralRule(OpenApiDocument contract, Supressions supressions,
            IOptions<RuleSettings> ruleSettings, OpenApiDocumentCache cache) :
            base(contract, supressions, ruleSettings, cache, ruleName, Severity.Warning)
        {
            exceptions = ruleSettings.Value.PathPlural?.Exceptions?.Split();
        }

        private protected override void ExecuteRuleLogic()
        {
            

            foreach (KeyValuePair<string, OpenApiPathItem> path in Contract.Paths)
            {
                if (Supressions.IsSupressed(ruleName, path.Key)) continue;

                string[] pathSplited =
                    path.Key.StartsWith('/')
                        ? path.Key.Substring(1).Split('/')
                        : path.Key.Split('/');

                foreach (string pathSegment in pathSplited)
                {
                    if (Array.Exists(exceptions, x => x == pathSegment)) continue;                

                    if (pathSegment.Contains("{") || string.IsNullOrEmpty(pathSegment)) continue;

                    var hasOneWordFinishingWithS = false;
                    foreach (string word in pathSegment.SplitCompositeWord())
                    {
                        if (word.EndsWith("s"))
                        {
                            hasOneWordFinishingWithS = true;
                            break;
                        }
                    }

                    if (!hasOneWordFinishingWithS)
                        listResult.Add(
                            new ResultItem(this, path:path.Key, pathSegment: pathSegment));
                }
            }
        }
    }
}
