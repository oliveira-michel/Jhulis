using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Jhulis.Core;

namespace Jhulis.Core.Helpers.Extensions
{
    public static class OpenApiDocumentExtensions
    {
        //RESPONSES
        public static IEnumerable<Response> GetAllResponses(this OpenApiDocument document, OpenApiDocumentCache cache)
        {
            if (cache.Responses != null && cache.Responses.Any()) return cache.Responses;

            var resultItens = new List<Response>();

            foreach (KeyValuePair<string, OpenApiPathItem> path in document.Paths)
            foreach (KeyValuePair<OperationType, OpenApiOperation> operation in path.Value.Operations)
            foreach (KeyValuePair<string, OpenApiResponse> response in operation.Value.Responses)
                resultItens.Add(
                    new Response
                    {
                        Path = path.Key,
                        Method = operation.Key.ToString().ToLowerInvariant(),
                        Name = response.Key,
                        OpenApiResponseObject = response.Value
                    });

            cache.Responses = resultItens;

            return resultItens;
        }

        public class Response
        {
            private string method;
            public string Path { get; set; }

            public string Method
            {
                get => method;
                set => method = value.ToLowerInvariant();
            }

            public string Name { get; set; }

            public OpenApiResponse OpenApiResponseObject { get; set; }

            public string ResultLocation()
            {
                return ResultItem.FormatValue(path: Path, method: Method, response: Name);
            }
        }

        //PROPERTIES
        public static IEnumerable<Property> GetAllBodyProperties(this OpenApiDocument document,
            IOptions<RuleSettings> ruleSettings, OpenApiDocumentCache cache)
        {
            if (cache.Properties != null && cache.Properties.Any()) return cache.Properties;

            var maxDepth = ruleSettings.Value.NestingDepth.Depth;

            var resultItens = new List<Property>();

            foreach (KeyValuePair<string, OpenApiPathItem> path in document.Paths)
            foreach (KeyValuePair<OperationType, OpenApiOperation> operation in path.Value.Operations)
            {
                //Response Bodies
                foreach (KeyValuePair<string, OpenApiResponse> response in operation.Value.Responses)
                foreach (KeyValuePair<string, OpenApiMediaType> content in response.Value.Content)
                    if (content.Value.Schema != null)
                    {
                        IDictionary<string, OpenApiSchema> propertyList = content.Value.Schema.Type == "array"
                            ? content.Value.Schema.Items.Properties
                            : content.Value.Schema.Properties;

                        var currentDepth = 0;

//                    foreach (KeyValuePair<string, OpenApiSchema> firstLevelproperty in content.Value.Schema.Properties)
                        //primeiro nível: data, pagination, etc.
                        //segundo nível: primeiro nível de propriedades da entidade, exceto quando o payload é de erro.
                        resultItens.TryAddEmptiableRange(
                            GetInnerProperties(path.Key, operation.Key.ToString(),
                                response.Key, content.Key, null,
                                //firstLevelproperty.Value.Properties)
                                propertyList, Property.ProcessingLevel.Response, maxDepth, currentDepth)
                        );
                    }

                //Request Bodies
                if (operation.Value?.RequestBody?.Content != null)
                    foreach (KeyValuePair<string, OpenApiMediaType> content in operation.Value?.RequestBody?.Content)
                        if (content.Value.Schema != null)
                        {
                            IDictionary<string, OpenApiSchema> propertyList = content.Value.Schema.Type == "array"
                                ? content.Value.Schema.Items.Properties
                                : content.Value.Schema.Properties;

                            var currentDepth = 0;

                            resultItens.TryAddEmptiableRange(
                                GetInnerProperties(path.Key, operation.Key.ToString(),
                                    /*body de entrada não tem response code*/null, content.Key, null,
                                    propertyList, Property.ProcessingLevel.Request, maxDepth, currentDepth)
                            );
                        }
            }

            cache.Properties = resultItens;

            return resultItens;
        }

        private static List<Property> GetInnerProperties(
            string path, string operation, string responseCode, string content, string propertiesChain,
            IDictionary<string, OpenApiSchema> properties, Property.ProcessingLevel processingLevel, int maxDepth, int currentDepth)
        {
            var resultItens = new List<Property>();

            foreach (KeyValuePair<string, OpenApiSchema> property in properties)
            {
                string propertyFullName = string.IsNullOrEmpty(propertiesChain)
                    ? property.Key
                    : propertiesChain + "." + property.Key;

                bool hittedMaxDepth = currentDepth >= maxDepth;

                resultItens.Add(
                    new Property
                    {
                        Path = path,
                        Operation = operation.ToLowerInvariant(),
                        ResponseCode = responseCode,
                        Content = content,
                        FullName = propertyFullName,
                        Name = property.Key,
                        Description = property.Value.Description,
                        OpenApiSchemaObject = property.Value,
                        Example = property.Value.Example,
                        HittedMaxDepth = hittedMaxDepth,
                        Depth = currentDepth,
                        Level = processingLevel
                    });

                if (!hittedMaxDepth)
                {
                    IDictionary<string, OpenApiSchema> propertyList = property.Value.Type == "array"
                        ? property.Value.Items.Properties
                        : property.Value.Properties;

                    if (propertyList.Count > 0)
                    {
                        currentDepth++;
                        resultItens.TryAddEmptiableRange(
                            GetInnerProperties(path, operation, responseCode, content,
                                string.IsNullOrEmpty(propertiesChain)
                                    ? propertiesChain + property.Key
                                    : propertiesChain + "." + property.Key,
                                propertyList, processingLevel, maxDepth, currentDepth)
                        );
                        currentDepth--;
                    }
                }
            }

            return resultItens;
        }

