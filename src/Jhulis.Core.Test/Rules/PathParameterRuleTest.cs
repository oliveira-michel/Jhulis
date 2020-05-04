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
    public class PathParameterRuleTest
    {
        private OpenApiDocument openApiContract;
        private Supressions supressions;
        private Supressions supressionEntireRule;
        private IOptions<RuleSettings> ruleSettings;
        private const string ruleName = "PathParameter";
        private OpenApiDocumentCache cache; 

        public PathParameterRuleTest()
        {
            const string contract =
                "{\n  \"swagger\" : \"2.0\",\n  \"info\" : {\n    \"version\" : \"v1\",\n    \"title\" : \"Swagger Test\",\n  },\n  \"consumes\" : [ \"application/json\" ],\n  \"produces\" : [ \"application/json\" ],\n  \"paths\" : {\n    \"/path-one/{idNothingToMatch}\" : {\n      \"parameters\": [\n        {\n          \"type\": \"string\",\n          \"name\": \"idNothingToMatch\",\n          \"in\": \"path\",\n          \"required\": true\n        }\n      ],\n      \"get\" : {\n        \"responses\" : {\n          \"200\" : {\n            \"description\" : \"\",\n            \"schema\" : {\n              \"type\" : \"object\",\n              \"properties\" : {\n                \"data\" : {\n                  \"type\" : \"object\",\n                  \"properties\" : {\n                    \"propertyOne\" : {\n                      \"type\" : \"string\"\n                    }\n                  }\n                }\n              }\n            }\n          }\n        }\n      }\n    },\n    \"/path-two/{idNothingToMatch}\" : {\n      \"get\" : {\n        \"parameters\": [\n          {\n            \"type\": \"string\",\n            \"name\": \"idNothingToMatch\",\n            \"in\": \"path\",\n            \"required\": true\n          }\n        ],\n        \"responses\" : {\n          \"200\" : {\n            \"description\": \"Status 200\",\n            \"schema\" : {\n              \"type\" : \"object\",\n              \"properties\" : {\n                \"data\" : {\n                  \"type\" : \"object\",\n                  \"properties\" : {\n                    \"propertyOne\" : {\n                      \"type\" : \"string\"\n                    }\n                  }\n                }\n              }\n            }\n          }\n        }\n      }\n    }\n  }\n}";
            openApiContract = new OpenApiStringReader().Read(contract, out OpenApiDiagnostic _);

            supressions = new Supressions(new List<Supression>
            {
                new Supression
                {
                    RuleName = ruleName,
                    Target = "Path='/path-one/{idNothingToMatch}',Operation='get',Parameter='idNothingToMatch'"
                }
            });

            supressionEntireRule = new Supressions(
                new List<Supression> {new Supression {RuleName = ruleName, Target = "*"}});
            
            var settings = new RuleSettings()
            {
                PathParameter = new PathParameterConfig()
                {
                    CaseType = "CamelCase",
                    Example = "idCliente",
                    PrefixToRemove = "id",
                    SufixToRemove = "",
                    Regex = "^(id[a-zA-Z]+)$",
                    MatchEntityNamePercentage = 0.6,
                    HumanReadeableFormat = "'id' + 'NomeSingularDoPath'"
                }
            };

            ruleSettings = Options.Create(settings);
            
            cache = new OpenApiDocumentCache();
        }

        [Fact]
        public void Execute()
        {
            List<ResultItem> results = new PathParameterRule(openApiContract, supressions, ruleSettings, cache).Execute();

            Assert.Equal("Path='/path-two/{idNothingToMatch}',Operation='get',Parameter='idNothingToMatch'",
                results[0].Value);

            Assert.True(new PathParameterRule(openApiContract, supressionEntireRule, ruleSettings, cache).Execute().Count == 0);
        }
    }
}
