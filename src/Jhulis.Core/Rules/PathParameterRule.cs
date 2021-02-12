using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using Jhulis.Core;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Jhulis.Core.Helpers.Extensions;

namespace Jhulis.Core.Rules
{
    public class PathParameterRule : RuleBase
    {
        private const string ruleName = "PathParameter";

        private static double matchEntityNamePercentage;
        private static string regex;
        private static string prefixToRemove;
        private static string sufixToRemove;
        private static string humanReadeableFormat;
        private static CaseType caseType;
        private static string example;

        /// <summary>
        /// Validate the path parameters case type.
        /// Supressions: Rule,Path,Operation,Parameter
        /// </summary>
        public PathParameterRule(OpenApiDocument contract, Supressions supressions,
            IOptions<RuleSettings> ruleSettings, OpenApiDocumentCache cache)
            : base(contract, supressions, ruleSettings, cache, ruleName, Severity.Warning)
        {
            matchEntityNamePercentage = ruleSettings.Value.PathParameter.MatchEntityNamePercentage;
            regex = ruleSettings.Value.PathParameter.Regex;
            prefixToRemove = ruleSettings.Value.PathParameter.PrefixToRemove;
            sufixToRemove = ruleSettings.Value.PathParameter.SufixToRemove;
            humanReadeableFormat = ruleSettings.Value.PathParameter.HumanReadeableFormat;
            example = ruleSettings.Value.PathParameter.Example;
            
            if (Enum.TryParse(ruleSettings.Value.PathParameter.CaseType, out CaseType caseTypeConversionTryOut))
                caseType = caseTypeConversionTryOut;
            else
                throw new InvalidEnumArgumentException(
                    $"{ruleName}.CaseType={ruleSettings.Value.PathParameter.CaseType}");
        }

        private protected override void ExecuteRuleLogic()
        {
            foreach (OpenApiDocumentExtensions.Parameter parameter in Contract.GetAllParameters())
            {
                if (Supressions.IsSupressed(ruleName, parameter.Path, parameter.Method, parameter.Name)) continue;

                if (parameter.OpenApiParameter.In != ParameterLocation.Path) continue;

                string cleanedParameter = parameter.Name;

                if (!string.IsNullOrEmpty(prefixToRemove))
                    cleanedParameter = cleanedParameter
                        .Remove(0, prefixToRemove.Length);

                if (!string.IsNullOrEmpty(sufixToRemove))
                    cleanedParameter = cleanedParameter
                        .Remove(parameter.Name.LastIndexOf(sufixToRemove, StringComparison.Ordinal),
                            sufixToRemove.Length);

                cleanedParameter = cleanedParameter.ToLower();

                if (
                    !Regex.IsMatch(parameter.Name, regex)
                    || cleanedParameter.CalculateSimilarityWith(parameter.ParentPathSegment) <
                    matchEntityNamePercentage)
                    listResult.Add(
                        new ResultItem(this)
                        {
                            Description = Description
                                .Replace("{0}", humanReadeableFormat)
                                .Replace("{1}", caseType.GetEnumDescription())
                                .Replace("{2}", example),
                            Value =
                                $"Path='{parameter.Path}',Operation='{parameter.Method}',Parameter='{parameter.Name}'"
                        });
            }
        }
    }
}
