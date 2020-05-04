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

                if (response.Name != "204" && (response.Name.StartsWith('2') || response.Name.StartsWith('3'))
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
