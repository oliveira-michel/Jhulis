using System.Collections.Generic;
using System.Linq;
using Jhulis.Core;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Jhulis.Core.Helpers.Extensions;

namespace Jhulis.Core.Rules
{
    public class IdPropertyResponseRule : RuleBase
    {
        private const string ruleName = "IdPropertyResponse";
        private readonly string idPropertyName;

        /// <summary>
        /// Validate if a resource has an id property on response.
        /// Supressions: Rule,Path,Operation,ResponseCode
        /// </summary>
        public IdPropertyResponseRule(OpenApiDocument contract,
            Supressions supressions, IOptions<RuleSettings> ruleSettings, OpenApiDocumentCache cache) : base(contract, supressions,
            ruleSettings, cache, ruleName, Severity.Warning)
        {
            idPropertyName = ruleSettings.Value.IdPropertyResponse.IdPropertyName;
        }

        private protected override void ExecuteRuleLogic()
        {
            IEnumerable<IGrouping<string, OpenApiDocumentExtensions.Property>> propertiesByPath = Contract
                .GetAllBodyProperties(RuleSettings, Cache).GroupBy(
                    prop => prop.Path + "|" + prop.Operation + "|" + prop.ResponseCode + "|" + prop.Content,
                    prop => prop);

            foreach (IGrouping<string, OpenApiDocumentExtensions.Property> properties in propertiesByPath)
            {
                if (Supressions.IsSupressed(ruleName, properties.First().Path, properties.First().Operation,
                    string.Empty,
                    properties.First().ResponseCode)) continue;

                if (
                    properties.First().ResponseCode != "204" && (properties.First().ResponseCode.StartsWith("2") || properties.First().ResponseCode.StartsWith("3"))
                    && properties.All(prop => prop.Name != idPropertyName))
                    listResult.Add(
                        new ResultItem(this)
                        {
                            Description = Description.Replace("{0}", idPropertyName),
                            Details = Details.Replace("{0}", idPropertyName),
                            Value =
                                $"Path='{properties.First().Path}',Operation='{properties.First().Operation}',ResponseCode='{properties.First().ResponseCode}'"
                        });
            }
        }
    }
}
