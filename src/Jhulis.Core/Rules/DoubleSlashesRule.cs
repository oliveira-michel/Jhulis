using System.Collections.Generic;
using Jhulis.Core;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Jhulis.Core.Helpers.Extensions;

namespace Jhulis.Core.Rules
{
    public class DoubleSlashesRule : RuleBase
    {
        private const string ruleName = "DoubleSlashes";

        /// <summary>
        /// Validates paths finishes with slashes that can result in bad service functioning. 
        /// Supressions: Rule,Path
        /// </summary>
        public DoubleSlashesRule(OpenApiDocument contract, Supressions supressions, IOptions<RuleSettings> ruleSettings, OpenApiDocumentCache cache)
            : base(contract, supressions, ruleSettings, cache, ruleName, Severity.Error)
        {
        }

        private protected override void ExecuteRuleLogic()
        {
            foreach (OpenApiServer server in Contract.Servers)
            {
                if (server.Url.Split("//").Length > 2)
                    listResult.Add(new ResultItem(this)
                    {
                        Value = $"server.url:{server.Url}"
                    });
            }

            foreach (KeyValuePair<string, OpenApiPathItem> path in Contract.Paths)
            {
                if (Supressions.IsSupressed(ruleName, path.Key)) continue;
                
                if (path.Key.Contains("//"))
                    listResult.Add(new ResultItem(this, path:path.Key));
            }
        }
    }
}
