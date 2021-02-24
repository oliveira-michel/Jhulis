using System.Linq;
using Jhulis.Core;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Jhulis.Core.Helpers.Extensions;

namespace Jhulis.Core.Rules
{
    public class PropertyNamingMatchingPathRule : RuleBase
    {
        private const string ruleName = "PropertyNamingMatchingPath";

        /// <summary>
        /// Validate if there are properties whose names matches with their paths.
        /// Supressions: Rule,Path,Operation,ResponseCode,PropertyFull
        /// </summary>
        public PropertyNamingMatchingPathRule(OpenApiDocument contract,
            Supressions supressions, IOptions<RuleSettings> ruleSettings, OpenApiDocumentCache cache) : base(contract, supressions,
            ruleSettings, cache, ruleName, Severity.Information)
        {

        }

        private protected override void ExecuteRuleLogic()
        {
            foreach (OpenApiDocumentExtensions.Property property in Contract.GetAllBodyProperties(RuleSettings, Cache))
            {
                if (Supressions.IsSupressed(ruleName, property.Path, property.Operation, string.Empty,
                    property.ResponseCode, property.FullName)) continue;

                var segments = property.Path.Split('/');
                string lastPath = segments.Last(segment => !segment.Contains("{"));

                var wordsInPath = lastPath.SplitCompositeWord(); //separa as palavras compostas

                var alreadHitTheRule = false;

                foreach (string wordInPath in wordsInPath)
                {

                    if (alreadHitTheRule) continue;

                    string singularWordInPath = wordInPath.EndsWith("s") //Remove o s que representa plural
                                                    ? wordInPath.Remove(wordInPath.Length - 2, 1) 
                                                    : wordInPath;

                    var wordsInProperty = property.Name.SplitCompositeWord(); //separa as palavras

                    foreach (string wordInProperty in wordsInProperty)
                    {
                        if (wordInProperty.ToLowerInvariant() == singularWordInPath.ToLowerInvariant()) //Verifica se a palavra da propriedade bate com a palavra do path.
                        {
                            listResult.Add(
                                new ResultItem(this, property.ResultLocation()));
                            alreadHitTheRule = true;
                        }
                    }
                }
            }
        }
    }
}
