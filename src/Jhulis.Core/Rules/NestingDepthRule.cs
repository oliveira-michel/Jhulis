using System;
using Jhulis.Core;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Jhulis.Core.Helpers.Extensions;

namespace Jhulis.Core.Rules
{
    public class NestingDepthRule : RuleBase
    {
        private const string ruleName = "NestingDepth";
        private static int maxDepth;

        /// <summary>
        /// Validate the properties case type.
        /// Supressions: Rule,Path,Operation,ResponseCode,PropertyFull
        /// </summary>
        public  NestingDepthRule(OpenApiDocument contract, Supressions supressions,
            IOptions<RuleSettings> ruleSettings, OpenApiDocumentCache cache)
            : base(contract, supressions, ruleSettings, cache, ruleName, Severity.Warning)
        {
            maxDepth = ruleSettings.Value.NestingDepth.Depth;
        }

        private protected override void ExecuteRuleLogic()
        {
            foreach (OpenApiDocumentExtensions.Property property in Contract.GetAllBodyProperties(RuleSettings, Cache))
            {
                if (Supressions.IsSupressed(ruleName, property.Path, property.Operation, string.Empty,
                    property.ResponseCode, property.FullName)) continue;

                if (property.HittedMaxDepth)
                    listResult.Add(
                        new ResultItem(this)
                        {
                            Details = Details.Replace("{0}", Convert.ToString(maxDepth)),
                            Value = property.ResultLocation()
                        });
            }
        }
    }
}
