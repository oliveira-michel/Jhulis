using System.Text.RegularExpressions;
using Jhulis.Core;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Jhulis.Core.Helpers.Extensions;

namespace Jhulis.Core.Rules
{
    public class VersionFormatRule : RuleBase
    {
        private const string ruleName = "VersionFormat";
        private static string regexExpectedFormat;
        private static string humanReadeableFormat;
        private static string example;

        /// <summary>
        /// Validate the version format.
        /// Supressions: Rule
        /// </summary>
        public VersionFormatRule(OpenApiDocument contract, Supressions supressions,
            IOptions<RuleSettings> ruleSettings, OpenApiDocumentCache cache)
            : base(contract, supressions, ruleSettings, cache, ruleName, Severity.Error)
        {
            regexExpectedFormat = ruleSettings.Value.VersionFormat.RegexExpectedFormat;
            humanReadeableFormat = ruleSettings.Value.VersionFormat.HumanReadeableFormat;
            example = ruleSettings.Value.VersionFormat.Example;
        }

        private protected override void ExecuteRuleLogic()
        {
            if (!Regex.IsMatch(Contract.Info.Version, regexExpectedFormat))
                listResult.Add(
                    new ResultItem(this)
                    {
                        Value = $"info.version:{Contract.Info.Version}",
                        Description = Description
                            .Replace("{0}", humanReadeableFormat)
                            .Replace("{1}", example),
                        Details = Details.Replace("{0}", regexExpectedFormat)
                    }
                );
        }
    }
}
