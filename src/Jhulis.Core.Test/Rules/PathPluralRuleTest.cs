using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Xunit;
using Jhulis.Core.Helpers.Extensions;
using Jhulis.Core;
using Jhulis.Core.Rules;
using Jhulis.Core.Resources;

namespace Jhulis.Core.Test.Rules
{
    public class PathPluralRuleTest
    {
        private OpenApiDocument openApiContract;
        private Supressions supressions;
        private Supressions supressionEntireRule;
        private IOptions<RuleSettings> ruleSettings;
        private const string ruleName = "PathPlural";
        private OpenApiDocumentCache cache; 

        public PathPluralRuleTest()
        {
            const string contract =
                "{\r\n  \"swagger\" : \"2.0\",\r\n  \"info\" : {\r\n    \"version\" : \"v1\",\r\n    \"title\" : \"Swagger Test\",\r\n  },\r\n  \"consumes\" : [ \"application/json\" ],\r\n  \"produces\" : [ \"application/json\" ],\r\n  \"paths\" : {\r\n    \"/path-one\" : {\r\n      \"get\" : {\r\n        \"responses\" : {\r\n          \"200\" : {\r\n            \"description\" : \"Will not hit the Rule because there are not data envelope\",\r\n            \"schema\" : {\r\n              \"type\" : \"object\",\r\n              \"properties\" : {\r\n                \"noDataEnvelope\" : {\r\n                  \"type\" : \"object\",\r\n                  \"properties\" : {\r\n                    \"propertyOne\" : {\r\n                      \"type\" : \"string\"\r\n                    }\r\n                  }\r\n                }\r\n              }\r\n            }\r\n          }\r\n        }\r\n      }\r\n    },\r\n    \"/paths-two\" : {\r\n      \"get\" : {\r\n        \"responses\" : {\r\n          \"200\" : {\r\n            \"description\" : \"Will hit the rule: plural, data, no array.\",\r\n            \"schema\" : {\r\n              \"type\" : \"object\",\r\n              \"properties\" : {\r\n                \"data\" : {\r\n                  \"type\" : \"object\",\r\n                  \"properties\" : {\r\n                    \"propertyOne\" : {\r\n                      \"type\" : \"string\"\r\n                    }\r\n                  }\r\n                }\r\n              }\r\n            }\r\n          }\r\n        }\r\n      }\r\n    },\r\n    \"/path-three\" : {\r\n      \"get\" : {\r\n        \"responses\" : {\r\n          \"200\" : {\r\n            \"description\" : \"Will not hit the rule: singular\",\r\n            \"schema\" : {\r\n              \"type\" : \"object\",\r\n              \"properties\" : {\r\n                \"data\" : {\r\n                  \"type\" : \"object\",\r\n                  \"properties\" : {\r\n                    \"propertyOne\" : {\r\n                      \"type\" : \"string\"\r\n                    }\r\n                  }\r\n                }\r\n              }\r\n            }\r\n          }\r\n        }\r\n      }\r\n    },\r\n    \"/path-fours\" : {\r\n      \"get\" : {\r\n        \"responses\" : {\r\n          \"200\" : {\r\n            \"description\" : \"Will not hit the rule: empty\"\r\n          }\r\n        }\r\n      }\r\n    },\r\n    \"/path-fives\" : {\r\n      \"get\" : {\r\n        \"responses\" : {\r\n          \"200\" : {\r\n            \"description\" : \"Will not hit the rule: array ok\",\r\n            \"schema\" : {\r\n              \"type\" : \"object\",\r\n              \"properties\" : {\r\n                \"data\" : {\r\n                  \"type\" : \"array\",\r\n                  \"items\": {\r\n           \"type\" : \"object\",\r\n                    \"properties\" : {\r\n                      \"propertyOne\" : {\r\n                      \"type\" : \"string\"\r\n                      }\r\n                    }\r\n                  }\r\n                }\r\n              }\r\n            }\r\n          }\r\n        }\r\n      }\r\n    },\r\n    \"/paths-six/{PathSixId}\" : {\r\n      \"get\" : {\r\n        \"parameters\": [ {\r\n          \"in\": \"path\",\r\n          \"name\": \"PathSixId\",\r\n          \"required\": true,\r\n          \"type\": \"string\"\r\n        }],\r\n        \"responses\" : {\r\n          \"200\" : {\r\n            \"description\" : \"Will not hit the rule: {id}\"\r\n          }\r\n        }\r\n      }\r\n    },\r\n    \"/paths-seven/{PathSixId}\" : {\r\n      \"get\" : {\r\n        \"parameters\": [ {\r\n          \"in\": \"path\",\r\n          \"name\": \"PathSixId\",\r\n          \"required\": true,\r\n          \"type\": \"string\"\r\n        }],\r\n        \"responses\" : {\r\n          \"200\" : {\r\n            \"description\" : \"Will not hit the rule: {id}\"\r\n          }\r\n        }\r\n      }\r\n    },\r\n    \"/health\" : {\r\n      \"get\" : {\r\n        \"responses\" : {\r\n          \"200\" : {\r\n            \"description\" : \"Will not hit the rule, because it is at exceptions.\"\r\n          }\r\n        }\r\n      }\r\n    }\r\n  }\r\n}";
            openApiContract = new OpenApiStringReader().Read(contract, out OpenApiDiagnostic _);

            supressions = new Supressions(new List<Supression>
            {
                new Supression {RuleName = ruleName, Target = "Path='/path-one'"}
            });

            supressionEntireRule = new Supressions(
                new List<Supression> {new Supression {RuleName = ruleName, Target = "*"}});

            var settings = new RuleSettings()
            {
                PathPlural = new PathPluralConfig()
                {
                    Exceptions = "health,xpto"
                }
            };

            ruleSettings = Options.Create(settings);

            cache = new OpenApiDocumentCache();

            ruleSettings.Value.PathPlural.Exceptions = "health,xpto";
        }

        [Fact]
        public void Execute()
        {
            List<ResultItem> results = new PathPluralRule(openApiContract, supressions, ruleSettings, cache).Execute();

            Assert.Equal("path:/path-three,path-segment:path-three", results[0].Value);

            Assert.Equal(1, results.Count);

            Assert.True(new PathPluralRule(openApiContract, supressionEntireRule, ruleSettings, cache).Execute().Count == 0);
        }
    }
}
