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
    public class DoubleSlashesRuleTest
    {
        private OpenApiDocument openApiContract;
        private Supressions supressions;
        private Supressions supressionEntireRule;
        private IOptions<RuleSettings> ruleSettings;
        private const string ruleName = "DoubleSlashes";
        private OpenApiDocumentCache cache; 

        public DoubleSlashesRuleTest()
        {
            const string contract =
                "{\n  \"swagger\" : \"2.0\",\n  \"info\" : {\n    \"version\" : \"v1\",\n    \"title\" : \"Test API\",\n  },\n  \"host\" : \"api.test.com\",\n  \"basePath\" : \"//base-path\",\n  \"consumes\" : [ \"application/json\" ],\n  \"produces\" : [ \"application/json\" ],\n  \"schemes\" : [ \"https\" ],\n  \"paths\" : {\n    \"//path-one\" : {\n      \"get\" : {\n        \"responses\" : {\n          \"200\" : {\n            \"schema\" : {\n              \"type\" : \"object\",\n              \"properties\" : {\n                \"data\" : {\n                  \"type\" : \"object\",\n                  \"properties\" : {\n                    \"propertyOne\" : {\n                      \"type\" : \"string\"\n                    }\n                  }\n                }\n              }\n            }\n          }\n        }\n      }\n    },\n    \"//path-two\" : {\n      \"get\" : {\n        \"parameters\" : [{\n            \"in\" : \"query\",\n            \"name\" : \"parameterOne\",\n            \"type\" : \"string\",\n            \"description\" : \"too short\",\n            \"required\" : true\n        }]\n        ,\n        \"responses\" : {\n          \"200\" : {\n            \"description\": \"Status 200\",\n            \"schema\" : {\n              \"type\" : \"object\",\n              \"properties\" : {\n                \"data\" : {\n                  \"type\" : \"object\",\n                  \"properties\" : {\n                    \"propertyOne\" : {\n                      \"type\" : \"string\"\n                    }\n                  }\n                }\n              }\n            }\n          }\n        }\n      }\n    }\n  }\n}";
            openApiContract = new OpenApiStringReader().Read(contract, out OpenApiDiagnostic _);

            supressions = new Supressions(new List<Supression>
            {
                new Supression {RuleName = ruleName, Target = "Path='//path-one'"}}
            );

            supressionEntireRule = new Supressions(
                new List<Supression>{new Supression {RuleName = ruleName, Target = "*"}});
            
            cache = new OpenApiDocumentCache();
        }

        [Fact]
        public void Execute()
        {
            List<ResultItem> results = new DoubleSlashesRule(openApiContract, supressions, ruleSettings, cache).Execute();

            Assert.Equal("Server.Url='https://api.test.com//base-path'", results[0].Value);

            Assert.Equal("Path='//path-two'", results[1].Value);

            Assert.True(new DoubleSlashesRule(openApiContract, supressionEntireRule, ruleSettings, cache).Execute().Count == 0);
        }
    }
}
