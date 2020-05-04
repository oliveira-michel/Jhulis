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
    public class Http200WithoutPaginationRuleTest
    {
        private OpenApiDocument openApiContract;
        private Supressions supressions;
        private Supressions supressionEntireRule;
        private IOptions<RuleSettings> ruleSettings;
        private const string ruleName = "Http200WithoutPagination";
        private OpenApiDocumentCache cache; 

        public Http200WithoutPaginationRuleTest()
        {
            const string contract =
                "{\n  \"swagger\" : \"2.0\",\n  \"info\" : {\n    \"version\" : \"v1\",\n    \"title\" : \"Swagger Test\",\n  },\n  \"consumes\" : [ \"application/json\" ],\n  \"produces\" : [ \"application/json\" ],\n  \"paths\" : {\n    \"/path-one\" : {\n      \"get\" : {\n        \"responses\" : {\n          \"200\" : {\n            \"description\": \"\",\n            \"schema\" : {\n              \"type\" : \"object\",\n              \"properties\" : {\n                \"data\" : {\n                  \"type\" : \"array\",\n                  \"items\" : {\n                    \"properties\" : {\n                      \"intPropertyOne\" : {\n                        \"type\" : \"number\"\n                      },\n                      \"flagUpdated\" : {\n                        \"type\" : \"number\"\n                      }\n                    }\n                  }\n                }\n              }\n            }\n          }\n        }\n      }\n    },\n    \"/path-two\" : {\n      \"get\" : {\n        \"parameters\" : [{\n            \"in\" : \"query\",\n            \"name\" : \"parameterOne\",\n            \"type\" : \"string\",\n            \"description\" : \"Too short, but ok.\",\n            \"required\" : true\n        }]\n        ,\n        \"responses\" : {\n          \"200\" : {\n            \"description\": \"\",\n            \"schema\" : {\n              \"type\" : \"object\",\n              \"properties\" : {\n                \"data\" : {\n                  \"type\" : \"array\",\n                  \"items\" : {\n                    \"properties\" : {\n                      \"intPropertyOne\" : {\n                        \"type\" : \"number\"\n                      },\n                      \"flagUpdated\" : {\n                        \"type\" : \"number\"\n                      }\n                    }\n                  }\n                }\n              }\n            }\n          }\n        }\n      }\n    }\n  }\n}";
            openApiContract = new OpenApiStringReader().Read(contract, out OpenApiDiagnostic _);

            supressions = new Supressions(new List<Supression>
            {
                new Supression {RuleName = ruleName, Target = "Path='/path-one'"}
            });

            supressionEntireRule = new Supressions(
                new List<Supression> {new Supression {RuleName = ruleName, Target = "*"}});
            
            var settings = new RuleSettings()
            {
                Http200WithoutPagination = new Http200WithoutPaginationConfig()
                {
                    PaginationEnvelopeName = "pagination",
                    ContentEnvelopeName = "data"
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
            List<ResultItem> results =
                new Http200WithoutPaginationRule(openApiContract, supressions, ruleSettings, cache).Execute();

            Assert.Equal("Path='/path-two'", results[0].Value);

            Assert.True(new Http200WithoutPaginationRule(openApiContract, supressionEntireRule, ruleSettings, cache).Execute()
                            .Count == 0);
        }
    }
}
