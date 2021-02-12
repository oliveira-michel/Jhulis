using System;
using System.ComponentModel;
using System.Linq;
using Jhulis.Core;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Jhulis.Core.Helpers.Extensions;

namespace Jhulis.Core.Rules
{
    public class PropertyCaseRule : RuleBase
    {
        private const string ruleName = "PropertyCase";
        private static CaseType caseType;
        private static string example;
        private readonly bool caseTypeTolerateNumber; 

        /// <summary>
        /// Validate the properties case type.
        /// Supressions: Rule,Path,Operation,ResponseCode,PropertyFull
        /// </summary>
        public PropertyCaseRule(OpenApiDocument contract, Supressions supressions,
            IOptions<RuleSettings> ruleSettings, OpenApiDocumentCache cache)
            : base(contract, supressions, ruleSettings, cache, ruleName, Severity.Warning)
        {
            if (Enum.TryParse(ruleSettings.Value.PropertyCase.CaseType, out CaseType caseTypeConversionTryOut))
                caseType = caseTypeConversionTryOut;
            else
                throw new InvalidEnumArgumentException(
                    $"{ruleName}.CaseType={ruleSettings.Value.PropertyCase.CaseType}");

            example = ruleSettings.Value.PropertyCase.Example;
            caseTypeTolerateNumber = ruleSettings.Value.PropertyCase.CaseTypeTolerateNumber;
        }

        private protected override void ExecuteRuleLogic()
        {
            foreach (OpenApiDocumentExtensions.Property property in Contract.GetAllBodyProperties(RuleSettings, Cache))
            {
                if (Supressions.IsSupressed(ruleName, property.Path, property.Operation, string.Empty,
                    property.ResponseCode, property.FullName)) continue;

                if (property.Name.GetCaseType(caseTypeTolerateNumber).First() != caseType)
                    listResult.Add(
                        new ResultItem(this)
                        {
                            Description = Description
                                .Replace("{0}", caseType.GetEnumDescription())
                                .Replace("{1}", example),
                            Value = property.ResultLocation()
                        });
            }
        }
    }
}
