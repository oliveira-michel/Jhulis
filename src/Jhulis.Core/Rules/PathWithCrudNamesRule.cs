using System.Collections.Generic;
using System.Linq;
using Jhulis.Core;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Jhulis.Core.Helpers.Extensions;

namespace Jhulis.Core.Rules
{
    public class PathWithCrudNamesRule : RuleBase
    {
        private const string ruleName = "PathWithCrudNames";
        private static string[] wordsToAvoid;

        /// <summary>
        /// Validate if paths have names that represents CRUD actions.
        /// Supressions: Rule,Path
        /// </summary>
        public PathWithCrudNamesRule(OpenApiDocument contract, Supressions supressions,
            IOptions<RuleSettings> ruleSettings, OpenApiDocumentCache cache) :
            base(contract, supressions, ruleSettings, cache, ruleName, Severity.Warning)
        {
            wordsToAvoid = ruleSettings.Value.PathWithCrudNames.WordsToAvoid.Split(',');
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
                    if (pathSegment.Contains("{") || string.IsNullOrEmpty(pathSegment)) continue;

                    foreach (string word in pathSegment.SplitCompositeWord())
                    {
                        if (wordsToAvoid.Contains(word.ToLowerInvariant()))
                        {
                            listResult.Add(
                                new ResultItem(this) {Value = $"Path='{path.Key}',PathSegment='{pathSegment}'"});
                        }
                    }
                }
            }
        }
    }
}
