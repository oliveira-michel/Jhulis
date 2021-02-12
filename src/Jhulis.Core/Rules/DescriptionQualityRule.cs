using System;
using System.Collections.Generic;
using System.Linq;
using Jhulis.Core;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Jhulis.Core.Helpers.Extensions;

namespace Jhulis.Core.Rules
{
    public class DescriptionQualityRule : RuleBase
    {
        private const string ruleName = "DescriptionQuality";
        
        /// <summary>
        /// Validates if descriptions or summaries have some quality level, as begining with upper letter and finishing with period.
        /// Supressions: Rule,Path,Operation,parameter,ResponseCode,PropertyFull 
        /// </summary>
        public DescriptionQualityRule(OpenApiDocument contract,
            Supressions supressions, IOptions<RuleSettings> ruleSettings, OpenApiDocumentCache cache) :
            base(contract, supressions, ruleSettings, cache, ruleName, Severity.Information)
        {
        }

        private protected override void ExecuteRuleLogic()
        {
            if (!string.IsNullOrEmpty(Contract.Info.Description) &&
                !Contract.Info.Description.BeginsUpperAndFinishesWithPeriod())
                listResult.Add(
                    new ResultItem(this, $"info.description:{Contract.Info.Description}"));

            foreach (KeyValuePair<string, OpenApiPathItem> path in Contract.Paths.Where(path =>
                !Supressions.IsSupressed(ruleName, path.Key)))
            {
                if (
                    !string.IsNullOrEmpty(path.Value.Description) &&
                    !path.Value.Description.BeginsUpperAndFinishesWithPeriod()
                    ||
                    !string.IsNullOrEmpty(path.Value.Summary) &&
                     !path.Value.Summary.BeginsUpperAndFinishesWithPeriod()
                )
                    listResult.Add(
                        new ResultItem(this, path: path.Key));

                foreach (var parameter in Contract.GetAllParameters().Where(x=> x.Path == path.Key))
                {
                    if (Supressions.IsSupressed(ruleName, path.Key, parameter.Method)) continue;

                    if (Supressions.IsSupressed(ruleName, path.Key, parameter.Method, parameter.Name)) continue;

                    if (!string.IsNullOrEmpty(parameter.OpenApiParameter.Description) &&
                        !parameter.OpenApiParameter.Description.BeginsUpperAndFinishesWithPeriod())
                        listResult.Add(new ResultItem(this, parameter.ResultLocation()));
                }

                foreach (KeyValuePair<OperationType, OpenApiOperation> operation in path.Value.Operations)
                {
                    foreach (KeyValuePair<string, OpenApiResponse> response in operation.Value.Responses)
                    {
                        if (Supressions.IsSupressed(ruleName, path.Key,
                            Convert.ToString(operation.Key.ToString().ToLowerInvariant()), string.Empty,
                            response.Key)) continue;

                        //Usually, response description only have the response code and description without punctuation.
                        //Ex: 200 Ok
                        
                        foreach (KeyValuePair<string, OpenApiMediaType> content in response.Value.Content)
                            if (content.Value.Schema != null)
                            {
                                foreach (KeyValuePair<string, OpenApiSchema> firstLevelproperty in content.Value.Schema
                                        .Properties)
                                    //fist level: data, pagination, etc.
                                    //second level: resource properties first level.
                                    listResult.TryAddEmptiableRange(
                                        CheckInnerPropertyDescription(path.Key, operation.Key.ToString().ToLower(),
                                            response.Key, content.Key, firstLevelproperty.Key,
                                            firstLevelproperty.Value.Properties)
                                    );
                            }
                    }
                }
            }
        }

        //TODO Verificar possibilidade de usar o .GetAllProperties()
        private List<ResultItem> CheckInnerPropertyDescription(
            string path, string operation, string response, string content, string propertiesChain,
            IDictionary<string, OpenApiSchema> properties)
        {
            var resultItens = new List<ResultItem>();

            foreach (KeyValuePair<string, OpenApiSchema> property in properties)
            {
                if (Supressions.IsSupressed(ruleName, path, operation, string.Empty,
                    response, property.Key)) continue;

                if (!string.IsNullOrEmpty(property.Value.Description) &&
                    !property.Value.Description.BeginsUpperAndFinishesWithPeriod())
                {
                    string propertyName = string.IsNullOrEmpty(propertiesChain)
                        ? property.Key
                        : propertiesChain + "." + property.Key;

                    resultItens.Add(
                        new ResultItem(this, path: path, method: operation, response: response, content: content, responseProperty: propertyName));
                }

                if (property.Value.Properties.Count > 0)
                {
                    resultItens.TryAddEmptiableRange(
                        CheckInnerPropertyDescription(path, operation, response, content,
                            string.IsNullOrEmpty(propertiesChain)
                                ? propertiesChain + property.Key
                                : propertiesChain + "." + property.Key,
                            property.Value.Properties)
                    );
                }
            }

            return resultItens;
        }
    }
}
