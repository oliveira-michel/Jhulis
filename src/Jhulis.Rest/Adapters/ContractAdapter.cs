using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jhulis.Core;
using Jhulis.Rest.Models;

namespace Jhulis.Adapters
{
    public static class ContractAdapter
    {
        private static IEnumerable<ResultItemModel> ToIEnumerableOfResultItemModel(this Result result)
        {
            foreach (ResultItem resultItem in result.ResultItens)
                yield return resultItem.ToResultItemModel();
        }

        private static ResultItemModel ToResultItemModel(this ResultItem result)
        {
            return new ResultItemModel
            {
                Rule = result.Rule,
                Description = result.Description,
                Message = result.Details,
                Severity = result.Severity.ToString(),
                Value = result.Value
            };
        }

        public static ValidarGetResponseModel ToValidarResponseGetModel(this Result result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));

            return new ValidarGetResponseModel
            {
                Result = result.Status.ToString(),
                ResultItens = result.ToIEnumerableOfResultItemModel().ToList()
            };
        }

        public static List<Core.Supression> ToSupressions(this List<Rest.Models.Supression> supresionList)
        {
            if (supresionList == null) throw new ArgumentNullException(nameof(supresionList));

            var supressions = new List<Core.Supression>();

            foreach (var supression in supresionList)
                supressions.Add(new Core.Supression()
                {
                    Justification = supression.Justification,
                    RuleName = supression.RuleName,
                    Target = supression.Target
                });

            return supressions;
        }
        public static string ToText(this Result result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            
            var sb = new StringBuilder();
            sb.Append("Result: ").Append(result.Status).Append(Environment.NewLine).Append(Environment.NewLine);

            foreach (ResultItem resultResultItem in result.ResultItens)
            {
                sb.Append("Rule: ").Append(resultResultItem.Rule).Append(Environment.NewLine)
                .Append("Value: ").Append(resultResultItem.Value).Append(Environment.NewLine)
                .Append("Description: ").Append(resultResultItem.Description).Append(Environment.NewLine)
                .Append("Details: ").Append(resultResultItem.Details).Append(Environment.NewLine)
                .Append("Severity: ").Append(resultResultItem.Severity.ToString()).Append(Environment.NewLine);

                sb.Append(Environment.NewLine);
            }

            return sb.ToString();
        }
    }
}
