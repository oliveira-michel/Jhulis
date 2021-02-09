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
    public class ContentEnvelopeRuleTest
    {
        private RuleSet localizer;
        private OpenApiDocument openApiContract;
        private Supressions supressions;
        private Supressions supressionEntireRule;
        private IOptions<RuleSettings> ruleSettings;
        private const string ruleName = "ContentEnvelope";
        private OpenApiDocumentCache cache; 
        public ContentEnvelopeRuleTest()
        {
            const string contract =
                "{\n  \"swagger\" : \"2.0\",\n  \"info\" : {\n    \"version\" : \"v1\",\n    \"title\" : \"Swagger Test\",\n  },\n  \"consumes\" : [ \"application/json\" ],\n  \"produces\" : [ \"application/json\" ],\n  \"paths\" : {\n    \"/path-one\" : {\n      \"get\" : {\n        \"responses\" : {\n          \"200\" : {\n            \"description\" : \"Status 200\",\n            \"schema\" : {\n              \"type\" : \"object\",\n              \"properties\" : {\n                \"noDataEnvelope\" : {\n                  \"type\" : \"object\",\n                  \"properties\" : {\n                    \"propertyOne\" : {\n                      \"type\" : \"string\"\n                    }\n                  }\n                }\n              }\n            }\n          }\n        }\n      }\n    },\n    \"/path-two\" : {\n      \"get\" : {\n        \"responses\" : {\n          \"200\" : {\n            \"description\" : \"Status 200\",\n            \"schema\" : {\n              \"type\" : \"object\",\n              \"properties\" : {\n                \"noDataEnvelope\" : {\n                  \"type\" : \"object\",\n                  \"properties\" : {\n                    \"propertyOne\" : {\n                      \"type\" : \"string\"\n                    }\n                  }\n                }\n              }\n            }\n          }\n        }\n      }\n    },\n    \"/path-three\" : {\n      \"get\" : {\n        \"responses\" : {\n          \"200\" : {\n            \"description\" : \"Status 200\",\n            \"schema\" : {\n              \"type\" : \"object\",\n              \"properties\" : {\n                \"data\" : {\n                  \"type\" : \"object\",\n                  \"properties\" : {\n                    \"propertyOne\" : {\n                      \"type\" : \"string\"\n                    }\n                  }\n                }\n              }\n            }\n          }\n        }\n      }\n    },\n    \"/path-four\" : {\n      \"get\" : {\n        \"responses\" : {\n          \"200\" : {\n            \"description\" : \"Status 200\"\n          }\n        }\n      }\n    }\n  }\n}";
            openApiContract = new OpenApiStringReader().Read(contract, out OpenApiDiagnostic _);

            var settings = new RuleSettings()
            {
                ContentEnvelope = new ContentEnvelopeConfig()
                {
                    EnvelopeName = "data"
                }
            };

            ruleSettings = Options.Create(settings);

            supressions = new Supressions(new List<Supression>
            {
                new Supression {RuleName = ruleName, Target = "Path='/path-one',Operation='get',ResponseCode='200'"}
            });

            supressionEntireRule = new Supressions(
              new List<Supression> {new Supression {RuleName = ruleName, Target = "*"}});
            
            cache = new OpenApiDocumentCache();
        }

        [Fact]
        public void Execute()
        {
            List<ResultItem> results = new ContentEnvelopeRule(openApiContract, supressions, ruleSettings, cache).Execute();

            Assert.Equal(1, results.Count);

            Assert.Equal("Path='/path-two',Operation='get',ResponseCode='200'", results[0].Value);

            Assert.True(new ContentEnvelopeRule(openApiContract, supressionEntireRule, ruleSettings, cache).Execute().Count == 0);
        }
    }
}
