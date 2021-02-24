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
                "{\r\n  \"swagger\": \"2.0\",\r\n  \"info\": {\r\n    \"title\": \"teste 2\",\r\n    \"version\": \"1.0\"\r\n  },\r\n  \"consumes\": [\r\n    \"application/json\"\r\n  ],\r\n  \"produces\": [\r\n    \"application/json\"\r\n  ],\r\n  \"paths\": {\r\n    \"/path-one\": {\r\n      \"get\": {\r\n        \"responses\": {\r\n          \"200\": {\r\n            \"description\": \"Status 200\",\r\n            \"schema\": {\r\n              \"type\": \"object\",\r\n              \"properties\": {\r\n                \"data\": {\r\n                  \"type\": \"object\",\r\n                  \"properties\": {\r\n                    \"propertyOne\": {\r\n                      \"type\": \"string\",\r\n                      \"example\": \"An example\"\r\n                    },\r\n                    \"propertyTwo\": {\r\n                      \"type\": \"string\"\r\n                    }\r\n                  }\r\n                },\r\n                \"propertyThree\": {\r\n                  \"type\": \"string\"\r\n                }\r\n              }\r\n            }\r\n          }\r\n        }\r\n      }\r\n    },\r\n    \"/path-two\": {\r\n      \"get\": {\r\n        \"responses\": {\r\n          \"200\": {\r\n            \"description\": \"Status 200\",\r\n            \"schema\": {\r\n              \"type\": \"object\",\r\n              \"properties\": {\r\n                \"data\": {\r\n                  \"type\": \"object\",\r\n                  \"properties\": {\r\n                    \"propertyFour\": {\r\n                      \"type\": \"string\"\r\n                    }\r\n                  }\r\n                }\r\n              }\r\n            }\r\n          }\r\n        }\r\n      }\r\n    },\r\n    \"/path-three\": {\r\n      \"get\": {\r\n        \"responses\": {\r\n          \"200\": {\r\n            \"description\": \"Status 200\",\r\n            \"schema\": {\r\n              \"type\": \"object\",\r\n              \"properties\": {\r\n                \"data\": {\r\n                  \"type\": \"object\",\r\n                  \"properties\": {\r\n                    \"propertyOne\": {\r\n                      \"type\": \"string\"\r\n                    },\r\n                    \"propertyTwo\": {\r\n                      \"type\": \"string\"\r\n                    }\r\n                  }\r\n                },\r\n                \"propertyThree\": {\r\n                  \"type\": \"string\"\r\n                }\r\n              }\r\n            },\r\n            \"examples\": {\r\n              \"example-1\": {\r\n                \"data\": {\r\n                  \"propertyOne\": \"An example at schema for propertyOne\",\r\n                  \"propertyTwo\": \"An example at schema for propertyTwo\"\r\n                },\r\n                \"propertyThree\": \"An example at schema for propertyThree\"\r\n              },\r\n              \"example-2\": {\r\n                \"data\": {\r\n                  \"propertyOne\": \"ex2\",\r\n                  \"propertyTwo\": \"ex2\"\r\n                },\r\n                \"propertyThree\": \"ex2\"\r\n              }\r\n            }\r\n          }\r\n        }\r\n      }\r\n    }\r\n  }\r\n}";
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
                "path:/path-one,method:get,response:200,content:application/json,response.property:data",
                results[0].Value);
            
            Assert.Equal(
                "path:/path-one,method:get,response:200,content:application/json,response.property:data.propertyTwo",
                results[1].Value);

            Assert.Equal(
                "path:/path-one,method:get,response:200,content:application/json,response.property:propertyThree",
                results[2].Value);
            
            Assert.True(new EmptyExamplesRule(openApiContract, supressionEntireRule, ruleSettings, cache).Execute().Count == 0);
        }
    }
}
