using Jhulis.Core;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Jhulis.Core.Helpers.Extensions;

namespace Jhulis.Core.Rules
{
    public class InfoContactRule : RuleBase
    {
        private const string ruleName = "InfoContact";
        //No supressions

        /// <summary>
        /// Validate if the contact information is present.
        /// Supressions: Rule
        /// </summary>
        public InfoContactRule(OpenApiDocument contract, Supressions supressions,
            IOptions<RuleSettings> ruleSettings, OpenApiDocumentCache cache) :
            base(contract, supressions, ruleSettings, cache, ruleName, Severity.Information)
        {
        }

        private protected override void ExecuteRuleLogic()
        {
            if (string.IsNullOrEmpty(Contract?.Info?.Contact?.Name)
                && string.IsNullOrEmpty(Contract?.Info?.Contact?.Email))
                listResult.Add(new ResultItem(this)
                {
                    Value =
                        $"Info.Contact.Name='{Contract?.Info?.Contact?.Name}',Info.Contact.Email='{Contract?.Info?.Contact?.Email}'"
                });
        }
    }
}
