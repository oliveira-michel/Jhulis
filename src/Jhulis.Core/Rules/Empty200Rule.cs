using System.Collections.Generic;
using System.Linq;
using Jhulis.Core;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Jhulis.Core.Helpers.Extensions;

namespace Jhulis.Core.Rules
{
    public class Empty200Rule : RuleBase
    {
        private const string ruleName = "Empty200";
        private readonly OpenApiDocumentCache cache;

        /// <summary>
        /// Validates if 200 or 206 HTTP Status codes are wrongly with empty content as answer.
        /// Supressions: Rule,Path,Operation,ResponseCode
        /// </summary>
        public Empty200Rule(OpenApiDocument contract, Supressions supressions,
            IOptions<RuleSettings> ruleSettings, OpenApiDocumentCache cache) :
            base(contract, supressions, ruleSettings, cache, ruleName, Severity.Warning)
        {
            this.cache = cache;
        }

        private protected override void ExecuteRuleLogic()
        {
            IEnumerable<IGrouping<string, OpenApiDocumentExtensions.Response>> responsesByPath = Contract
                .GetAllResponses(cache).GroupBy(
                    resp => resp.Path + "|" + resp.Method,
                    resp => resp);

            foreach (IGrouping<string, OpenApiDocumentExtensions.Response> responses in responsesByPath)
            {
                if (Supressions.IsSupressed(ruleName, responses.First().Path, responses.First().Method, string.Empty,
                    responses.First().Name)) continue;

                if (
                    responses.Any(resp => (resp.Name == "200" || resp.Name == "206")
                        && resp.OpenApiResponseObject.Content.Any(content =>
                            content.Value.Schema == null || content.Value.Schema.Properties.Count == 0))
                )
                    listResult.Add(
                        new ResultItem(this)
                            {Value = $"Path='{responses.First().Path}',Operation='{responses.First().Method}'"});
            }
        }
    }
}
