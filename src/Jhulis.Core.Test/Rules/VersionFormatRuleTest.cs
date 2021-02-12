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
    public class VersionFormatRuleTest
    {
        private OpenApiDocument openApiContract;
        private Supressions supressions;
        private Supressions supressionEntireRule;
        private IOptions<RuleSettings> ruleSettings;
        private const string ruleName = "VersionFormat";
        private OpenApiDocumentCache cache; 

        public VersionFormatRuleTest()
        {
            const string contract = "{\n  \"swagger\" : \"2.0\",\n  \"info\" : {\n    \"version\" : \"1.2.3\",\n    \"title\" : \"Swagger Test\",\n  },\n  \"consumes\" : [ \"application/json\" ],\n  \"produces\" : [ \"application/json\" ],\n  \"paths\" : {\n    \"/path-one\" : {\n      \"get\" : {\n        \"responses\" : {\n          \"200\" : {\n            \"description\" : \"\",\n            \"schema\" : {\n              \"type\" : \"object\",\n              \"properties\" : {\n                \"data\" : {\n                  \"type\" : \"object\",\n                  \"properties\" : {\n                    \"propertyOne\" : {\n                      \"type\" : \"string\"\n                    }\n                  }\n                }\n              }\n            }\n          }\n        }\n      }\n    }\n  }\n}";
            openApiContract = new OpenApiStringReader().Read(contract, out OpenApiDiagnostic _);

            supressions = new Supressions(new List<Supression>());

            supressionEntireRule = new Supressions(
                new List<Supression> {new Supression {RuleName = ruleName, Target = "*"}});
            
            var settings = new RuleSettings()
            {
                VersionFormat = new VersionFormatConfig()
                {
                    RegexExpectedFormat = "^(v\\d+)$",
                    HumanReadeableFormat = "'v' + integer version number",
                    Example = "v2"  
                }
            };

            ruleSettings = Options.Create(settings);
            
            cache = new OpenApiDocumentCache();
        }

        [Fact]
        public void Execute()
        {
            List<ResultItem> results = new VersionFormatRule(openApiContract, supressions, ruleSettings, cache).Execute();

            Assert.Equal("info.version:1.2.3", results[0].Value);

            Assert.True(new VersionFormatRule(openApiContract, supressionEntireRule, ruleSettings, cache).Execute().Count == 0);
        }
    }
}
