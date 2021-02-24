using System;
using System.Collections.Generic;
using System.ComponentModel;
using Jhulis.Core;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Jhulis.Core.Helpers.Extensions;

namespace Jhulis.Core.Rules
{
    public class PathCaseRule : RuleBase
    {
        private const string ruleName = "PathCase";
        private readonly CaseType caseType;
        private readonly bool caseTypeTolerateNumber;
        private readonly string example;

        /// <summary>
        /// Validate the paths case type.
        /// Supressions: Rule,Path
        /// </summary>
        public PathCaseRule(OpenApiDocument contract, Supressions supressions,
            IOptions<RuleSettings> ruleSettings, OpenApiDocumentCache cache) :
            base(contract, supressions, ruleSettings, cache, ruleName, Severity.Warning)
        {
            if (Enum.TryParse(ruleSettings.Value.PathCase.CaseType, out CaseType caseTypeConversionTryOut))
                caseType = caseTypeConversionTryOut;
            else
                throw new InvalidEnumArgumentException(
                    $"{ruleName}.CaseType:{ruleSettings.Value.PathCase.CaseType}");

            caseTypeTolerateNumber = ruleSettings.Value.PathCase.CaseTypeTolerateNumber;
            example = ruleSettings.Value.PathCase.Example;
        }

        private protected override void ExecuteRuleLogic()
        {
            foreach (KeyValuePair<string, OpenApiPathItem> path in Contract.Paths)
            {
                if (Supressions.IsSupressed(ruleName, path.Key)) continue;

                string[] pathSplited = path.Key.Split('/');

                foreach (string pathSegment in pathSplited)
                    if (!string.IsNullOrEmpty(pathSegment)
                        && !pathSegment.Contains("{")
                        && !pathSegment.GetCaseType(caseTypeTolerateNumber).Contains(caseType))
                        listResult.Add(
                            new ResultItem(this)
                            {
                                Description = Description
                                    .Replace("{0}", caseType.GetEnumDescription())
                                    .Replace("{1}", example),
                                Value = ResultItem.FormatValue(path: path.Key, pathSegment: pathSegment)
                            });
            }
        }
    }
}
