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
    public class ErrorResponseFormatRuleTest
    {
        private OpenApiDocument openApiContract;
        private Supressions supressions;
        private Supressions supressionEntireRule;
        private IOptions<RuleSettings> ruleSettings;
        private const string ruleName = "ErrorResponseFormat";
        private OpenApiDocumentCache cache; 
        public ErrorResponseFormatRuleTest()
        {
            const string contract =
                "{\n  \"swagger\" : \"2.0\",\n  \"info\" : {\n    \"version\" : \"v1\",\n    \"title\" : \"Swagger Test\",\n  },\n  \"consumes\" : [ \"application/json\" ],\n  \"produces\" : [ \"application/json\" ],\n  \"paths\" : {\n    \"/path-one\" : {\n      \"get\" : {\n        \"responses\" : {\n          \"400\" : {\n            \"description\": \"Status 400\",\n            \"schema\" : {\n              \"type\" : \"object\",\n              \"properties\" : {\n                \"codeX\" : {\n                  \"type\" : \"string\"\n                },\n                \"messageX\" : {\n                  \"type\" : \"string\"\n                },\n                \"detailsX\" : {\n                  \"type\" : \"string\"\n                }\n              }                \n            }\n          },\n          \"500\" : {\n            \"description\": \"Status 500\",\n            \"schema\" : {\n              \"type\" : \"object\",\n              \"properties\" : {\n                \"codeX\" : {\n                  \"type\" : \"string\"\n                },\n                \"messageX\" : {\n                  \"type\" : \"string\"\n                },\n                \"detailsX\" : {\n                  \"type\" : \"string\"\n                }\n              }                \n            }\n          }\n        }\n      }\n    },\n    \"/path-two\" : {\n      \"get\" : {\n        \"responses\" : {\n          \"500\" : {\n            \"description\": \"Status 200\",\n            \"schema\" : {\n              \"type\" : \"object\",\n              \"properties\" : {\n                \"codeX\" : {\n                  \"type\" : \"string\"\n                },\n                \"messageX\" : {\n                  \"type\" : \"string\"\n                },\n                \"detailsX\" : {\n                  \"type\" : \"string\"\n                }\n              }                \n            }\n          }\n        }\n      }\n    }\n  }\n}";
            openApiContract = new OpenApiStringReader().Read(contract, out OpenApiDiagnostic _);

            supressions = new Supressions(new List<Supression>
            {
                new Supression
                {
                    RuleName = ruleName,
                    Target = "Path='/path-one',Operation='get',ResponseCode='400'"
                }
            });

            supressionEntireRule = new Supressions(
                new List<Supression> {new Supression {RuleName = ruleName, Target = "*"}});

            var settings = new RuleSettings()
            {
                ErrorResponseFormat = new ErrorResponseFormatConfig()
                {
                    ObligatoryErrorProperties = "code,message",
                    NonObligatoryErrorProperties = "details,fields.name,fields.message,fields.value,fields.detail"
                },
                NestingDepth = new NestingDepthConfig()
                {
                    Depth = 20    
                }
            };

            ruleSettings = Options.Create(settings);
            
            cache = new OpenApiDocumentCache();
        }

        [Fact]
        public void Execute()
        {
            List<ResultItem> results = new ErrorResponseFormatRule(openApiContract, supressions, ruleSettings, cache).Execute();

            Assert.Equal("path:/path-one,method:get,response:500", results[0].Value);

            Assert.Equal("path:/path-two,method:get,response:500", results[1].Value);

            Assert.True(new ErrorResponseFormatRule(openApiContract, supressionEntireRule, ruleSettings, cache).Execute().Count ==
                        0);
        }
    }
}
