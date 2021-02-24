using System.Collections.Generic;
using System.Linq;
using Jhulis.Core;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Jhulis.Core.Helpers.Extensions;

namespace Jhulis.Core.Rules
{
    public class ContentIn204Rule : RuleBase
    {
        private const string ruleName = "ContentIn204";

        /// <summary>
        /// Validates if there are any 204 response code with some content.
        /// Supressions: Rule,Path,Operation
        /// </summary>
        public ContentIn204Rule(OpenApiDocument contract, Supressions supressions,
            IOptions<RuleSettings> ruleSettings, OpenApiDocumentCache cache)
            : base(contract, supressions, ruleSettings, cache, ruleName, Severity.Warning)
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
                if (Supressions.IsSupressed(ruleName, responses.First().Path, responses.First().Method)) continue;

                if (
                    responses.Any(resp => resp.Name == "204"
                                          && resp.OpenApiResponseObject.Content.Values
                                              .Any(content => content.Schema != null)))
                    listResult.Add(
                        new ResultItem(this, path: responses.First().Path, method: responses.First().Method));
            }
        }
    }
}
