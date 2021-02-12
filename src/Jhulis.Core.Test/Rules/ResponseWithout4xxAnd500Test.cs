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
    public class ResponseWithout4xxAnd500Test
    {
        private OpenApiDocument openApiContract;
        private Supressions supressions;
        private Supressions supressionEntireRule;
        private IOptions<RuleSettings> ruleSettings;
        private const string ruleName = "ResponseWithout4xxAnd500";
        private OpenApiDocumentCache cache; 

        public ResponseWithout4xxAnd500Test()
        {
            const string contract =
                "{\n  \"swagger\": \"2.0\",\n  \"info\": {\n    \"title\": \"teste 2\",\n    \"version\": \"1.0\"\n  },\n  \"consumes\": [\n    \"application/json\"\n  ],\n  \"produces\": [\n    \"application/json\"\n  ],\n  \"paths\": {\n    \"/path-one\": {\n      \"get\": {\n        \"responses\": {\n          \"201\": {\n            \"description\": \"Status 200\",\n            \"schema\": {\n              \"type\": \"object\",\n              \"properties\": {\n                \"noDataEnvelope\": {\n                  \"type\": \"object\",\n                  \"properties\": {\n                    \"propertyOne\": {\n                      \"type\": \"string\"\n                    }\n                  }\n                }\n              }\n            }\n          }\n        }\n      }\n    },\n    \"/path-two\": {\n      \"get\": {\n        \"responses\": {\n          \"201\": {\n            \"description\": \"Status 200\",\n            \"schema\": {\n              \"type\": \"object\",\n              \"properties\": {\n                \"noDataEnvelope\": {\n                  \"type\": \"object\",\n                  \"properties\": {\n                    \"propertyOne\": {\n                      \"type\": \"string\"\n                    }\n                  }\n                }\n              }\n            }\n          }\n        }\n      }\n    }\n  }\n}";
            openApiContract = new OpenApiStringReader().Read(contract, out OpenApiDiagnostic _);

            supressions = new Supressions(new List<Supression>
            {
                new Supression {RuleName = ruleName, Target = "Path='/path-one',Operation='get'"}
            });

            supressionEntireRule = new Supressions(
                new List<Supression> {new Supression {RuleName = ruleName, Target = "*"}});
            
            cache = new OpenApiDocumentCache();
        }

        [Fact]
        public void Execute()
        {
            List<ResultItem> results =
                new ResponseWithout4xxAnd500Rule(openApiContract, supressions, ruleSettings, cache).Execute();

            Assert.Equal("path:/path-two,method:get", results[0].Value);

            Assert.True(new ResponseWithout4xxAnd500Rule(openApiContract, supressionEntireRule, ruleSettings, cache).Execute()
                            .Count == 0);
        }
    }
}
