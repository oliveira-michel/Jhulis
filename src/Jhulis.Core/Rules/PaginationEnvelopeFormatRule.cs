using System.Collections.Generic;
using System.Linq;
using Jhulis.Core;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Jhulis.Core.Helpers.Extensions;

namespace Jhulis.Core.Rules
{
    public class PaginationEnvelopeFormatRule : RuleBase
    {
        private const string ruleName = "PaginationEnvelopeFormat";
        private static string[] propertiesInPagination;
        private static string paginationEnvelopePropertyName;

        /// <summary>
        /// Validate the pagination envelope.
        /// Supressions: Rule,Path,Operation,ResponseCode,PropertyFull
        /// </summary>
        public PaginationEnvelopeFormatRule(OpenApiDocument contract,
            Supressions supressions, IOptions<RuleSettings> ruleSettings, OpenApiDocumentCache cache) : base(contract, supressions,
            ruleSettings, cache, ruleName, Severity.Warning)
        {
            propertiesInPagination = ruleSettings.Value.PaginationEnvelopeFormat.PropertiesInPagination.Split(',');
            paginationEnvelopePropertyName = ruleSettings.Value.PaginationEnvelopeFormat.PaginationEnvelopePropertyName;
        }

        private protected override void ExecuteRuleLogic()
        {
            IEnumerable<IGrouping<string, OpenApiDocumentExtensions.Property>> propertiesByPath = Contract
                .GetAllBodyProperties(RuleSettings, Cache).GroupBy(
                    prop => prop.Path + "|" + prop.Operation + "|" + prop.ResponseCode + "|" + prop.Content,
                    prop => prop);

            foreach (IGrouping<string, OpenApiDocumentExtensions.Property> properties in propertiesByPath)
            foreach (OpenApiDocumentExtensions.Property property in properties)
            {
                if (Supressions.IsSupressed(ruleName, property.Path, property.Operation, string.Empty,
                    property.ResponseCode, property.FullName)) continue;

                if (//if property is into the pagination property
                    property.FullName.StartsWith($"{paginationEnvelopePropertyName}.")
                    && !propertiesInPagination.Contains(property.Name) //any is not contained in pagination ones
                )
                {
                    listResult.Add(
                        new ResultItem(this)
                        {
                            Description = Description.Replace("{0}", paginationEnvelopePropertyName),
                            Details = Details
                                .Replace("{0}", paginationEnvelopePropertyName)
                                .Replace("{1}", string.Join(", ", propertiesInPagination)),
                            Value =
                                $"Path='{properties.First().Path}',Operation='{properties.First().Operation}',ResponseCode='{properties.First().ResponseCode}',Content='{property.Content}',PropertyFull='{property.FullName}'"
                        });
                    break;
                }
            }
        }
    }
}
