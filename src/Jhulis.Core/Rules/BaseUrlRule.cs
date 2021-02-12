using System;
using Jhulis.Core;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Jhulis.Core.Helpers.Extensions;

namespace Jhulis.Core.Rules
{
    public class BaseUrlRule : RuleBase
    {
        private const string ruleName = "BaseUrl";

        /// <summary>
        /// Try to test if the base URL parts togheter compose a valid URL.
        /// Supressions: Rule
        /// </summary>
        public BaseUrlRule(OpenApiDocument contract, Supressions supressions,
            IOptions<RuleSettings> ruleSettings, OpenApiDocumentCache cache) :
            base(contract, supressions, ruleSettings, cache, ruleName, Severity.Error)
        {
        }

        private protected override void ExecuteRuleLogic()
        {
            foreach (OpenApiServer server in Contract.Servers)
            {
                bool validUri = Uri.TryCreate(server.Url, UriKind.Absolute, out Uri uriResult)
                                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                if (!validUri)
                    listResult.Add(new ResultItem(this, $"servers.url:{uriResult}"));
            }

            if (Contract.Servers.Count == 0)
                listResult.Add(new ResultItem(this));
        }
    }
}
