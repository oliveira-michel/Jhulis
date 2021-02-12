using System;
using System.Collections.Generic;
using System.Linq;
using Jhulis.Core;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Jhulis.Core.Helpers.Extensions;

namespace Jhulis.Core.Rules
{
    public class DateWithoutFormatRule : RuleBase
    {
        private const string ruleName = "DateWithoutFormat";

        /// <summary>
        /// Validates if parameters or attributes have examples indicating that the content is likely date type and the format is not date or date-time.
        /// Supressions: Rule,Path,Operation,ResponseCode,Content,PropertyFull
        /// </summary>
        public DateWithoutFormatRule(OpenApiDocument contract,
            Supressions supressions,
            IOptions<RuleSettings> ruleSettings, OpenApiDocumentCache cache)
            : base(contract, supressions, ruleSettings, cache, ruleName, Severity.Hint)
        {
        }

        private protected override void ExecuteRuleLogic()
        {

            var parameters = Contract.GetAllParameters();

            //Search for parameters in query, path or header with examples that value is likelihood date and the format attribute is not date or date-time.
            foreach (OpenApiDocumentExtensions.Parameter parameter in parameters)
            {
                if (Supressions.IsSupressed(ruleName, parameter.Path))
                    continue;

                if (Supressions.IsSupressed(ruleName, parameter.Path, parameter.Method))
                    continue;

                if (Supressions.IsSupressed(ruleName, parameter.Path, parameter.Method, parameter.Name))
                    continue;

                //Parameters in query, path or header
                if (parameter.OpenApiParameter.Example != null && parameter.OpenApiParameter.Examples?.Count > 0 &&
                    parameter.OpenApiParameter.Schema.Type.ToLower() == "string")
                {
                    if (parameter.OpenApiParameter.Example is OpenApiString exApiString &&
                        DateTime.TryParse(exApiString.Value, out _) &&
                        parameter.OpenApiParameter.Schema.Format != "date" &&
                        parameter.OpenApiParameter.Schema.Format != "date-time")
                    {
                        listResult.Add(new ResultItem(this) { Value = parameter.ResultLocation() });
                    }

                    foreach (KeyValuePair<string, OpenApiExample> example in parameter.OpenApiParameter.Examples)
                    {
                        if (parameter.OpenApiParameter.Schema.Type.ToLower() == "string" &&
                            example.Value.Value is OpenApiString apiString &&
                            DateTime.TryParse(apiString.Value, out _) &&
                            parameter.OpenApiParameter.Schema.Format != "date" &&
                            parameter.OpenApiParameter.Schema.Format != "date-time")
                        {
                            listResult.Add(new ResultItem(this) { Value = parameter.ResultLocation() });
                        }
                    }
                }
            }

            //Search for properties in request and response bodies with examples that value is likelihood date and the format is not date or date-time.
            var propertiesWithLikelyNumericExample =
                Contract.GetAllBodyProperties(RuleSettings, Cache)
                    .Where(property =>
                        property.OpenApiSchemaObject.Type.ToLower() == "string" &&
                        property.Example is OpenApiString apiString &&
                        DateTime.TryParse(apiString.Value, out _) &&
                        property.OpenApiSchemaObject.Format != "date" &&
                        property.OpenApiSchemaObject.Format != "date-time"
                    )
                    .ToList();

            foreach (var property in propertiesWithLikelyNumericExample)
            {
                if (Supressions.IsSupressed(ruleName, property.Path, property.Operation.ToLowerInvariant(),
                    property.FullName,
                    property.ResponseCode))
                    continue;

                listResult.Add(new ResultItem(this) { Value = property.ResultLocation() });
            }
        }
    }
}