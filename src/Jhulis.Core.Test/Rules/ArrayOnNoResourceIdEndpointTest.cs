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
    public class ArrayOnNoResourceIdEndpointTest
    {
        private RuleSet localizer;
        private OpenApiDocument openApiContract;
        private Supressions supressions;
        private Supressions supressionEntireRule;
        private IOptions<RuleSettings> ruleSettings;
        private const string ruleName = "ArrayOnNoResourceIdEndpoint";
        private OpenApiDocumentCache cache; 

        public ArrayOnNoResourceIdEndpointTest()
        {
            const string contract =
                "{\n  \"swagger\" : \"2.0\",\n  \"info\" : {\n    \"version\" : \"v1\",\n    \"title\" : \"Swagger Test\",\n  },\n  \"consumes\" : [ \"application/json\" ],\n  \"produces\" : [ \"application/json\" ],\n  \"paths\" : {\n    \"/path-one\" : {\n      \"get\" : {\n        \"responses\" : {\n          \"200\" : {\n            \"description\" : \"Will not hit the Rule because there are not data envelope\",\n            \"schema\" : {\n              \"type\" : \"object\",\n              \"properties\" : {\n                \"noDataEnvelope\" : {\n                  \"type\" : \"object\",\n                  \"properties\" : {\n                    \"propertyOne\" : {\n                      \"type\" : \"string\"\n                    }\n                  }\n                }\n              }\n            }\n          }\n        }\n      }\n    },\n    \"/paths-two\" : {\n      \"get\" : {\n        \"responses\" : {\n          \"200\" : {\n            \"description\" : \"Will hit the rule: plural, data, no array.\",\n            \"schema\" : {\n              \"type\" : \"object\",\n              \"properties\" : {\n                \"data\" : {\n                  \"type\" : \"object\",\n                  \"properties\" : {\n                    \"propertyOne\" : {\n                      \"type\" : \"string\"\n                    }\n                  }\n                }\n              }\n            }\n          }\n        }\n      }\n    },\n    \"/path-three\" : {\n      \"get\" : {\n        \"responses\" : {\n          \"200\" : {\n            \"description\" : \"Will not hit the rule: singular\",\n            \"schema\" : {\n              \"type\" : \"object\",\n              \"properties\" : {\n                \"data\" : {\n                  \"type\" : \"object\",\n                  \"properties\" : {\n                    \"propertyOne\" : {\n                      \"type\" : \"string\"\n                    }\n                  }\n                }\n              }\n            }\n          }\n        }\n      }\n    },\n    \"/path-fours\" : {\n      \"get\" : {\n        \"responses\" : {\n          \"200\" : {\n            \"description\" : \"Will not hit the rule: empty\"\n          }\n        }\n      }\n    },\n    \"/path-fives\" : {\n      \"get\" : {\n        \"responses\" : {\n          \"200\" : {\n            \"description\" : \"Will not hit the rule: array ok\",\n            \"schema\" : {\n              \"type\" : \"object\",\n              \"properties\" : {\n                \"data\" : {\n                  \"type\" : \"array\",\n                  \"items\": {\n \t\t\t\t\t\"type\" : \"object\",\n                    \"properties\" : {\n                      \"propertyOne\" : {\n                    \t\"type\" : \"string\"\n                      }\n                  \t}\n                  }\n                }\n              }\n            }\n          }\n        }\n      }\n    },\n    \"/paths-six/{PathSixId}\" : {\n      \"get\" : {\n        \"parameters\": [ {\n          \"in\": \"path\",\n          \"name\": \"PathSixId\",\n          \"required\": true,\n          \"type\": \"string\"\n        }],\n        \"responses\" : {\n          \"200\" : {\n            \"description\" : \"Will not hit the rule: {id}\"\n          }\n        }\n      }\n    },\n    \"/paths-seven\" : {\n      \"get\" : {\n        \"responses\" : {\n          \"200\" : {\n            \"description\" : \"Will hit the rule: plural, data, no array.\",\n            \"schema\" : {\n              \"type\" : \"object\",\n              \"properties\" : {\n                \"data\" : {\n                  \"type\" : \"object\",\n                  \"properties\" : {\n                    \"propertyOne\" : {\n                      \"type\" : \"string\"\n                    }\n                  }\n                }\n              }\n            }\n          }\n        }\n      }\n    }\n  }\n}";
            openApiContract = new OpenApiStringReader().Read(contract, out OpenApiDiagnostic _);

            var settings = new RuleSettings()
            {
                ArrayOnNoResourceIdEndpoint = new ArrayOnNoResourceIdEndpointConfig()
                {
                    Example = "\"data\": [ { object 1 }, { object n } ]"
                }
            };

            ruleSettings = Options.Create(settings);

            supressions = new Supressions(new List<Supression>
            {
                new Supression {RuleName = ruleName, Target = "Path='/paths-two',Operation='get',ResponseCode='200'"}
            });

            supressionEntireRule = new Supressions(
              new List<Supression> {new Supression {RuleName = ruleName, Target = "*"}});
            
            cache = new OpenApiDocumentCache();
        }

        [Fact]
        public void Execute()
        {
            List<ResultItem> results = new ArrayOnNoResourceIdEndpointRule(openApiContract, supressions, ruleSettings, cache).Execute();

            Assert.Equal(1, results.Count);

            Assert.Equal("Path='/paths-seven',Operation='get',ResponseCode='200'", results[0].Value);

            Assert.True(new ArrayOnNoResourceIdEndpointRule(openApiContract, supressionEntireRule, ruleSettings, cache).Execute().Count == 0);
        }
    }
}
