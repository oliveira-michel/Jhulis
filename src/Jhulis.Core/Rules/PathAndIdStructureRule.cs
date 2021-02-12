using System.Collections.Generic;
using Jhulis.Core;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Jhulis.Core.Helpers.Extensions;

namespace Jhulis.Core.Rules
{
    public class PathAndIdStructureRule : RuleBase
    {
        private const string ruleName = "PathAndIDStructure";

        /// <summary>
        /// Validate if the structure is alternating collection and id like this:  /galaxy/{idGalaxy}/planets/{idPlanet}/countries/{idCountry}.
        /// Supressions: Rule,Path
        /// </summary>
        public PathAndIdStructureRule(OpenApiDocument contract,
            Supressions supressions, IOptions<RuleSettings> ruleSettings, OpenApiDocumentCache cache) : base(contract, supressions,
            ruleSettings, cache, ruleName, Severity.Warning)
        {
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

                if (pathSplited.Length > 1 && pathSplited[0].Contains("{")) //first segment cannot be an parameter.
                    listResult.Add(
                        new ResultItem(this, path:path.Key));

                for (var i = 0; i < pathSplited.Length - 1; i++) //go until the penultimate
                {
                    bool actualIsParameter = pathSplited[i].Contains("{");
                    bool nextIsParameter = pathSplited[i + 1].Contains("{");
                    if (!(actualIsParameter ^ nextIsParameter)
                    ) //cannot have two non-parameters or two parameters path segments together
                        listResult.Add(
                            new ResultItem(this, path:path.Key, pathSegment: pathSplited[0]));
                }
            }
        }
    }
}
