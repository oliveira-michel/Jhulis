using System.Collections.Generic;
using System.Linq;
using Jhulis.Core;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Jhulis.Core.Helpers.Extensions;

namespace Jhulis.Core.Rules
{
    public class Http3xxWithoutLocationHeaderRule : RuleBase
    {
        private const string ruleName = "Http3xxWithoutLocationHeader";
        private readonly OpenApiDocumentCache cache;

        /// <summary>
        /// Validate if a resource that returns some 3XX answers has Location header.
        /// Supressions: Rule,Path,Operation,ResponseCode
        /// </summary>
        public Http3xxWithoutLocationHeaderRule(OpenApiDocument contract,
            Supressions supressions, IOptions<RuleSettings> ruleSettings, OpenApiDocumentCache cache) : base(contract, supressions,
            ruleSettings, cache, ruleName, Severity.Warning)
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
                    responses.Key)) continue;

                foreach (OpenApiDocumentExtensions.Response response in responses)
                {
                    if (
                        (response.Name == "300" ||
                         response.Name == "301" ||
                         response.Name == "302" ||
                         response.Name == "303" ||
                         response.Name == "307")
                        && !response.OpenApiResponseObject.Headers.ContainsKey("Location")
                    )
                        listResult.Add(
                            new ResultItem(this)
                            {
                                Value =
                                    $"Path='{responses.First().Path}',Operation='{responses.First().Method}',ResponseCode='{response.Name}'"
                            });
                }
            }
        }
    }
}
