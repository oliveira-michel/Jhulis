using System;
using System.Collections.Generic;
using System.Linq;
using Jhulis.Core;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Jhulis.Core.Helpers.Extensions;
using System.Text.RegularExpressions;
using Jhulis.Core.Resources;

namespace Jhulis.Core.Rules
{
    public class StringCouldBeNumberRule : RuleBase
    {
        private const string ruleName = "StringCouldBeNumber";
        private readonly string exceptionsRegex = string.Empty;
        /// <summary>
        /// Validates if parameters or attributes have examples indicating that the content is likely numeric type.
        /// Supressions: Rule,Path,Operation,ResponseCode,Content,PropertyFull
        /// </summary>
        public StringCouldBeNumberRule(OpenApiDocument contract,
            Supressions supressions,
            IOptions<RuleSettings> ruleSettings, OpenApiDocumentCache cache)
            : base(contract, supressions, ruleSettings, cache, ruleName, Severity.Hint)
        {
            exceptionsRegex = ruleSettings.Value.StringCouldBeNumber.ExceptionsRegex;
        }

        private protected override void ExecuteRuleLogic()
        {
            //Examples in parameters in query, path or header (operation.Value.Parameters[0].Example e Examples) are always empty
            //OAS do not define example for these items
            
            //Search for properties in request and response bodies with examples that value is likelihood number
            var propertiesWithLikelyNumericExample =
                Contract.GetAllBodyProperties(RuleSettings, Cache)
                    .Where(property =>
                        property.OpenApiSchemaObject?.Type?.ToLower() == "string" &&
                        property.Example is OpenApiString apiString &&
                        !Regex.IsMatch(apiString.Value, RegexLibrary.IpV4) &&//It is not an IP v4
                        !Regex.IsMatch(property.Name, exceptionsRegex) &&
                        IsLikelyNumberType(apiString.Value))
                    .ToList();

            foreach (var property in propertiesWithLikelyNumericExample)
            {
                if (Supressions.IsSupressed(ruleName, property.Path, property.Operation.ToLowerInvariant(),
                    property.FullName,
                    property.ResponseCode))
                    continue;

                listResult.Add(
                    new ResultItem(this, property.ResultLocation()));
            }
        }

        /// <summary>
        /// Check if example shows that value is likelihood a number or currency.
        /// </summary>
        /// <param name="value">example given</param>
        /// <returns></returns>
        private bool IsLikelyNumberType(string value)
        {
            //remove spaces
            value = value.Replace(" ", "");

            //remove concurrency simbol
            string biggestMatchCurrency = "";
            foreach (string currency in RuleSettings.Value.StringCouldBeNumber.CurrencySymbols.Split(','))
            {
                if (value.StartsWith(currency) && currency.Length > biggestMatchCurrency.Length)
                {
                    biggestMatchCurrency = currency;
                }
            }

            if (!string.IsNullOrEmpty(biggestMatchCurrency))
                value = value.Replace(biggestMatchCurrency, "");

            //remove comma, period and hifen
            value = value.Replace(",", "");
            value = value.Replace(".", "");
            value = value.Replace("-", "");

            //bool if remainder is a number 
            return Double.TryParse(value, out _);
        }
    }
}
