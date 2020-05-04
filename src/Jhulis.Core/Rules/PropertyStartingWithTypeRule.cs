using System.Linq;
using Jhulis.Core;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Jhulis.Core.Helpers.Extensions;

namespace Jhulis.Core.Rules
{
    public class PropertyStartingWithTypeRule : RuleBase
    {
        private const string ruleName = "PropertyStartingWithType";
        private static string[] wordsToAvoid;
        //Supressions 

        /// <summary>
        /// Validate if there are properties starting with language types.
        /// Supressions: Rule,Path,Operation,ResponseCode,PropertyFull
        /// </summary>
        public PropertyStartingWithTypeRule(OpenApiDocument contract,
            Supressions supressions, IOptions<RuleSettings> ruleSettings, OpenApiDocumentCache cache) : base(contract, supressions,
            ruleSettings, cache, ruleName, Severity.Information)
        {
            wordsToAvoid = ruleSettings.Value.PropertyStartingWithType.WordsToAvoid.Split(',');
        }

        private protected override void ExecuteRuleLogic()
        {
            foreach (OpenApiDocumentExtensions.Property property in Contract.GetAllBodyProperties(RuleSettings, Cache))
            {
                if (Supressions.IsSupressed(ruleName, property.Path, property.Operation, string.Empty,
                    property.ResponseCode, property.FullName)) continue;

                if (wordsToAvoid.Any(w => property.Name.StartsWith(w)))
                {
                    listResult.Add(
                        new ResultItem(this)
                        {
                            Value =
                                $"Path='{property.Path}',Operation='{property.Operation}',ResponseCode='{property.ResponseCode}',Content='{property.Content}',PropertyFull='{property.FullName}',Property='{property.Name}'"
                        });
                }
            }
        }
    }
}
