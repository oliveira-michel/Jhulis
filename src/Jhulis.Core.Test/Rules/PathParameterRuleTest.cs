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
                "{\r\n  \"swagger\" : \"2.0\",\r\n  \"info\" : {\r\n    \"version\" : \"v1\",\r\n    \"title\" : \"Swagger Test\",\r\n  },\r\n  \"consumes\" : [ \"application/json\" ],\r\n  \"produces\" : [ \"application/json\" ],\r\n  \"paths\" : {\r\n    \"/path-one/{idNothingToMatch}\" : {\r\n      \"parameters\": [\r\n        {\r\n          \"type\": \"string\",\r\n          \"name\": \"idNothingToMatch\",\r\n          \"in\": \"path\",\r\n          \"required\": true\r\n        }\r\n      ],\r\n      \"get\" : {\r\n        \"responses\" : {\r\n          \"200\" : {\r\n            \"description\" : \"\",\r\n            \"schema\" : {\r\n              \"type\" : \"object\",\r\n              \"properties\" : {\r\n                \"data\" : {\r\n                  \"type\" : \"object\",\r\n                  \"properties\" : {\r\n                    \"propertyOne\" : {\r\n                      \"type\" : \"string\"\r\n                    }\r\n                  }\r\n                }\r\n              }\r\n            }\r\n          }\r\n        }\r\n      }\r\n    },\r\n    \"/path-two/{idNothingToMatch}\" : {\r\n      \"get\" : {\r\n        \"parameters\": [\r\n          {\r\n            \"type\": \"string\",\r\n            \"name\": \"idNothingToMatch\",\r\n            \"in\": \"path\",\r\n            \"required\": true\r\n          }\r\n        ],\r\n        \"responses\" : {\r\n          \"200\" : {\r\n            \"description\": \"Status 200\",\r\n            \"schema\" : {\r\n              \"type\" : \"object\",\r\n              \"properties\" : {\r\n                \"data\" : {\r\n                  \"type\" : \"object\",\r\n                  \"properties\" : {\r\n                    \"propertyOne\" : {\r\n                      \"type\" : \"string\"\r\n                    }\r\n                  }\r\n                }\r\n              }\r\n            }\r\n          }\r\n        }\r\n      }\r\n    },\r\n    \"/all-to-match/{idAllToMatch}\" : {\r\n      \"get\" : {\r\n        \"parameters\": [\r\n          {\r\n            \"type\": \"string\",\r\n            \"name\": \"idAllToMatch\",\r\n            \"in\": \"path\",\r\n            \"required\": true\r\n          }\r\n        ],\r\n        \"responses\" : {\r\n          \"200\" : {\r\n            \"description\": \"Status 200\",\r\n            \"schema\" : {\r\n              \"type\" : \"object\",\r\n              \"properties\" : {\r\n                \"data\" : {\r\n                  \"type\" : \"object\",\r\n                  \"properties\" : {\r\n                    \"propertyOne\" : {\r\n                      \"type\" : \"string\"\r\n                    }\r\n                  }\r\n                }\r\n              }\r\n            }\r\n          }\r\n        }\r\n      }\r\n    }\r\n  }\r\n}";
            openApiContract = new OpenApiStringReader().Read(contract, out OpenApiDiagnostic _);

            supressions = new Supressions(new List<Supression>
            {
                new Supression
                {
                    RuleName = ruleName,
                    Target = "Path='/path-one/{idNothingToMatch}',Parameter='idNothingToMatch'"
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

            Assert.Equal("path:/path-two/{idNothingToMatch},path-parameter:idNothingToMatch",
                results[0].Value);

            Assert.Equal(1, results.Count);

            Assert.True(new PathParameterRule(openApiContract, supressionEntireRule, ruleSettings, cache).Execute().Count == 0);
        }
    }
}
