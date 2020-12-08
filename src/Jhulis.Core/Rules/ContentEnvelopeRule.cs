using System.Linq;
using Jhulis.Core;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Jhulis.Core.Helpers.Extensions;

namespace Jhulis.Core.Rules
{
    public class ContentEnvelopeRule : RuleBase
    {
        private const string ruleName = "ContentEnvelope";
        private readonly string contentEnvelopeName;

        /// <summary>
        /// Validates if successful response codes (2xx and 3xx) contains valid envelope to separate the resource content from metadatas as pagination or messages.
        /// The 204 used for empty responses is escaped in this rule.
        /// Supressions: Rule,Path,Operation,ResponseCode
        /// </summary>
        public ContentEnvelopeRule(OpenApiDocument contract,
            Supressions supressions, IOptions<RuleSettings> ruleSettings, OpenApiDocumentCache cache) : base(contract, supressions,
            ruleSettings, cache, ruleName, Severity.Error)
        {
            contentEnvelopeName =
                ruleSettings.Value.ContentEnvelope.EnvelopeName;
            Description = Description.Replace("{0}", contentEnvelopeName);
        }

        private protected override void ExecuteRuleLogic()
        {
            foreach (OpenApiDocumentExtensions.Response response in Contract.GetAllResponses(Cache))
            {
                if (Supressions.IsSupressed(ruleName, response.Path, response.Operation, string.Empty, response.Name))
                    continue;

                //GET e POST são mais prováveis de ter payload.
                //DELETE, PUT e PATCH são fortes candidatos a não terem payload
                //204, 201 e 3xx são fortes candidatos a não terem payload
                //200 e 206 são fortes candidatos a terem payload
                if (
                       (response.Operation == "get" || response.Operation == "post")
                    && (response.Name == "200" || response.Name == "206")
                    && MissingEnvelopeProperty(response.OpenApiResponseObject))
                    listResult.Add(
                        new ResultItem(this)
                            {Value = response.ToString()});
            }
        }

        private bool MissingEnvelopeProperty(OpenApiResponse response)
        {
            return response.Content.Count(
                       content =>
                           content.Value.Schema != null &&
                           content.Value.Schema.Properties.ContainsKey(contentEnvelopeName)
                   ) == 0;
        }
    }
}
