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
    public class PathWithCrudNamesRuleTest
    {
        private OpenApiDocument openApiContract;
        private Supressions supressions;
        private Supressions supressionEntireRule;
        private IOptions<RuleSettings> ruleSettings;
        private const string ruleName = "PathWithCrudNames";
        private OpenApiDocumentCache cache; 

        public PathWithCrudNamesRuleTest()
        {
            const string contract =
                "{\n  \"swagger\" : \"2.0\",\n  \"info\" : {\n    \"version\" : \"v1\",\n    \"title\" : \"Swagger Test\",\n  },\n  \"consumes\" : [ \"application/json\" ],\n  \"produces\" : [ \"application/json\" ],\n  \"paths\" : {\n    \"/gravar-path-one\" : {\n      \"get\" : {\n        \"responses\" : {\n          \"200\" : {\n            \"description\" : \"\",\n            \"schema\" : {\n              \"type\" : \"object\",\n              \"properties\" : {\n                \"data\" : {\n                  \"type\" : \"object\",\n                  \"properties\" : {\n                    \"propertyOne\" : {\n                      \"type\" : \"string\"\n                    }\n                  }\n                }\n              }\n            }\n          }\n        }\n      }\n    },\n    \"/apagar-path-two\" : {\n      \"get\" : {\n        \"responses\" : {\n          \"200\" : {\n            \"description\": \"Status 200\",\n            \"schema\" : {\n              \"type\" : \"object\",\n              \"properties\" : {\n                \"data\" : {\n                  \"type\" : \"object\",\n                  \"properties\" : {\n                    \"propertyOne\" : {\n                      \"type\" : \"string\"\n                    }\n                  }\n                }\n              }\n            }\n          }\n        }\n      }\n    }\n  }\n}";
            openApiContract = new OpenApiStringReader().Read(contract, out OpenApiDiagnostic _);

            supressions = new Supressions(new List<Supression>
            {
                new Supression {RuleName = ruleName, Target = "Path='/gravar-path-one'"}
            });

            supressionEntireRule = new Supressions(
                new List<Supression> {new Supression {RuleName = ruleName, Target = "*"}});
            
            var settings = new RuleSettings()
            {
                PathWithCrudNames = new PathWithCrudNamesConfig()
                {
                    WordsToAvoid = "get,consultar,recuperar,listar,ler,obter,post,salvar,gravar,enviar,postar,path,atualizar,delete,apagar,deletar,remover,excluir"
                }
            };

            ruleSettings = Options.Create(settings);
            
            cache = new OpenApiDocumentCache();
        }

        [Fact]
        public void Execute()
        {
            List<ResultItem> results = new PathWithCrudNamesRule(openApiContract, supressions, ruleSettings, cache).Execute();

            Assert.Equal("path:/apagar-path-two,path-segment:apagar-path-two", results[0].Value);

            Assert.True(new PathWithCrudNamesRule(openApiContract, supressionEntireRule, ruleSettings, cache).Execute().Count == 0);
        }
    }
}
