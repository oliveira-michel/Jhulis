using System.Collections.Generic;
using System.Linq;
using Jhulis.Core.Rules;

namespace Jhulis.Core
{
    public class Result
    {
        public Status Status =>
            ResultItens.Count(item => item.Severity == Severity.Error) > 0
                ? Status.Error
                : ResultItens.Count(item => item.Severity == Severity.Warning) > 0
                    ? Status.PassedWithWarnings
                    : ResultItens.Count(item => item.Severity == Severity.Information) > 0 
                        ? Status.PassedWithInformations
                        : Status.Passed;

        public List<ResultItem> ResultItens = new List<ResultItem>();
    }

    public struct ResultItem{
        public string Rule { get; set; }
        public string Description { get; set; }
        public string Details { get; set; }
        public Severity Severity { get; set; }
        public string Value { get; set; }

        public ResultItem(RuleBase rule)
        {
            Rule = rule.Name;
            Description = rule.Description;
            Details = rule.Details;
            Severity = rule.Severity;
            Value = null;
        }
    }

    public enum Status
    {
        None,
        Passed,
        PassedWithWarnings,
        PassedWithInformations,
        Error
    }
}
