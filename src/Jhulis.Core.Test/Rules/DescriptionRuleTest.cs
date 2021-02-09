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
    public class DescriptionRuleTest
    {
        private OpenApiDocument openApiContract;
        private Supressions supressions;
        private Supressions supressionEntireRule;
        private IOptions<RuleSettings> ruleSettings;
        private const string ruleName = "Description";
        private OpenApiDocumentCache cache; 

        public DescriptionRuleTest()
        {
            const string contract =
                "{\n  \"swagger\" : \"2.0\",\n  \"info\" : {\n    \"version\" : \"v1\",\n    \"title\" : \"Swagger Test\",\n  },\n  \"consumes\" : [ \"application/json\" ],\n  \"produces\" : [ \"application/json\" ],\n  \"paths\" : {\n    \"/path-one\" : {\n      \"get\" : {\n        \"responses\" : {\n          \"200\" : {\n            \"schema\" : {\n              \"type\" : \"object\",\n              \"properties\" : {\n                \"data\" : {\n                  \"type\" : \"object\",\n                  \"properties\" : {\n                    \"propertyOne\" : {\n                      \"type\" : \"string\"\n                    }\n                  }\n                }\n              }\n            }\n          }\n        }\n      }\n    },\n    \"/path-two\" : {\n      \"get\" : {\n        \"parameters\" : [{\n            \"in\" : \"query\",\n            \"name\" : \"parameterOne\",\n            \"type\" : \"string\",\n            \"description\" : \"too short\",\n            \"required\" : true\n        }]\n        ,\n        \"responses\" : {\n          \"200\" : {\n            \"description\": \"Status 200\",\n            \"schema\" : {\n              \"type\" : \"object\",\n              \"properties\" : {\n                \"data\" : {\n                  \"type\" : \"object\",\n                  \"properties\" : {\n                    \"propertyOne\" : {\n                      \"type\" : \"string\"\n                    }\n                  }\n                }\n              }\n            }\n          }\n        }\n      }\n    }\n  }\n}";
            openApiContract = new OpenApiStringReader().Read(contract, out OpenApiDiagnostic _);

            supressions = new Supressions(new List<Supression>
            {
                new Supression
                {
                    RuleName = ruleName,
                    Target = "Path='/path-one',Operation='get',ResponseCode='200'"
                }
            });

            supressionEntireRule = new Supressions(
                new List<Supression> {new Supression {RuleName = ruleName, Target = "*"}});
            
            var settings = new RuleSettings()
            {
                Description = new DescriptionConfig()
                {
                    MinDescriptionLength = 5,
                    MidDescriptionLength = 15,
                    LargeDescriptionLength = 20,
                    TestDescriptionInOperation = true,
                    TestDescriptionInPaths = true
                }
            };

            ruleSettings = Options.Create(settings);
            
            cache = new OpenApiDocumentCache();
        }

        [Fact]
        public void Execute()
        {
            List<ResultItem> results = new DescriptionRule(openApiContract, supressions, ruleSettings, cache).Execute();

            Assert.Equal("Info.Description=''", results[0].Value);

            Assert.Equal("Path='/path-one'", results[1].Value);

            Assert.Equal("Path='/path-one',Operation='get'", results[2].Value);

            Assert.Equal("Path='/path-two'", results[3].Value);

            Assert.Equal("Path='/path-two',Operation='get'", results[4].Value);

            Assert.Equal(
                "Path='/path-two',Operation='get',Parameter='parameterOne',Value='too short'",
                results[5].Value);

            Assert.Equal(
                "Path='/path-two',Operation='get',Response='200',Description='Status 200'",
                results[6].Value);
            
            Assert.Equal(
                "Path='/path-two',Operation='get',ResponseCode='200',Content='application/json',PropertyFull='data.propertyOne','PropertyDescription=''",
                results[7].Value);

            Assert.True(new DescriptionRule(openApiContract, supressionEntireRule, ruleSettings, cache).Execute().Count == 0);
        }
    }
}
