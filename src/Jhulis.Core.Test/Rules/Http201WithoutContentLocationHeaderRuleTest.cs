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
    public class Http201WithoutLocationHeaderRuleTest
    {
        private OpenApiDocument openApiContract;
        private Supressions supressions;
        private Supressions supressionEntireRule;
        private IOptions<RuleSettings> ruleSettings;
        private const string ruleName = "Http201WithoutLocationHeader";
        private OpenApiDocumentCache cache; 

        public Http201WithoutLocationHeaderRuleTest()
        {
            const string contract =
                "{\n  \"swagger\": \"2.0\",\n  \"info\": {\n    \"title\": \"teste 2\",\n    \"version\": \"1.0\"\n  },\n  \"consumes\": [\n    \"application/json\"\n  ],\n  \"produces\": [\n    \"application/json\"\n  ],\n  \"paths\": {\n    \"/path-one\": {\n      \"post\": {\n        \"responses\": {\n          \"201\": {\n            \"description\": \"Status 201\",\n            \"schema\": {\n              \"type\": \"object\",\n              \"properties\": {\n                \"noDataEnvelope\": {\n                  \"type\": \"object\",\n                  \"properties\": {\n                    \"propertyOne\": {\n                      \"type\": \"string\"\n                    }\n                  }\n                }\n              }\n            }\n          }\n        }\n      }\n    },\n    \"/path-two\": {\n      \"post\": {\n        \"responses\": {\n          \"201\": {\n            \"description\": \"Status 201\",\n            \"schema\": {\n              \"type\": \"object\",\n              \"properties\": {\n                \"noDataEnvelope\": {\n                  \"type\": \"object\",\n                  \"properties\": {\n                    \"propertyOne\": {\n                      \"type\": \"string\"\n                    }\n                  }\n                }\n              }\n            }\n          }\n        }\n      }\n    }\n  }\n}";
            openApiContract = new OpenApiStringReader().Read(contract, out OpenApiDiagnostic _);

            supressions = new Supressions(new List<Supression>
            {
                new Supression {RuleName = ruleName, Target = "Path='/path-one',Operation='post'"}
            });

            supressionEntireRule = new Supressions(
                new List<Supression> {new Supression {RuleName = ruleName, Target = "*"}});
            
            cache = new OpenApiDocumentCache();
        }

        [Fact]
        public void Execute()
        {
            List<ResultItem> results =
                new Http201WithoutLocationHeaderRule(openApiContract, supressions, ruleSettings, cache).Execute();

            Assert.Equal("path:/path-two,method:post,response:201", results[0].Value);

            Assert.True(new Http201WithoutLocationHeaderRule(openApiContract, supressionEntireRule, ruleSettings, cache).Execute()
                            .Count == 0);
        }
    }
}
