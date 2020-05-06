using System.Collections.Generic;
using System.Linq;
using Jhulis.Core.Exceptions;
using System;
using System.Reflection;

namespace Jhulis.Core
{
    public class Supressions
    {
        private readonly Dictionary<string, Dictionary<string, string>> supressionsDictionary;

        public Supressions(List<Supression> supressions)
        {
            supressionsDictionary = new Dictionary<string, Dictionary<string, string>>();

            if (supressions == null) return;

            foreach (Supression supression in supressions)
            {
                //This throw error if invalid RuleName
                supressionsDictionary.TryAdd(
                    Rules.RuleBase.TryGetValueFromResource($"{supression.RuleName}.Name"),
                    new Dictionary<string, string>()); ;

                //This throw error if invalid Target structure;

                ValidateSupressionTarget(supression.Target);

                string[] targetSegments = supression.Target.Split(',');
                foreach (string targetSegment in targetSegments)
                {
                    string[] keyValueTargetSegment = targetSegment.Split('=');

                    string value = keyValueTargetSegment.Length >= 2
                        ? keyValueTargetSegment[1].TrimStart('\'').TrimEnd('\'')
                        : string.Empty;

                    supressionsDictionary[supression.RuleName]
                        .TryAdd(keyValueTargetSegment[0], value);
                }
            }
        }

        private static void ValidateSupressionTarget(string target)
        {
            //Path='/collection/{collectionId}/sub-collection'
            //PathSegment='sub-collection'
            //Operation='get'
            //Parameter='city'
            //ResponseCode='200'
            //Content='application/json' <------??? Não tenho certeza.
            //PropertyFull='address.city'

            var validSegments = new[]
                {"*", "Path", "Operation", "Parameter", "ResponseCode", "Content", "PropertyFull"};

            string[] targetSegments = target.Split(',');

            foreach (string targetSegment in targetSegments)
            {
                string[] keyValueTargetSegment = targetSegment.Split('=');

                if (targetSegment != "*" && keyValueTargetSegment.Length != 2)
                    throw new InvalidSupressionPathException(
                        $"This '{targetSegment}' is not a valid supression segment.");

                if (!validSegments.Contains(keyValueTargetSegment[0]))
                    throw new InvalidSupressionPathException(
                        $"This '{keyValueTargetSegment[0]}' is not a valid supression segment key.");

                if (targetSegment != "*" && !keyValueTargetSegment[1].StartsWith('\'') &&
                    !keyValueTargetSegment[1].EndsWith('\''))
                    throw new InvalidSupressionPathException(
                        $"This '{keyValueTargetSegment[1]}' is not a valid supression segment value. It needs to be between single quotes.");
            }
        }

        public bool IsSupressed(string ruleName)
        {
            return IsSupressed(ruleName, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
        }

        public bool IsSupressed(string ruleName, string path)
        {
            return IsSupressed(ruleName, path, string.Empty, string.Empty, string.Empty, string.Empty);
        }

        public bool IsSupressed(string ruleName, string path, string operation)
        {
            return IsSupressed(ruleName, path, operation, string.Empty, string.Empty, string.Empty);
        }

        public bool IsSupressed(string ruleName, string path, string operation, string parameter)
        {
            return IsSupressed(ruleName, path, operation, parameter, string.Empty, string.Empty);
        }

        public bool IsSupressed(string ruleName, string path, string operation, string parameter,
            string responseCode)
        {
            return IsSupressed(ruleName, path, operation, parameter, responseCode, string.Empty);
        }

        public bool IsSupressed(string ruleName, string path, string operation, string parameter,
            string responseCode, string propertyFull)
        {
            return
                supressionsDictionary.ContainsKey(ruleName) &&
                (
                    supressionsDictionary[ruleName].ContainsKey("*") ||
                    (
                        supressionsDictionary[ruleName].ContainsKey("Path")
                        && supressionsDictionary[ruleName]["Path"] == path
                        || !supressionsDictionary[ruleName].ContainsKey("Path")
                    ) && (supressionsDictionary[ruleName].ContainsKey("Operation")
                          && supressionsDictionary[ruleName]["Operation"].ToLowerInvariant() == operation.ToLowerInvariant()
                          || !supressionsDictionary[ruleName].ContainsKey("Operation")
                    ) && (supressionsDictionary[ruleName].ContainsKey("Parameter")
                          && supressionsDictionary[ruleName]["Parameter"] == parameter
                          || !supressionsDictionary[ruleName].ContainsKey("Parameter")
                    ) && (supressionsDictionary[ruleName].ContainsKey("ResponseCode")
                          && supressionsDictionary[ruleName]["ResponseCode"] == responseCode
                          || !supressionsDictionary[ruleName].ContainsKey("ResponseCode")
                    ) && (supressionsDictionary[ruleName].ContainsKey("PropertyFull")
                          && supressionsDictionary[ruleName]["PropertyFull"] == propertyFull
                          || !supressionsDictionary[ruleName].ContainsKey("PropertyFull")
                    ));
        }
    }
}
