using System.Linq;
using Jhulis.Core;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Jhulis.Core.Helpers.Extensions;
using System;
using System.ComponentModel;

namespace Jhulis.Core.Rules
{
    public class PrepositionsRule : RuleBase
    {
        private const string ruleName = "Prepositions";
        private static string[] wordsToAvoid;

        //TODO criar validação de case para Parameters (query e path) > Regra nova

        /// <summary>
        /// Validate if there are paths, parameters or properties with prepositions.
        /// Supressions: Rule,Path,Operation,Parameter,ResponseCode,PropertyFull
        /// </summary>
        public PrepositionsRule(OpenApiDocument contract,
            Supressions supressions, IOptions<RuleSettings> ruleSettings, OpenApiDocumentCache cache) : base(contract, supressions,
            ruleSettings, cache, ruleName, Severity.Information)
        {
            wordsToAvoid = ruleSettings.Value.Prepositions.WordsToAvoid.Split(',');
        }

        private protected override void ExecuteRuleLogic()
        {
            foreach (string path in Contract.Paths.Keys)
            {
                if (Supressions.IsSupressed(ruleName, path)) continue;

                string[] pathSplited =
                   path.StartsWith('/')
                      ? path.Substring(1).Split('/')
                      : path.Split('/');

                foreach (var pathSegment in pathSplited)
                {
                    if (pathSegment.Length > 0)
                    {
                        if (pathSegment.StartsWith('{')) continue; //Parameters came on other part in this rule.

                        if (
                        pathSegment.SplitCompositeWord()
                        .Select(w => w.ToLower()).ToList()
                        .Any(w => wordsToAvoid.Contains(w)))
                        {
                            listResult.Add(
                                new ResultItem(this, path:path));
                        }
                    }
                }
            }

            foreach (OpenApiDocumentExtensions.Parameter parameter in Contract.GetAllParameters())
            {
                if (Supressions.IsSupressed(ruleName, parameter.Path, parameter.Method, parameter.Name)) continue;

                if (
                    parameter.Name.SplitCompositeWord()
                    .Select(w => w.ToLower()).ToList()
                    .Any(w => wordsToAvoid.Contains(w)))
                {
                    listResult.Add(
                        new ResultItem(this, parameter.ResultLocation()));
                }
            }

            foreach (OpenApiDocumentExtensions.Property property in Contract.GetAllBodyProperties(RuleSettings, Cache))
            {
                if (Supressions.IsSupressed(ruleName, property.Path, property.Operation, string.Empty,
                    property.ResponseCode, property.FullName)) continue;

                if (
                    property.Name.SplitCompositeWord()
                    .Select(w => w.ToLower()).ToList()
                    .Any(w => wordsToAvoid.Contains(w)))
                {
                    listResult.Add(
                        new ResultItem(this, property.ResultLocation()));
                }
            }
        }
    }
}
