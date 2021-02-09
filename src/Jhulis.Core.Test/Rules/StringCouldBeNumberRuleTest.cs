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
    public class StringCouldBeNumberRuleTest
    {
        private OpenApiDocument openApiContract;
        private Supressions supressions;
        private Supressions supressionEntireRule;
        private IOptions<RuleSettings> ruleSettings;
        private const string ruleName = "StringCouldBeNumber";
        private OpenApiDocumentCache cache; 

        public StringCouldBeNumberRuleTest()
        {
            const string contract = "{\n  \"swagger\" : \"2.0\",\n  \"info\" : {\n    \"title\": \"teste 2\",\n    \"version\": \"1.0\"\n  },\n  \"consumes\" : [ \"application/json\" ],\n  \"produces\" : [ \"application/json\" ],\n  \"paths\" : {\n    \"/path-one\" : {\n      \"get\" : {\n        \"responses\" : {\n          \"200\" : {\n            \"description\" : \"Status 200\",\n            \"schema\" : {\n              \"type\" : \"object\",\n              \"properties\" : {\n                \"one\" : {\n                  \"type\" : \"string\",\n                  \"example\": \"R$ 100.000,11\"\n                },\n                \"two\" : {\n                  \"type\" : \"string\",\n                  \"example\": \"020.22\"\n                },\n                \"three\" : {\n                  \"type\" : \"string\",\n                  \"example\": \"030,30\"\n                },\n                \"four\" : {\n                  \"type\" : \"string\",\n                  \"example\": \" 44 \"\n                },\n                \"five\" : {\n                  \"type\" : \"number\",\n                  \"example\": .5\n                }\n              }\n            }\n          }\n        }\n      }\n    },\n    \"/path-two\" : {\n      \"get\" : {\n        \"responses\" : {\n          \"200\" : {\n            \"description\" : \"Status 200\",\n            \"schema\" : {\n              \"type\" : \"object\",\n              \"properties\" : {\n                \"data\" : {\n                  \"type\" : \"object\",\n                  \"properties\" : {\n                    \"six\" : {\n                      \"type\" : \"string\",\n                      \"example\": \"6\"\n                    },\n                    \"seven\" : {\n                      \"type\" : \"string\",\n                      \"example\": \"A77\"\n                    },\n                    \"anIp\" : {\n                      \"type\" : \"string\",\n                      \"example\": \"192.168.0.110\"\n                    }\n                  }\n                }\n              }\n            }\n          }\n        }\n      }\n    }\n  }\n}";
            openApiContract = new OpenApiStringReader().Read(contract, out OpenApiDiagnostic _);

            supressions = new Supressions(new List<Supression>
            {
                new Supression
                {
                    RuleName = ruleName,
                    Target = "Path='/path-two',Operation='get',ResponseCode='200',Parameter='data.six'"
                }
            });

            supressionEntireRule = new Supressions(
                new List<Supression> {new Supression {RuleName = ruleName, Target = "*"}});
            
            var settings = new RuleSettings()
            {
                StringCouldBeNumber = new StringCouldBeNumberConfig()
                {
                    CurrencySymbols = "$,.?,?,?.,?.?.,??,???,???.,¢,£,¥,€,AMD,Ar,AUD,B/.,BND,Br,Bs.,Bs.,Bs.S.,C$,CUC,D,Db,din.,Esc,ƒ,FOK,Fr,Fr.,Ft,G,GBP,GGP,INR,JOD,K,Kc,KM,kn,kr,Ks,Kz,L,Le,lei,m,MAD,MK,MRU,MT,Nfk,Nu.,NZD,P,PND,Q,R,R$,RM,Rp,Rs,RUB,S/.,SGD,Sh,Sl,so'm,T,T$,UM,USD,Vt,ZAR,ZK,zl"    
                },
                NestingDepth = new NestingDepthConfig()
                {
                    Depth = 5
                }
            };

            ruleSettings = Options.Create(settings);
            
            cache = new OpenApiDocumentCache();
        }

        [Fact]
        public void Execute()
        {
            List<ResultItem> results = new StringCouldBeNumberRule(openApiContract, supressions, ruleSettings, cache).Execute();

            Assert.Equal(4, results.Count);
            
            Assert.Equal(@"Path='/path-one',Operation='get',Parameter='one'",
                results[0].Value);
            Assert.Equal(@"Path='/path-one',Operation='get',Parameter='two'",
                results[1].Value);
            Assert.Equal(@"Path='/path-one',Operation='get',Parameter='three'",
                results[2].Value);
            Assert.Equal(@"Path='/path-one',Operation='get',Parameter='four'",
                results[3].Value);

            Assert.True(new StringCouldBeNumberRule(openApiContract, supressionEntireRule, ruleSettings, cache).Execute().Count == 0);
        }
    }
}
