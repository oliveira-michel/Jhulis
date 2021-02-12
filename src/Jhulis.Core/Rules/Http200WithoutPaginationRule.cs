using System.Collections.Generic;
using System.Linq;
using Jhulis.Core;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Jhulis.Core.Helpers.Extensions;

namespace Jhulis.Core.Rules
{
    public class Http200WithoutPaginationRule : RuleBase
    {
        private const string ruleName = "Http200WithoutPagination";
        private static string paginationEnvelopeName;
        private static string contentEnvelopeName;

        /// <summary>
        /// Validate if a resource with GET have any pagination answer.
        /// Supressions: Rule,Path
        /// </summary>
        public Http200WithoutPaginationRule(OpenApiDocument contract,
            Supressions supressions, IOptions<RuleSettings> ruleSettings, OpenApiDocumentCache cache) : base(contract, supressions,
            ruleSettings, cache, ruleName, Severity.Hint)
        {
            paginationEnvelopeName = ruleSettings.Value.Http200WithoutPagination.PaginationEnvelopeName;
            contentEnvelopeName = ruleSettings.Value.Http200WithoutPagination.ContentEnvelopeName;
        }

        private protected override void ExecuteRuleLogic()
        {
            //Paths {contentEnvelopeName} = [] && 2xx && get.
            IEnumerable<IGrouping<string, OpenApiDocumentExtensions.Property>> responseAndPathWithContents =
                Contract.GetAllBodyProperties(RuleSettings, Cache)
                    .Where(pathContent => pathContent.Operation == "get" &&
                                         pathContent.ResponseCode.StartsWith("2"))
                    .GroupBy(resp => $"{resp.ResponseCode} | {resp.Path}",
                        resp => resp)
                    .Where(path => path.Any(prop =>
                        prop.FullName == contentEnvelopeName && prop.OpenApiSchemaObject.Type == "array"));

            foreach (IGrouping<string, OpenApiDocumentExtensions.Property> responses in responseAndPathWithContents)
            {
                if (Supressions.IsSupressed(ruleName, responses.First().Path)) continue;

                if (responses.All(resp => resp.FullName != paginationEnvelopeName))
                    listResult.Add(
                        new ResultItem(this, path: responses.First().Path));
            }
        }
    }
}
