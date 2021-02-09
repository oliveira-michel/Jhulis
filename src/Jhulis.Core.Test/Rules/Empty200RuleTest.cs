using System.Collections.Generic;
using System.Linq;
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
    public class Empty200RuleTest
    {
        private OpenApiDocument openApiContract;
        private Supressions supressions;
        private Supressions supressionEntireRule;
        private IOptions<RuleSettings> ruleSettings;
        private const string ruleName = "Empty200";
        private OpenApiDocumentCache cache; 
        public Empty200RuleTest()
        {
            const string contract =
                "{\n  \"swagger\" : \"2.0\",\n  \"info\" : {\n    \"version\" : \"v1\",\n    \"title\" : \"Swagger Test\",\n  },\n  \"consumes\" : [ \"application/json\" ],\n  \"produces\" : [ \"application/json\" ],\n  \"paths\" : {\n    \"/path-one\" : {\n      \"get\" : {\n        \"responses\" : {\n          \"200\" : {\n            \"schema\" : {\n             \n            }\n          }\n        }\n      }\n    },\n    \"/path-two\" : {\n      \"get\" : {\n        \"parameters\" : [{\n            \"in\" : \"query\",\n            \"name\" : \"parameterOne\",\n            \"type\" : \"string\",\n            \"description\" : \"too short\",\n            \"required\" : true\n        }]\n        ,\n        \"responses\" : {\n          \"200\" : {\n            \"description\": \"Status 200\",\n            \"schema\" : {\n              \n            }\n          }\n        }\n      }\n    }\n  }\n}";
            openApiContract = new OpenApiStringReader().Read(contract, out OpenApiDiagnostic _);

            supressions = new Supressions(new List<Supression>
            {
                new Supression {RuleName = ruleName, Target = "Path='/path-two',Operation='get'"}
            });

            supressionEntireRule = new Supressions(
                new List<Supression> {new Supression {RuleName = ruleName, Target = "*"}});
            
            cache = new OpenApiDocumentCache();
        }

        [Fact]
        public void Execute()
        {
            List<ResultItem> results = new Empty200Rule(openApiContract, supressions, ruleSettings, cache).Execute();

            Assert.Equal(
                "Path='/path-one',Operation='get'",
                results[0].Value);
            
            Assert.Equal(results.Count(),1);

            Assert.True(new Empty200Rule(openApiContract, supressionEntireRule, ruleSettings, cache).Execute().Count == 0);
        }
    }
}
