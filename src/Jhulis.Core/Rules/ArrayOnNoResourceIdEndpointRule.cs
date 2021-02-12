using System.Collections.Generic;
using Jhulis.Core;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Jhulis.Core.Helpers.Extensions;

namespace Jhulis.Core.Rules
{
    public class ArrayOnNoResourceIdEndpointRule : RuleBase
    {
        private const string ruleName = "ArrayOnNoResourceIdEndpoint";

        /// <summary>
        /// Validate if a likelihood plural collection path (without {id}) have an array as response.
        /// Supressions: Rule,Path
        /// </summary>
        public ArrayOnNoResourceIdEndpointRule(OpenApiDocument contract, Supressions supressions,
            IOptions<RuleSettings> ruleSettings, OpenApiDocumentCache cache) :
            base(contract, supressions, ruleSettings, cache, ruleName, Severity.Warning)
        {
            var example =
                ruleSettings.Value.ArrayOnNoResourceIdEndpoint.Example;
            Description = Details.Replace("{0}", example);
        }

        private protected override void ExecuteRuleLogic()
        {
            foreach (OpenApiDocumentExtensions.Response response in Contract.GetAllResponses(Cache))
            {
                if (Supressions.IsSupressed(ruleName, response.Path, response.Method, string.Empty, response.Name))
                    continue;

                string[] pathSplited =
                    response.Path.StartsWith('/')
                       ? response.Path.Substring(1).Split('/')
                       : response.Path.Split('/');

                var hasLastPathSegmentFinishingWithS = false;

                if (pathSplited.Length > 0)
                {
                    if (pathSplited[pathSplited.Length - 1].Contains("{") || string.IsNullOrEmpty(pathSplited[pathSplited.Length - 1])) continue;

                    foreach (string word in pathSplited[pathSplited.Length - 1].SplitCompositeWord())
                    {
                        if (word.EndsWith("s"))
                        {
                            hasLastPathSegmentFinishingWithS = true;
                            break;
                        }
                    }

                    if (hasLastPathSegmentFinishingWithS)
                    {
                        foreach (var content in response.OpenApiResponseObject?.Content)
                        {
                            if (content.Value?.Schema?.Properties?.ContainsKey("data") == true)
                            {
                                var dataProperty = content.Value.Schema.Properties["data"];
                                if (dataProperty.Type.ToLower() != "array")
                                    listResult.Add(new ResultItem(this, response.ResultLocation()));
                            }
                        }
                    }
                }
            }
        }
    }
}
