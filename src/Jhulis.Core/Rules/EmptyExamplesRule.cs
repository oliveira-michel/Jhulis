using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Jhulis.Core;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Jhulis.Core.Helpers.Extensions;

namespace Jhulis.Core.Rules
{
    public class EmptyExamplesRule : RuleBase
    {
        private const string ruleName = "EmptyExamples";

        /// <summary>
        /// Validates if there are examples in response and request bodies.
        /// Supressions: Rule,Path,Operation,ResponseCode 
        /// </summary>
        public EmptyExamplesRule(OpenApiDocument contract, Supressions supressions,
            IOptions<RuleSettings> ruleSettings, OpenApiDocumentCache cache)
            : base(contract, supressions, ruleSettings, cache, ruleName, Severity.Information)
        {
        }

        //Path|Operation|Response|Content 
        private HashSet<string> schemasWithoutExample = new HashSet<string>();
        private HashSet<string> schemasWithExample = new HashSet<string>();

        private protected override void ExecuteRuleLogic()
        {
            //Examples in parameters in query, path or header (operation.Value.Parameters[0].Example e Examples) are always empty
            //OAS do not define example for these items
            
            foreach (KeyValuePair<string, OpenApiPathItem> path in Contract.Paths)
            {
                foreach (KeyValuePair<OperationType, OpenApiOperation> operation in path.Value.Operations)
                {
                    //Search for schemas with empty examples in responses
                    foreach (KeyValuePair<string, OpenApiResponse> response in operation.Value.Responses)
                    {
                        if (Supressions.IsSupressed(ruleName, path.Key, operation.Key.ToString().ToLowerInvariant(),
                            string.Empty,
                            response.Key))
                            continue;

                        //Here, we want at least one example at response type level
                        bool foundExample = response.Value.Content.Any(x =>
                                x.Value?.Schema?.Example != null
                            || (x.Value?.Example != null)
                            || (x.Value?.Examples != null && x.Value?.Examples.Count > 0));

                        if (!foundExample)
                            foreach (KeyValuePair<string, OpenApiMediaType> content in response.Value.Content)
                                schemasWithoutExample.Add(
                                    $"{path.Key}|{operation.Key.ToString().ToLowerInvariant()}|{response.Key}|{content.Key}");
                        else
                            foreach (KeyValuePair<string, OpenApiMediaType> content in response.Value.Content)
                                schemasWithExample.Add(
                                    $"{path.Key}|{operation.Key.ToString().ToLowerInvariant()}|{response.Key}|{content.Key}");
                    }

                    //Search for schemas with empty examples in request
                    if (operation.Value?.RequestBody?.Content != null)
                        foreach (KeyValuePair<string, OpenApiMediaType> content in operation.Value?.RequestBody?.Content)
                            if (content.Value.Schema != null)
                            {
                                //TODO, colocar a mesma verificação acima.
                                //It is an object with schema
                                if (content.Value.Schema != null)
                                {
                                    if (content.Value.Schema.Example == null)
                                        schemasWithoutExample.Add(
                                            $"{path.Key}|{operation.Key.ToString().ToLowerInvariant()}||{content.Key}");
                                }
                                else //May be a content without schema
                                {
                                    if (content.Value != null && content.Value.Example == null &&
                                        content.Value.Examples.Count == 0)
                                        schemasWithoutExample.Add(
                                            $"{path.Key}|{operation.Key.ToString().ToLowerInvariant()}||{content.Key}");
                                }
                            }
                }
            }

            var propertyLevelWithExample = new Stack<int>();
            var currentBody = "";
            //Search for properties in request and response
            foreach (var property in Contract.GetAllBodyProperties(RuleSettings, Cache))
            {
                if (Supressions.IsSupressed(ruleName, property.Path, property.Operation.ToLowerInvariant(),
                    property.FullName,
                    property.ResponseCode))
                    continue;

                //Clear stack if changes the path, operation, response or content.
                if (currentBody !=
                    $"{property.Path}|{property.Operation}|{property.ResponseCode}|{property.Content}")
                {
                    currentBody = $"{property.Path}|{property.Operation}|{property.ResponseCode}|{property.Content}";
                    propertyLevelWithExample.Clear();
                }

                //This property is into an schema with example at schema level.
                if (schemasWithExample.Contains(currentBody))
                    continue;

                //Define current depth as last one with example
                //Clear item from schemasWithoutExample list 
                if (property.Example != null)
                {
                    propertyLevelWithExample.Push(property.Depth);
                    schemasWithoutExample.Remove(
                        $"{property.Path}|{property.Operation}|{property.ResponseCode}|{property.Content}");
                    continue;
                }

                //The property is under another one with example
                if (propertyLevelWithExample.Count > 0 && property.Depth > propertyLevelWithExample.Peek())
                    continue;

                //Clear stack until current depth
                while (propertyLevelWithExample.Count > 0 && property.Depth < propertyLevelWithExample.Peek())
                    propertyLevelWithExample.Pop();

                var responseKeyValue = property.InsideOf == OpenApiDocumentExtensions.Property.BodyType.Response
                    ? $"Response='{property.ResponseCode}',"
                    : "";

                listResult.Add(
                    new ResultItem(this)
                    {
                        Value =
                            $"Path='{property.Path}',Operation='{property.Operation.ToLowerInvariant()}',{responseKeyValue}Parameter='{property.FullName}'"
                    });
            }

            foreach (string s in schemasWithoutExample)
            {
                var schemaParts = s.Split('|');

                var responseKeyValue = !string.IsNullOrEmpty(schemaParts[2].ToString())
                    ? $"Response='{schemaParts[2].ToString()}',"
                    : "";

                listResult.Add(
                    new ResultItem(this)
                    {
                        Value =
                            $"Path='{schemaParts[0].ToString()}',Operation='{schemaParts[1].ToString()}',{responseKeyValue}Parameter='{schemaParts[3].ToString()}'"
                    });
            }
        }
    }
}
