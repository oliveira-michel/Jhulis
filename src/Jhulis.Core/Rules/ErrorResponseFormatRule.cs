using System.Collections.Generic;
using System.Linq;
using Jhulis.Core;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Jhulis.Core.Helpers.Extensions;

namespace Jhulis.Core.Rules
{
    public class ErrorResponseFormatRule : RuleBase
    {
        private const string ruleName = "ErrorResponseFormat";
        private static string[] obligatoryErrorProperties;
        private static string[] nonObligatoryErrorProperties;

        /// <summary>
        /// Validate if HTTP 4xx or 5xx error responses are in compliance with an pre defined structure.
        /// Supressions: Rule,Path,Operation,ResponseCode,PropertyFull
        /// </summary>
        public ErrorResponseFormatRule(OpenApiDocument contract,
            Supressions supressions, IOptions<RuleSettings> ruleSettings, OpenApiDocumentCache cache) : base(contract, supressions,
            ruleSettings, cache, ruleName, Severity.Warning)
        {
            obligatoryErrorProperties = ruleSettings.Value.ErrorResponseFormat.ObligatoryErrorProperties.Split(',');
            nonObligatoryErrorProperties = ruleSettings.Value.ErrorResponseFormat.NonObligatoryErrorProperties.Split(',');
        }

        private protected override void ExecuteRuleLogic()
        {
            IEnumerable<IGrouping<string, OpenApiDocumentExtensions.Property>> propertiesByPath = Contract
                .GetAllBodyProperties(RuleSettings, Cache).GroupBy(
                    prop => prop.Path + "|" + prop.Operation + "|" + prop.ResponseCode + "|" + prop.Content,
                    prop => prop);

            foreach (IGrouping<string, OpenApiDocumentExtensions.Property> properties in propertiesByPath)
                foreach (OpenApiDocumentExtensions.Property property in properties)
                //            foreach (OpenApiDocumentExtensions.Property property in contract.GetAllBodyProperties())
                {
                    if (Supressions.IsSupressed(ruleName, property.Path, property.Operation, string.Empty,
                        property.ResponseCode, property.FullName)) continue;

                    if (
                        property.InsideOf == OpenApiDocumentExtensions.Property.BodyType.Response &&
                        property.ResponseCode != null &&
                        (property.ResponseCode.StartsWith("4") || property.ResponseCode.StartsWith("5"))
                        && //There aren't any property not contained on the default ones.
                        obligatoryErrorProperties
                            .Concat(nonObligatoryErrorProperties)
                            .Except(properties.Select(p => p.FullName)).Any()
                        && //All obligatory properties are defined.
                        obligatoryErrorProperties.Intersect(properties.Select(p => p.FullName)).Count() !=
                        obligatoryErrorProperties.Length
                    )
                    {
                        listResult.Add(
                            new ResultItem(this)
                            {
                                Details = Details
                                    .Replace("{0}",
                                        string.Join(", ", obligatoryErrorProperties.Concat(nonObligatoryErrorProperties))),
                                Value = ResultItem.FormatValue(path:property.Path, method:property.Operation, response: property.ResponseCode)
                            });
                        break;
                    }
                }
        }
    }
}
