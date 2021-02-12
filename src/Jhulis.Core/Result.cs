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

        public ResultItem(RuleBase rule) : this(rule, null) { }

        public ResultItem(RuleBase rule, string resultText)
        {
            Rule = rule.Name;
            Description = rule.Description;
            Details = rule.Details;
            Severity = rule.Severity;
            Value = resultText;
        }

        public ResultItem(RuleBase rule, 
            string path = null,
            string pathSegment = null,
            string pathParameter = null,
            string queryParameter = null,
            string method = null,
            string requestHeader = null,
            string requestProperty = null,
            string cookie = null,
            string response = null,
            string responseHeader = null,
            string content = null,
            string responseProperty = null) : this(rule, null)
        {
            Value = FormatValue(path, pathSegment, pathParameter, queryParameter, method, requestHeader, 
                requestProperty, cookie, response, responseHeader, content, responseProperty);
        }

        public static string FormatValue(
            string path = null,
            string pathSegment = null,
            string pathParameter = null,
            string queryParameter = null, 
            string method = null, 
            string requestHeader = null,
            string requestProperty = null,
            string cookie = null,
            string response = null, 
            string responseHeader = null, 
            string content = null, 
            string responseProperty = null)
        {
            string formatedResult = "";

            if (!string.IsNullOrEmpty(path)) formatedResult += $"path:{path},";
            if (!string.IsNullOrEmpty(pathSegment)) formatedResult += $"path-segment:{pathSegment},";
            if (!string.IsNullOrEmpty(pathParameter)) formatedResult += $"path-parameter:{pathParameter},";
            if (!string.IsNullOrEmpty(queryParameter)) formatedResult += $"query-paramter:{queryParameter},";
            if (!string.IsNullOrEmpty(method)) formatedResult += $"method:{method},";
            if (!string.IsNullOrEmpty(requestHeader)) formatedResult += $"request.header:{requestHeader},";
            if (!string.IsNullOrEmpty(requestProperty)) formatedResult += $"request.property:{requestProperty},";
            if (!string.IsNullOrEmpty(cookie)) formatedResult += $"cookie:{cookie},";
            if (!string.IsNullOrEmpty(response)) formatedResult += $"response:{response},";
            if (!string.IsNullOrEmpty(responseHeader)) formatedResult += $"response.header:{responseHeader},";
            if (!string.IsNullOrEmpty(content)) formatedResult += $"content:{content},";
            if (!string.IsNullOrEmpty(responseProperty)) formatedResult += $"response.property:{responseProperty},";

            formatedResult = formatedResult.Remove(formatedResult.LastIndexOf(','));

            return formatedResult;
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
