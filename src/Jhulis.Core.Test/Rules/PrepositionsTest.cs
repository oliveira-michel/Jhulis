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
    public class PrepositionsTest
    {
        private RuleSet localizer;
        private OpenApiDocument openApiContract;
        private Supressions supressions;
        private Supressions supressionEntireRule;
        private IOptions<RuleSettings> ruleSettings;
        private const string ruleName = "Prepositions";
        private OpenApiDocumentCache cache; 

        public PrepositionsTest()
        {
            const string contract =
                "{\n  \"swagger\" : \"2.0\",\n  \"info\" : {\n    \"version\" : \"v1\",\n    \"title\" : \"Swagger Test\",\n  },\n  \"consumes\" : [ \"application/json\" ],\n  \"produces\" : [ \"application/json\" ],\n  \"paths\" : {\n    \"/meios-de-pagamento\" : {\n      \"get\" : {\n        \"responses\" : {\n          \"200\" : {\n            \"description\" : \"Irá bater na regra\",\n            \"schema\" : {\n              \"type\" : \"object\",\n              \"properties\" : {\n                \"data\" : {\n                  \"type\" : \"object\",\n                  \"properties\" : {\n                    \"nome\" : {\n                      \"type\" : \"string\"\n                    }\n                  }\n                }\n              }\n            }\n          }\n        }\n      }\n    },\n    \"/paths-two\" : {\n      \"get\" : {\n        \"responses\" : {\n          \"200\" : {\n            \"description\" : \"Irá bater na regra\",\n            \"schema\" : {\n              \"type\" : \"object\",\n              \"properties\" : {\n                \"data\" : {\n                  \"type\" : \"object\",\n                  \"properties\" : {\n                    \"campoDePreposicao\" : {\n                      \"type\" : \"string\"\n                    }\n                  }\n                }\n              }\n            }\n          }\n        }\n      }\n    },\n    \"/path-three\" : {\n      \"get\" : {\n        \"responses\" : {\n          \"200\" : {\n            \"description\" : \"Não irá bater na regra\",\n            \"schema\" : {\n              \"type\" : \"object\",\n              \"properties\" : {\n                \"data\" : {\n                  \"type\" : \"object\",\n                  \"properties\" : {\n                    \"propertyOne\" : {\n                      \"type\" : \"string\"\n                    }\n                  }\n                }\n              }\n            }\n          }\n        }\n      }\n    },\n    \"/path-fours/{idAlgoDeDepois}\" : {\n      \"get\" : {\n         parameters: [{\n            \"name\": \"idAlgoDeDepois\",\n            \"in\": \"path\",\n            \"description\": \"ID\",\n            \"required\": true,\n            \"type\": \"integer\",\n            \"format\": \"int64\",\n         }],\n        \"responses\" : {\n          \"200\" : {\n            \"description\" : \"Irá bater na regra\"\n          }\n        }\n      }\n    },\n    \"/path-fives\" : {\n      \"get\" : {\n        parameters: [{\n            \"name\": \"nomeDaEmpresa\",\n            \"in\": \"query\",\n            \"description\": \"nome\",\n            \"required\": true,\n            \"type\": \"string\"\n         }],\n        \"responses\" : {\n          \"200\" : {\n            \"description\" : \"Irá bater na regra\",\n            \"schema\" : {\n              \"type\" : \"object\",\n              \"properties\" : {\n                \"data\" : {\n                  \"type\" : \"array\",\n                  \"items\": {\n \t\t\t\t\t\"type\" : \"object\",\n                    \"properties\" : {\n                      \"propertyOne\" : {\n                    \t\"type\" : \"string\"\n                      }\n                  \t}\n                  }\n                }\n              }\n            }\n          }\n        }\n      }\n    },\n    \"/paths-six\" : {\n      \"get\" : {\n        \"responses\" : {\n          \"200\" : {\n            \"description\" : \"Irá bater na regra\",\n            \"schema\" : {\n              \"type\" : \"object\",\n              \"properties\" : {\n                \"data\" : {\n                  \"type\" : \"object\",\n                  \"properties\" : {\n                    \"cadastroAsEscuras\" : {\n                      \"type\" : \"string\"\n                    }\n                  }\n                }\n              }\n            }\n          }\n        }\n      }\n    }\n  }\n}";
            openApiContract = new OpenApiStringReader().Read(contract, out OpenApiDiagnostic _);

            var settings = new RuleSettings()
            {
                Prepositions = new PrepositionsConfig()
                {
                    WordsToAvoid = "a,à,as,às,ao,aos,ante,aonde,apos,aquilo,com,contra,da,de,desde,dessa,dessas,desse,desses,desta,destas,deste,destes,disto,do,duma,e,em,entre,na,nas,no,nos,num,numa,numas,nuns,nessa,naquilo,nessas,nesse,nesses,nesta,nestas,neste,nestes,nisso,nisto,o,para,perante,pela,pelas,pelo,pelos,por,pra,pras,sem,sob,sobre,tras,trás",
                    Example = "cartao-credito ao invés de cartao-de-credito, telefoneContato ao invés de telefoneParaContato"
                },
                NestingDepth = new NestingDepthConfig()
                { 
                    Depth = 10
                }
            };

            ruleSettings = Options.Create(settings);

            supressions = new Supressions(new List<Supression>
            {
                new Supression {RuleName = ruleName, Target = "Path='/paths-two',Operation='get'"}
            });

            supressionEntireRule = new Supressions(
              new List<Supression> {new Supression {RuleName = ruleName, Target = "*"}});
            
            cache = new OpenApiDocumentCache();
        }

        [Fact]
        public void Execute()
        {
            List<ResultItem> results = new PrepositionsRule(openApiContract, supressions, ruleSettings, cache).Execute();

            Assert.Equal(4, results.Count);

            Assert.Equal("Path='/meios-de-pagamento'", results[0].Value);
            Assert.Equal("Path='/path-fours/{idAlgoDeDepois}',Operation='get',Parameter='idAlgoDeDepois'", results[1].Value);
            Assert.Equal("Path='/path-fives',Operation='get',Parameter='nomeDaEmpresa'", results[2].Value);
            Assert.Equal("Path='/paths-six',Operation='get',ResponseCode='200',Content='application/json',PropertyFull='data.cadastroAsEscuras',Property='cadastroAsEscuras'", results[3].Value);

            Assert.True(new PrepositionsRule(openApiContract, supressionEntireRule, ruleSettings, cache).Execute().Count == 0);
        }
    }
}
