using System.Collections.Generic;
using System.Linq;
using Jhulis.Core;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Jhulis.Core.Helpers.Extensions;

namespace Jhulis.Core.Rules
{
    public class MessagesEnvelopeFormatRule : RuleBase
    {
        private const string ruleName = "MessagesEnvelopeFormat";
        private readonly string[] propertiesInMessages;
        private static string messagesEnvelopePropertyName;
        
        /// <summary>
        /// Validate the messages envelope.
        /// Supressions: Rule,Path,Operation,ResponseCode,PropertyFull
        /// </summary>
        public MessagesEnvelopeFormatRule(OpenApiDocument contract,
            Supressions supressions, IOptions<RuleSettings> ruleSettings, OpenApiDocumentCache cache) : base(contract, supressions,
            ruleSettings, cache, ruleName, Severity.Warning)
        {
            propertiesInMessages = ruleSettings.Value.MessagesEnvelopeFormat.MessagesEnvelopePropertyName.Split(',');
            messagesEnvelopePropertyName = ruleSettings.Value.MessagesEnvelopeFormat.MessagesEnvelopePropertyName;
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

                if (
                    property.FullName.StartsWith($"{messagesEnvelopePropertyName}.")
                    && property.Name != messagesEnvelopePropertyName
                    && properties
                        .Any(prop =>
                                !propertiesInMessages
                                    .Contains(prop.Name) //any property not contained in pagination ones
                        )
                )
                {
                    listResult.Add(
                        new ResultItem(this)
                        {
                            Description = Description.Replace("{0}", messagesEnvelopePropertyName),
                            Details = Details
                                .Replace("{0}", messagesEnvelopePropertyName)
                                .Replace("{1}", string.Join(", ", propertiesInMessages)),
                            Value =
                                $"Path='{properties.First().Path}',Operation='{properties.First().Operation}',ResponseCode='{properties.First().ResponseCode}'"
                        });
                    break;
                }
            }
        }
    }
}
