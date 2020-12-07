using System.Collections.Generic;
using System.Linq;
using Jhulis.Core;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Jhulis.Core.Helpers.Extensions;

namespace Jhulis.Core.Rules
{
    public class Http201WithoutLocationHeaderRule : RuleBase
    {
        private const string ruleName = "Http201WithoutLocationHeader";
        private readonly OpenApiDocumentCache cache;

        /// <summary>
        /// Validate if a resource that returns some 201 answers has Content-Location header.
        /// Supressions: Rule,Path,Operation
        /// </summary>
        public Http201WithoutLocationHeaderRule(OpenApiDocument contract,
            Supressions supressions, IOptions<RuleSettings> ruleSettings, OpenApiDocumentCache cache) :
            base(contract, supressions, ruleSettings, cache, ruleName, Severity.Warning)
        {
            this.cache = cache;
        }

        private protected override void ExecuteRuleLogic()
        {
            IEnumerable<IGrouping<string, OpenApiDocumentExtensions.Response>> responsesByPath = Contract
                .GetAllResponses(cache).GroupBy(
                    resp => resp.Path + "|" + resp.Operation,
                    resp => resp);

            foreach (IGrouping<string, OpenApiDocumentExtensions.Response> responses in responsesByPath)
            {
                if (Supressions.IsSupressed(ruleName, responses.First().Path, responses.First().Operation)) continue;

                foreach (OpenApiDocumentExtensions.Response response in responses)
                {
                    if (
                        (response.Name == "201")
                        && !response.OpenApiResponseObject.Headers.ContainsKey("Location")
                    )
                        listResult.Add(
                            new ResultItem(this)
                            {
                                Value =
                                    $"Path='{responses.First().Path}',Operation='{responses.First().Operation}',ResponseCode='{response.Name}'"
                            });
                }
            }
        }
    }
}
