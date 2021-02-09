using Jhulis.Core.Helpers.Extensions;
using Jhulis.Core.Rules;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Jhulis.Core.Test.Rules
{
    public class HealthCheckRuleTest
    {
        private OpenApiDocument openApiContract;
        private Supressions supressions;
        private Supressions supressionEntireRule;
        private IOptions<RuleSettings> ruleSettings;
        private const string ruleName = "HealthCheck";
        private OpenApiDocumentCache cache;

        public HealthCheckRuleTest()
        {
            const string contract =
                "{\n  \"swagger\" : \"2.0\",\n  \"info\" : {\n    \"version\" : \"v1\",\n    \"title\" : \"Swagger Test\",\n  },\n  \"consumes\" : [ \"application/json\" ],\n  \"produces\" : [ \"application/json\" ],\n  \"paths\" : {\n    \"/gravar-path-one\" : {\n      \"get\" : {\n        \"responses\" : {\n          \"200\" : {\n            \"description\" : \"\",\n            \"schema\" : {\n              \"type\" : \"object\",\n              \"properties\" : {\n                \"data\" : {\n                  \"type\" : \"object\",\n                  \"properties\" : {\n                    \"propertyOne\" : {\n                      \"type\" : \"string\"\n                    }\n                  }\n                }\n              }\n            }\n          }\n        }\n      }\n    },\n    \"/apagar-path-two\" : {\n      \"get\" : {\n        \"responses\" : {\n          \"200\" : {\n            \"description\": \"Status 200\",\n            \"schema\" : {\n              \"type\" : \"object\",\n              \"properties\" : {\n                \"data\" : {\n                  \"type\" : \"object\",\n                  \"properties\" : {\n                    \"propertyOne\" : {\n                      \"type\" : \"string\"\n                    }\n                  }\n                }\n              }\n            }\n          }\n        }\n      }\n    }\n  }\n}";
          
            openApiContract = new OpenApiStringReader().Read(contract, out OpenApiDiagnostic _);

            supressions = new Supressions(new List<Supression>());

            supressionEntireRule = new Supressions(
                new List<Supression> { new Supression { RuleName = ruleName, Target = "*" } });

            var settings = new RuleSettings()
            {
                HealthCheck = new HealthCheckConfig()
                {
                    Regex = "^(health)$"
                }
            };

            ruleSettings = Options.Create(settings);

            cache = new OpenApiDocumentCache();
        }

        [Fact]
        public void Execute()
        {
            List<ResultItem> results = new HealthCheckRule(openApiContract, supressions, ruleSettings, cache).Execute();

            Assert.Equal("Adicione um Health Check na sua API.", results[0].Description);

            Assert.True(new HealthCheckRule(openApiContract, supressionEntireRule, ruleSettings, cache).Execute().Count == 0);
        }

        [Theory]
        [InlineData("abc-health")]
        [InlineData("health-abc")]
        [InlineData("healthabc")]
        [InlineData("abchealthasdfdf")]       
        public void ExecuteContractWithAnotherFormatWordHealth(string healthPath)
        {
            var contractWithHealthCheck =
                          @"openapi: 3.0.0
info:
  title: heatlh
  version: '1.0'
servers:
  - url: 'http://localhost:3000'
paths:
  /"+$"{healthPath}"+@": 
    get:
      summary: Your GET endpoint
      tags: []
      responses:
        '200':
          description: OK
      operationId: get-health-check
  /clientes:
    get:
      summary: Your GET endpoint
      tags: []
      responses:
        '200':
          description: OK
      operationId: get-clientes
components:
  schemas: {}
";
            var openApiContractWithHealthCheck = new OpenApiStringReader().Read(contractWithHealthCheck, out OpenApiDiagnostic _);

            List<ResultItem> results = new HealthCheckRule(openApiContractWithHealthCheck, supressions, ruleSettings, cache).Execute();

            Assert.Equal("Adicione um Health Check na sua API.", results[0].Description);
        }

        [Fact]       
        public void ExecuteContractWithHealthCheck()
        {
            var contractWithHealthCheck =
            "{\n  \"swagger\" : \"2.0\",\n  \"info\" : {\n    \"version\" : \"v1\",\n    \"title\" : \"Swagger Test\",\n  },\n  \"consumes\" : [ \"application/json\" ],\n  \"produces\" : [ \"application/json\" ],\n  \"paths\" : {\n    \"/health\" : {\n      \"get\" : {\n        \"responses\" : {\n          \"200\" : {\n            \"description\" : \"\",\n            \"schema\" : {\n              \"type\" : \"object\",\n              \"properties\" : {\n                \"data\" : {\n                  \"type\" : \"object\",\n                  \"properties\" : {\n                    \"propertyOne\" : {\n                      \"type\" : \"string\"\n                    }\n                  }\n                }\n              }\n            }\n          }\n        }\n      }\n    },\n    \"/apagar-path-two\" : {\n      \"get\" : {\n        \"responses\" : {\n          \"200\" : {\n            \"description\": \"Status 200\",\n            \"schema\" : {\n              \"type\" : \"object\",\n              \"properties\" : {\n                \"data\" : {\n                  \"type\" : \"object\",\n                  \"properties\" : {\n                    \"propertyOne\" : {\n                      \"type\" : \"string\"\n                    }\n                  }\n                }\n              }\n            }\n          }\n        }\n      }\n    }\n  }\n}";

            var openApiContractWithHealthCheck = new OpenApiStringReader().Read(contractWithHealthCheck, out OpenApiDiagnostic _);

            List<ResultItem> results = new HealthCheckRule(openApiContractWithHealthCheck, supressions, ruleSettings, cache).Execute();

            Assert.Empty(results);
        }
    }
}
