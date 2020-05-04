using System.Linq;
using Jhulis.Core;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Jhulis.Core.Helpers.Extensions;

namespace Jhulis.Core.Rules
{
    public class ValidResponseCodesRule : RuleBase
    {
        private const string ruleName = "ValidResponseCodes";
        private static string[] validHttpCodes;

        /// <summary>
        /// Validate if the response codes are valids.
        /// Supressions: Rule,Path,Operation,ResponseCode
        /// </summary>
        public ValidResponseCodesRule(OpenApiDocument contract,
            Supressions supressions, IOptions<RuleSettings> ruleSettings, OpenApiDocumentCache cache) : base(contract, supressions,
            ruleSettings, cache, ruleName, Severity.Warning)
        {
            validHttpCodes = ruleSettings.Value.ValidResponseCodes.ValidHttpCodes.Split(',');
        }

        private protected override void ExecuteRuleLogic()
        {
            foreach (OpenApiDocumentExtensions.Response response in Contract.GetAllResponses(Cache))
            {
                if (Supressions.IsSupressed(ruleName, response.Path, response.Operation, string.Empty,
                    response.Name)) continue;

                if (!validHttpCodes.Contains(response.Name))
                    listResult.Add(
                        new ResultItem(this)
                        {
                            Value =
                                $"Path='{response.Path}',Operation='{response.Operation}',ResponseCode='{response.Name}'"
                        });
            }
        }
    }
}
