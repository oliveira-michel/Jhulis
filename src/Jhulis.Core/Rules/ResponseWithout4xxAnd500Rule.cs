using System.Collections.Generic;
using System.Linq;
using Jhulis.Core;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Jhulis.Core.Helpers.Extensions;

namespace Jhulis.Core.Rules
{
    public class ResponseWithout4xxAnd500Rule : RuleBase
    {
        private const string ruleName = "ResponseWithout4xxAnd500";

        /// <summary>
        /// Validate if a resource has at least one 4xx and a 500 response.
        /// Supressions: Rule,Path,Operation,ResponseCode
        /// </summary>
        public ResponseWithout4xxAnd500Rule(OpenApiDocument contract,
            Supressions supressions, IOptions<RuleSettings> ruleSettings, OpenApiDocumentCache cache) : base(contract, supressions,
            ruleSettings, cache, ruleName, Severity.Warning)
        {
        }

        private protected override void ExecuteRuleLogic()
        {
            IEnumerable<IGrouping<string, OpenApiDocumentExtensions.Response>> responsesByPath = Contract
                .GetAllResponses(Cache).GroupBy(
                    resp => resp.Path + "|" + resp.Operation,
                    resp => resp);

            foreach (IGrouping<string, OpenApiDocumentExtensions.Response> responses in responsesByPath)
            {
                if (Supressions.IsSupressed(ruleName, responses.First().Path, responses.First().Operation, string.Empty,
                    responses.Key)) continue;

                if (responses.Any(resp => (resp.Name == "500" &&
                                           resp.OpenApiResponseObject
                                               .Content
                                               .Any(content =>
                                                   (content.Value.Schema != null
                                                    && content.Value.Schema.Properties.Count != 0))
                        )
                    )
                    && responses.Any(resp => (resp.Name.StartsWith("4")
                                              && resp.OpenApiResponseObject
                                                  .Content
                                                  .Any(content =>
                                                      (content.Value.Schema != null &&
                                                       content.Value.Schema.Properties.Count != 0))
                        )
                    )
                )
                    continue;

                listResult.Add(
                    new ResultItem(this)
                    {
                        Value =
                            $"Path='{responses.First().Path}',Operation='{responses.First().Operation}'"
                    });
            }
        }
    }
}
