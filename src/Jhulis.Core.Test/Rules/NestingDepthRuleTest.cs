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
    public class NestingDepthRuleTest
    {
        private OpenApiDocument openApiContract;
        private Supressions supressions;
        private Supressions supressionEntireRule;
        private IOptions<RuleSettings> ruleSettings;
        private const string ruleName = "NestingDepth";
        private OpenApiDocumentCache cache; 

        public NestingDepthRuleTest()
        {
            const string contract =
                "{\n  \"swagger\" : \"2.0\",\n  \"info\" : {\n    \"version\" : \"v1\",\n    \"title\" : \"Swagger Test\",\n  },\n  \"consumes\" : [ \"application/json\" ],\n  \"produces\" : [ \"application/json\" ],\n  \"paths\" : {\n    \"/path-one\" : {\n      \"get\" : {\n        \"responses\" : {\n          \"200\" : {\n            \"description\" : \"Status 200\",\n            \"schema\" : {\n              \"type\" : \"object\",\n              \"properties\" : {\n                \"data\" : {\n                  \"type\" : \"object\",\n                  \"properties\" : {\n                    \"level1\" : {\n                      \"type\" : \"string\"\n                    },\n                    \"level1b\" : {\n                      \"type\" : \"object\",\n                      \"properties\" : {\n                        \"level2\": {\n                          \"type\" : \"object\",\n                          \"properties\" : {\n                            \"level3\": {\n                              \"type\" : \"object\",\n                              \"properties\" : {\n                                \"level4\": {\n                                    \"type\" : \"object\",\n                                    \"properties\" : {\n                                    \"level5\": {\n                                      \"type\" : \"string\"\n                                    }\n                                  }\n                                }\n                              }\n                            }\n                          }\n                        }\n                      }\n                    }\n                  }\n                }\n              }\n            }\n          }\n        }\n      }\n    },\n    \"/path-two\" : {\n      \"get\" : {\n        \"responses\" : {\n          \"200\" : {\n            \"description\" : \"Status 200\",\n            \"schema\" : {\n              \"type\" : \"object\",\n              \"properties\" : {\n                \"data\" : {\n                  \"type\" : \"object\",\n                  \"properties\" : {\n                    \"level1\" : {\n                      \"type\" : \"string\"\n                    },\n                    \"level1b\" : {\n                      \"type\" : \"object\",\n                      \"properties\" : {\n                        \"level2\": {\n                          \"type\" : \"string\"\n                        }\n                      }\n                    }\n                  }\n                }\n              }\n            }\n          }\n        }\n      }\n    },\n    \"/path-three\" : {\n      \"get\" : {\n        \"responses\" : {\n          \"200\" : {\n            \"description\" : \"Status 200\",\n            \"schema\" : {\n              \"type\" : \"object\",\n              \"properties\" : {\n                \"data\" : {\n                  \"type\" : \"object\",\n                  \"properties\" : {\n                    \"level1\" : {\n                      \"type\" : \"string\"\n                    },\n                    \"level1b\" : {\n                      \"type\" : \"object\",\n                      \"properties\" : {\n                        \"level2\": {\n                          \"type\" : \"object\",\n                          \"properties\" : {\n                            \"level3\": {\n                              \"type\" : \"object\",\n                              \"properties\" : {\n                                \"level4\": {\n                                    \"type\" : \"object\",\n                                    \"properties\" : {\n                                    \"level5\": {\n                                      \"type\" : \"string\"\n                                    }\n                                  }\n                                }\n                              }\n                            }\n                          }\n                        }\n                      }\n                    }\n                  }\n                }\n              }\n            }\n          }\n        }\n      }\n    }\n  }\n}";
            openApiContract = new OpenApiStringReader().Read(contract, out OpenApiDiagnostic _);

            var settings = new RuleSettings()
            {
                NestingDepth = new NestingDepthConfig()
                {
                    Depth = 5
                }
            };

            ruleSettings = Options.Create(settings);

            supressions = new Supressions(new List<Supression>
            {
                new Supression {RuleName = ruleName, Target = "Path='/path-three',Operation='get',ResponseCode='200'"}
            });

            supressionEntireRule = new Supressions(
              new List<Supression> {new Supression {RuleName = ruleName, Target = "*"}});
            
            cache = new OpenApiDocumentCache();
        }

        [Fact]
        public void Execute()
        {
            List<ResultItem> results = new NestingDepthRule(openApiContract, supressions, ruleSettings, cache).Execute();

            Assert.Equal(1, results.Count);

            Assert.Equal("path:/path-one,method:get,response:200,content:application/json,response.property:data.level1b.level2.level3.level4.level5", results[0].Value);

            Assert.True(new NestingDepthRule(openApiContract, supressionEntireRule, ruleSettings, cache).Execute().Count == 0);
        }
    }
}
