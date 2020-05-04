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
    public class PropertyStartingWithTypeRuleTest
    {
        private OpenApiDocument openApiContract;
        private Supressions supressions;
        private Supressions supressionEntireRule;
        private IOptions<RuleSettings> ruleSettings;
        private const string ruleName = "PropertyStartingWithType";
        private OpenApiDocumentCache cache; 
        public PropertyStartingWithTypeRuleTest()
        {
            const string contract =
                "{\n  \"swagger\" : \"2.0\",\n  \"info\" : {\n    \"version\" : \"v1\",\n    \"title\" : \"Swagger Test\",\n  },\n  \"consumes\" : [ \"application/json\" ],\n  \"produces\" : [ \"application/json\" ],\n  \"paths\" : {\n    \"/path-one\" : {\n      \"get\" : {\n        \"responses\" : {\n          \"200\" : {\n            \"description\": \"\",\n            \"schema\" : {\n              \"type\" : \"object\",\n              \"properties\" : {\n                \"data\" : {\n                  \"type\" : \"object\",\n                  \"properties\" : {\n                    \"dblPropertyOne\" : {\n                      \"type\" : \"string\"\n                    }\n                  }\n                }\n              }\n            }\n          }\n        }\n      }\n    },\n    \"/path-two\" : {\n      \"get\" : {\n        \"parameters\" : [{\n            \"in\" : \"query\",\n            \"name\" : \"parameterOne\",\n            \"type\" : \"string\",\n            \"description\" : \"Too short, but ok.\",\n            \"required\" : true\n        }]\n        ,\n        \"responses\" : {\n          \"200\" : {\n            \"description\": \"\",\n            \"schema\" : {\n              \"type\" : \"object\",\n              \"properties\" : {\n                \"data\" : {\n                  \"type\" : \"object\",\n                  \"properties\" : {\n                    \"intPropertyOne\" : {\n                      \"type\" : \"number\"\n                    },\n                    \"flagUpdated\" : {\n                      \"type\" : \"number\"\n                    }\n                  }\n                }\n              }\n            }\n          }\n        }\n      }\n    }\n  }\n}";
            openApiContract = new OpenApiStringReader().Read(contract, out OpenApiDiagnostic _);

            supressions = new Supressions(new List<Supression>
            {
                new Supression
                {
                    RuleName = ruleName,
                    Target = "Path='/path-one',Operation='get',ResponseCode='200',PropertyFull='data.dblPropertyOne'"
                }
            });

            supressionEntireRule = new Supressions(
                new List<Supression> {new Supression {RuleName = ruleName, Target = "*"}});
            
            var settings = new RuleSettings()
            {
                PropertyStartingWithType = new PropertyStartingWithTypeConfig()
                {
                    WordsToAvoid = "bool,byte,char,dbl,decimal,double,flag,float,indicador,int,integer,long,obj,sbyte,str,string,uint,ulong,short,ushort"
                },
                NestingDepth = new NestingDepthConfig()
                {
                    Depth = 20    
                }
            };

            ruleSettings = Options.Create(settings);
            
            cache = new OpenApiDocumentCache();
        }

        [Fact]
        public void Execute()
        {
            List<ResultItem> results =
                new PropertyStartingWithTypeRule(openApiContract, supressions, ruleSettings, cache).Execute();

            Assert.Equal(
                "Path='/path-two',Operation='get',ResponseCode='200',Content='application/json',PropertyFull='data.intPropertyOne',Property='intPropertyOne'",
                results[0].Value);
            
            Assert.Equal(
                "Path='/path-two',Operation='get',ResponseCode='200',Content='application/json',PropertyFull='data.flagUpdated',Property='flagUpdated'",
                results[1].Value);

            Assert.True(new PropertyStartingWithTypeRule(openApiContract, supressionEntireRule, ruleSettings, cache).Execute()
                            .Count == 0);
        }
    }
}
