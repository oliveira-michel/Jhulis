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

namespace Safra.Gsa.QaSwagger.Test.Rules
{
    public class EmptyExamplesRuleTest
    {
        private RuleSet localizer;
        private OpenApiDocument openApiContract;
        private Supressions supressions;
        private Supressions supressionEntireRule;
        private IOptions<RuleSettings> ruleSettings;
        private const string ruleName = "EmptyExamples";
        private OpenApiDocumentCache cache; 

        public EmptyExamplesRuleTest()
        {
            const string contract =
                "{\n  \"swagger\" : \"2.0\",\n  \"info\" : {\n    \"title\": \"teste 2\",\n    \"version\": \"1.0\"\n  },\n  \"consumes\" : [ \"application/json\" ],\n  \"produces\" : [ \"application/json\" ],\n  \"paths\" : {\n    \"/path-one\" : {\n      \"get\" : {\n        \"responses\" : {\n          \"200\" : {\n            \"description\" : \"Status 200\",\n            \"schema\" : {\n              \"type\" : \"object\",\n              \"properties\" : {\n                \"data\" : {\n                  \"type\" : \"object\",\n                  \"properties\" : {\n                    \"propertyOne\" : {\n                      \"type\" : \"string\",\n                      \"example\": \"An example\"\n                    },\n                    \"propertyTwo\" : {\n                      \"type\" : \"string\"\n                    }\n                  }\n                },\n                \"propertyThree\" : {\n                  \"type\" : \"string\"\n                }\n              }\n            }\n          }\n        }\n      }\n    },\n    \"/path-two\" : {\n      \"get\" : {\n        \"responses\" : {\n          \"200\" : {\n            \"description\" : \"Status 200\",\n            \"schema\" : {\n              \"type\" : \"object\",\n              \"properties\" : {\n                \"data\" : {\n                  \"type\" : \"object\",\n                  \"properties\" : {\n                    \"propertyFour\" : {\n                      \"type\" : \"string\"\n                    }\n                  }\n                }\n              }\n            }\n          }\n        }\n      }\n    }\n  }\n}";
            openApiContract = new OpenApiStringReader().Read(contract, out OpenApiDiagnostic _);

            supressions = new Supressions(new List<Supression>
            {
                new Supression
                {
                    RuleName = ruleName,
                    Target = "Path='/path-two',Operation='get',ResponseCode='200'"
                }
            });

            supressionEntireRule = new Supressions(
                new List<Supression> {new Supression {RuleName = ruleName, Target = "*"}});
            
            var settings = new RuleSettings()
            {
                NestingDepth = new NestingDepthConfig()
                {
                    Depth = 5    
                }
            };

            ruleSettings = Options.Create(settings);
            
            cache = new OpenApiDocumentCache();
        }

        [Fact]
        public void Execute()
        {
            List<ResultItem> results = new EmptyExamplesRule(openApiContract, supressions, ruleSettings, cache).Execute();
            
            Assert.Equal(3, results.Count);
            
            Assert.Equal(
                "Path='/path-one',Operation='get',Response='200',Parameter='data'",
                results[0].Value);
            
            Assert.Equal(
                "Path='/path-one',Operation='get',Response='200',Parameter='data.propertyTwo'",
                results[1].Value);

            Assert.Equal(
                "Path='/path-one',Operation='get',Response='200',Parameter='propertyThree'",
                results[2].Value);
            
            Assert.True(new EmptyExamplesRule(openApiContract, supressionEntireRule, ruleSettings, cache).Execute().Count == 0);
        }
    }
}
