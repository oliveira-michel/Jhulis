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
using Jhulis.Core.Resources;

namespace Safra.Gsa.QaSwagger.Test.Rules
{
    public class DateWithoutFormatRuleTest
    {
        private OpenApiDocument openApiContract;
        private Supressions supressions;
        private Supressions supressionEntireRule;
        private IOptions<RuleSettings> ruleSettings;
        private const string ruleName = "DateWithoutFormat";
        private OpenApiDocumentCache cache; 
        public DateWithoutFormatRuleTest()
        {
            const string contract = "{\n  \"swagger\" : \"2.0\",\n  \"info\" : {\n    \"title\": \"teste 2\",\n    \"version\": \"1.0\"\n  },\n  \"consumes\" : [ \"application/json\" ],\n  \"produces\" : [ \"application/json\" ],\n  \"paths\" : {\n    \"/path-one\" : {\n      \"get\" : {\n        \"responses\" : {\n          \"200\" : {\n            \"description\" : \"Status 200\",\n            \"schema\" : {\n              \"type\" : \"object\",\n              \"properties\" : {\n                 \"zero\" : {\n                  \"type\" : \"string\",\n                  \"example\": \"2020-07-09\"\n                },\n                \"one\" : {\n                  \"type\" : \"string\",\n                  \"example\": \"2020-07-08\"\n                },\n                \"two\" : {\n                  \"type\" : \"string\",\n                  \"example\": \"2017-07-21T17:32:28Z\"\n                },\n                \"three\" : {\n                  \"type\" : \"string\",\n                  \"example\": \"2019/09/11\"\n                },\n                \"four\" : {\n                  \"type\" : \"string\",\n                  \"example\": \"13:29\"\n                },\n                \"five\" : {\n                  \"type\" : \"string\",\n                  \"example\": \"04/03/1998\"\n                },\n                 \"six\" : {\n                  \"type\" : \"string\",\n                  \"example\": \"2017-07-21\",\n                  \"format\": \"date\"\n                }\n              }\n            }\n          }\n        }\n      }\n    },\n    \"/path-two\" : {\n      \"get\" : {\n        \"responses\" : {\n          \"200\" : {\n            \"description\" : \"Status 200\",\n            \"schema\" : {\n              \"type\" : \"object\",\n              \"properties\" : {\n                \"data\" : {\n                  \"type\" : \"object\",\n                  \"properties\" : {\n                    \"seven\" : {\n                      \"type\" : \"string\",\n                      \"example\": \"6\"\n                    },\n                    \"eight\" : {\n                      \"type\" : \"string\",\n                      \"example\": \"A77\"\n                    }\n                  }\n                }\n              }\n            }\n          }\n        }\n      }\n    }\n  }\n}";
            openApiContract = new OpenApiStringReader().Read(contract, out OpenApiDiagnostic _);

            supressions = new Supressions(new List<Supression>
            {
                new Supression
                {
                    RuleName = ruleName,
                    Target = "Path='/path-one',Operation='get',ResponseCode='200',Parameter='zero'"
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
            List<ResultItem> results = new DateWithoutFormatRule(openApiContract, supressions, ruleSettings, cache).Execute();

            Assert.Equal(5, results.Count);
            
            Assert.Equal(@"Path='/path-one',Operation='get',Parameter='one'",
                results[0].Value);
            Assert.Equal(@"Path='/path-one',Operation='get',Parameter='two'",
                results[1].Value);
            Assert.Equal(@"Path='/path-one',Operation='get',Parameter='three'",
                results[2].Value);
            Assert.Equal(@"Path='/path-one',Operation='get',Parameter='four'",
                results[3].Value);
            Assert.Equal(@"Path='/path-one',Operation='get',Parameter='five'",
                results[4].Value);
            
            Assert.True(new DateWithoutFormatRule(openApiContract, supressionEntireRule, ruleSettings, cache).Execute().Count == 0);
        }
    }
}
