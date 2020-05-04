using System;
using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Jhulis.Core.Helpers.Extensions;
using Jhulis.Core.Exceptions;
using System.Reflection;

namespace Jhulis.Core.Rules
{
    public abstract class RuleBase
    {
        protected readonly OpenApiDocument Contract;
        protected readonly Supressions Supressions;
        protected readonly OpenApiDocumentCache Cache;
        protected readonly IOptions<RuleSettings> RuleSettings;
        private protected List<ResultItem> listResult = new List<ResultItem>();
        private readonly string ruleName = string.Empty;

        private RuleBase(OpenApiDocument contract, Supressions supressions, IOptions<RuleSettings> ruleSettings, OpenApiDocumentCache cache)
        {
            this.RuleSettings = ruleSettings;
            this.Contract = contract;
            this.Supressions = supressions;
            this.Cache = cache;
        }

        protected RuleBase(OpenApiDocument contract, Supressions supressions,
            IOptions<RuleSettings> ruleSettings, OpenApiDocumentCache cache, string ruleName, Severity severity) :
            this(contract, supressions, ruleSettings, cache)
        {
            Name = TryGetValueFromResource($"{ruleName}.Name");
            Description = TryGetValueFromResource($"{ruleName}.Description");
            Details = TryGetValueFromResource($"{ruleName}.Details");
            Severity = severity;
            this.ruleName = ruleName;
        }

        public string Name { get; }
        
        // A meaningful feedback about the error
        public string Details { get; }

        // A long-form description of the rule formatted in markdown
        public string Description { get; protected set; }

        // The severity of results this rule generates
        public Severity Severity { get; }
        
        private protected virtual void ExecuteRuleLogic()
        {
            throw new NotImplementedException();
        }

        public List<ResultItem> Execute()
        {
            try
            {
                if (Supressions.IsSupressed(ruleName)) return listResult;
                
                ExecuteRuleLogic();
            }
            catch (Exception e)
            {
                listResult.Add(new ResultItem()
                {
                    Rule = ruleName,
                    Description = "An not expected error has ocurred during the rule execution.",
                    Details = $"StackTrace:\r{e}",
                    Severity = Severity.Error,
                    Value = ""
                });
            }
            return listResult;
        }

        internal static string TryGetValueFromResource(string key)
        {
            var keyFormated = key.Replace(".", "_");

            Type ruleSet = typeof(Resources.RuleSet);
            PropertyInfo propertyInfo = ruleSet.GetProperty(keyFormated);
            if (propertyInfo == null)
                throw new RuleNotFoundException(
                   $"Key '{key}' in supression definition not found on resource File. Verify if the rule exists.");

            return (string)propertyInfo.GetValue(null, null);
        }
    }
}
