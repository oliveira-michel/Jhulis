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
            //Search for parameters in query, path or header with examples that value is likelihood date and the format attribute is not date or date-time.
            foreach (KeyValuePair<string, OpenApiPathItem> path in Contract.Paths)
            {
                if (Supressions.IsSupressed(ruleName, path.Key))
                    continue;

                foreach (KeyValuePair<OperationType, OpenApiOperation> operation in path.Value.Operations)
                {
                    if (Supressions.IsSupressed(ruleName, path.Key, operation.Key.ToString().ToLowerInvariant()))
                        continue;

                    //Parameters in query, path or header
                    foreach (OpenApiParameter parameter in operation.Value.Parameters)
                    {
                        if (Supressions.IsSupressed(ruleName, path.Key, operation.Key.ToString().ToLowerInvariant(),
                            parameter.Name))
                            continue;

                        if (parameter.Example != null && parameter.Examples?.Count > 0 &&
                            parameter.Schema.Type.ToLower() == "string")
                        {
                            if (parameter.Example is OpenApiString exApiString &&
                                DateTime.TryParse(exApiString.Value, out _) &&
                                parameter.Schema.Format != "date" &&
                                parameter.Schema.Format != "date-time")
                            {
                                listResult.Add(
                                    new ResultItem(this)
                                    {
                                        Value =
                                            $"Path='{path.Key}',Operation='{operation.Key.ToString().ToLowerInvariant()}',Parameter='{parameter.Name}'"
                                    });
                            }

                            foreach (KeyValuePair<string, OpenApiExample> example in parameter.Examples)
                            {
                                if (parameter.Schema.Type.ToLower() == "string" &&
                                    example.Value.Value is OpenApiString apiString &&
                                    DateTime.TryParse(apiString.Value, out _) &&
                                    parameter.Schema.Format != "date" &&
                                    parameter.Schema.Format != "date-time")
                                {
                                    listResult.Add(
                                        new ResultItem(this)
                                        {
                                            Value =
                                                $"Path='{path.Key}',Operation='{operation.Key.ToString().ToLowerInvariant()}',Parameter='{parameter.Name}'"
                                        });
                                }
                            }
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

                listResult.Add(
                    new ResultItem(this)
                    {
                        Value =
                            $"Path='{property.Path}',Operation='{property.Operation.ToLowerInvariant()}',Parameter='{property.Name}'"
                    });
            }
        }
    }
}
