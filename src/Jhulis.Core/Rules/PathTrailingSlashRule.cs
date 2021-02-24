using System.Collections.Generic;
using System.Linq;
using Jhulis.Core;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Jhulis.Core.Helpers.Extensions;

namespace Jhulis.Core.Rules
{
    public class PathTrailingSlashRule : RuleBase
    {
        private const string ruleName = "PathTrailingSlash";

        /// <summary>
        /// Validate if patches are, correctly, do not finishing with slash.
        /// Supressions: Rule,Path
        /// </summary>        
        public PathTrailingSlashRule(OpenApiDocument contract,
            Supressions supressions, IOptions<RuleSettings> ruleSettings, OpenApiDocumentCache cache) : base(contract, supressions,
            ruleSettings, cache, ruleName, Severity.Error)
        {
        }

        private protected override void ExecuteRuleLogic()
        {
            foreach (KeyValuePair<string, OpenApiPathItem> path in Contract.Paths)
            {
                if (Supressions.IsSupressed(ruleName, path.Key)) continue;

                string[] pathSplited = path.Key.Split('/');

                if (string.IsNullOrWhiteSpace(pathSplited.Last()))
                    listResult.Add(
                        new ResultItem(this, path: path.Key));
            }
        }
    }
}
