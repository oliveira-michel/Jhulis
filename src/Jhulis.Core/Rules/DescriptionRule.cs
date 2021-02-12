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
    public class DescriptionRule : RuleBase
    {
        private const string ruleName = "Description";
        private readonly int minDescriptionLength;
        private readonly int midDescriptionLength;
        private readonly int largeDescriptionLength;
        private readonly bool testDescriptionInPaths;
        private readonly bool testDescriptionInOperation;

        /// <summary>
        /// Validates if the descriptions are present or have a minimum size lenght.
        /// Supressions: Rule,Path,Operation,ResponseCode,PropertyFull
        /// </summary>
        public DescriptionRule(OpenApiDocument contract, Supressions supressions, IOptions<RuleSettings> ruleSettings, OpenApiDocumentCache cache) :
            base(contract, supressions, ruleSettings, cache, ruleName, Severity.Information)
        {
            minDescriptionLength = ruleSettings.Value.Description.MinDescriptionLength;
            midDescriptionLength = ruleSettings.Value.Description.MidDescriptionLength;
            largeDescriptionLength = ruleSettings.Value.Description.LargeDescriptionLength;
            testDescriptionInPaths = ruleSettings.Value.Description.TestDescriptionInPaths;
            testDescriptionInOperation = ruleSettings.Value.Description.TestDescriptionInOperation;
        }

        //TODO falta percorrer também o body de entrada. Checar em todos os lugares que eu trato response, pois provavelmente deve estar esquecendo o body de entrada também.
        //A checagem nos bodies de entrada podem seguir o modelo do empty examples, pois lá já trata o body de entrada.
        //Usar o GetAllBodiesProperties e simplificar este código aqui.
        
        private protected override void ExecuteRuleLogic()
        {
            if (string.IsNullOrEmpty(Contract.Info.Description) ||
                Contract.Info.Description?.Length < largeDescriptionLength)
                listResult.Add(
                    new ResultItem(this) {Value = $"info.description:{Contract.Info.Description}"});

            foreach (KeyValuePair<string, OpenApiPathItem> path in Contract.Paths.Where(path => !Supressions.IsSupressed(ruleName, path.Key)))
            {
                if (testDescriptionInPaths)
                {
                    if (
                        string.IsNullOrEmpty(path.Value.Description) && string.IsNullOrEmpty(path.Value.Summary)
                        ||
                        path.Value.Description?.Length < midDescriptionLength &&
                        path.Value.Summary?.Length < midDescriptionLength
                    )
                        listResult.Add(new ResultItem(this, path:path.Key));
                }

                foreach (KeyValuePair<OperationType, OpenApiOperation> operation in path.Value.Operations)
                {
                    if (Supressions.IsSupressed(ruleName, path.Key,
                        Convert.ToString(operation.Key.ToString().ToLower()))) continue;

                    if (testDescriptionInOperation)
                    {
                        if (
                            string.IsNullOrEmpty(operation.Value.Description) &&
                            string.IsNullOrEmpty(operation.Value.Summary)
                            ||
                            operation.Value.Description?.Length < midDescriptionLength &&
                            operation.Value.Summary?.Length < midDescriptionLength)
                            listResult.Add(new ResultItem(this, path:path.Key, method: operation.Key.ToString().ToLowerInvariant()));
                    }

                    foreach (var parameter in Contract.GetAllParameters().Where(x=> x.Path == path.Key && x.Method == operation.Key.ToString().ToLowerInvariant()))
                    {
                        if (Supressions.IsSupressed(ruleName, path.Key,
                           Convert.ToString(operation.Key.ToString().ToLowerInvariant()), parameter.Name)) continue;

                        if (string.IsNullOrEmpty(parameter.OpenApiParameter.Description) ||
                           parameter.OpenApiParameter.Description?.Length < midDescriptionLength)
                            listResult.Add(
                                new ResultItem(this, parameter.ResultLocation()));
                    }

                    foreach (KeyValuePair<string, OpenApiResponse> response in operation.Value.Responses)
                    {
                        if (Supressions.IsSupressed(ruleName, path.Key,
                            Convert.ToString(operation.Key.ToString().ToLowerInvariant()), string.Empty,
                            response.Key)) continue;
                        
                        if (string.IsNullOrEmpty(response.Value.Description) ||
                            response.Value.Description?.Length < midDescriptionLength)
                        {
                            listResult.Add(
                                new ResultItem(this, path:path.Key, method: operation.Key.ToString().ToLowerInvariant(), response: response.Key));
                        }
                        
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

                if (string.IsNullOrEmpty(property.Value.Description) ||
                    property.Value.Description?.Length < minDescriptionLength)
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
