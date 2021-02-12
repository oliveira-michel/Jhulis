using System.Collections.Generic;
using System.Linq;
using Jhulis.Core;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Jhulis.Core.Helpers.Extensions;

namespace Jhulis.Core.Rules
{
    public class OperationSuccessResponseRule : RuleBase
    {
        private const string ruleName = "OperationSuccessResponse";

        /// <summary>
        /// Validate if there are at least one 2xx response on the path.
        /// Supressions: Rule,Path,Operation
        /// </summary>
        public OperationSuccessResponseRule(OpenApiDocument contract,
            Supressions supressions, IOptions<RuleSettings> ruleSettings, OpenApiDocumentCache cache) : base(contract, supressions,
            ruleSettings, cache, ruleName, Severity.Warning)
        {
        }

        private protected override void ExecuteRuleLogic()
        {
            IEnumerable<IGrouping<string, OpenApiDocumentExtensions.Response>> responsesByPath = Contract
                .GetAllResponses(Cache).GroupBy(
                    resp => resp.Path + "|" + resp.Method,
                    resp => resp);

            foreach (IGrouping<string, OpenApiDocumentExtensions.Response> responses in responsesByPath)
            {
                if (Supressions.IsSupressed(ruleName, responses.First().Path, responses.First().Method,
                    string.Empty)) continue;

                if (!responses.Any(resp => resp.Name.StartsWith("2")))
                    listResult.Add(
                        new ResultItem(this, path:responses.First().Path, method:responses.First().Method));
            }
        }
    }
}