        public class Property
        {
            private string operation;

            public string Path { get; set; }

            public string Operation
            {
                get => operation;
                set => operation = value.ToLowerInvariant();
            }

            public string ResponseCode { get; set; }
            public string Content { get; set; }
            public string FullName { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public IOpenApiAny Example { get; set; }
            public ProcessingLevel Level { get; set; }

            public OpenApiSchema OpenApiSchemaObject { get; set; }

            public bool HittedMaxDepth { get; set; }
            public int Depth { get; set; }

            //TODO Colocar esse ToString em todos os lugares que usam a Property
            public String ResultLocation()
            {
                return Level == ProcessingLevel.Request
                    ? ResultItem.FormatValue(path: Path, method: Operation, requestProperty: FullName)
                    : ResultItem.FormatValue(path: Path, method: Operation, response: ResponseCode, content: Content, responseProperty: FullName);
            }

            public enum ProcessingLevel
            {
                Request,
                Response
            }
        }

        //PARAMETERS
        public static List<Parameter> GetAllParameters(this OpenApiDocument document)
        {
            var resultItens = new List<Parameter>();

            foreach (KeyValuePair<string, OpenApiPathItem> path in document.Paths)
            {
                foreach (OpenApiParameter parameter in path.Value.Parameters)
                {
                    resultItens.Add(
                        new Parameter
                        {
                            Path = path.Key,
                            Name = parameter.Name,
                            OpenApiParameter = parameter,
                            ParentPathSegment = ExtractParentPahSegment(path, parameter),
                            Level = Parameter.ProcessingLevel.Request
                        });
                }

                foreach (KeyValuePair<OperationType, OpenApiOperation> operation in path.Value.Operations)
                    foreach (OpenApiParameter parameter in operation.Value.Parameters)
                    {
                        resultItens.Add(
                            new Parameter
                            {
                                Path = path.Key,
                                Method = Convert.ToString(operation.Key).ToLowerInvariant(),
                                Name = parameter.Name,
                                OpenApiParameter = parameter,
                                ParentPathSegment = ExtractParentPahSegment(path, parameter),
                                Level = Parameter.ProcessingLevel.Response
                            });
                    }
            }

            return resultItens;
        }

        private static string ExtractParentPahSegment(KeyValuePair<string, OpenApiPathItem> path, OpenApiParameter parameter)
        {
            string[] pathSegments = path.Key.Split('/');
            string parentPathSegment = string.Empty;
            for (int i = pathSegments.Length - 1; i >= 0; i--)
                if (pathSegments[i] == parameter.Name)
                    parentPathSegment = pathSegments[i - 1];
            return parentPathSegment;
        }

        public class Parameter
        {
            private string method;

            public string Path { get; set; }

            public string Method
            {
                get => method;
                set => method = value.ToLowerInvariant();
            }

            public string Name { get; set; }
            public string ParentPathSegment { get; set; }
            public OpenApiParameter OpenApiParameter { get; set; }
            public ProcessingLevel Level { get; set; }

            public enum ProcessingLevel
            {
                Request,
                Response
            }

            public String ResultLocation()
            {
                switch (OpenApiParameter.In)
                {
                    case ParameterLocation.Query:
                            return ResultItem.FormatValue(path: Path, method: method, queryParameter: Name);
                        break;
                    case ParameterLocation.Path:
                            return ResultItem.FormatValue(path: Path, pathParameter: Name);
                        break;
                    case ParameterLocation.Header:
                        switch (Level)
                        {
                            case ProcessingLevel.Request:
                                return ResultItem.FormatValue(path: Path, method: method, requestHeader: Name);
                                break;
                            case ProcessingLevel.Response:
                                return ResultItem.FormatValue(path: Path, method: method, responseHeader: Name);
                                break;
                        }
                        break;
                    case ParameterLocation.Cookie:
                            return ResultItem.FormatValue(path: Path, method: method, cookie: Name);
                        break;
                }

                return "ERROR while getting the parameter origin.";
            }
        }
    }
}
