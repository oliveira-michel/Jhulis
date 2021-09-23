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
            const string contract = "{\r\n  \"swagger\" : \"2.0\",\r\n  \"info\" : {\r\n    \"title\": \"teste 2\",\r\n    \"version\": \"1.0\"\r\n  },\r\n  \"consumes\" : [ \"application/json\" ],\r\n  \"produces\" : [ \"application/json\" ],\r\n  \"paths\" : {\r\n    \"/path-one\" : {\r\n      \"get\" : {\r\n        \"responses\" : {\r\n          \"200\" : {\r\n            \"description\" : \"Status 200\",\r\n            \"schema\" : {\r\n              \"type\" : \"object\",\r\n              \"properties\" : {\r\n                \"one\" : {\r\n                  \"type\" : \"string\",\r\n                  \"example\": \"R$ 100.000,11\"\r\n                },\r\n                \"two\" : {\r\n                  \"type\" : \"string\",\r\n                  \"example\": \"020.22\"\r\n                },\r\n                \"three\" : {\r\n                  \"type\" : \"string\",\r\n                  \"example\": \"030,30\"\r\n                },\r\n                \"four\" : {\r\n                  \"type\" : \"string\",\r\n                  \"example\": \" 44 \"\r\n                },\r\n                \"five\" : {\r\n                  \"type\" : \"number\",\r\n                  \"example\": .5\r\n                },\r\n                \"anIp\" : {\r\n                  \"type\" : \"string\",\r\n                  \"example\": \"192.168.0.110\"\r\n                },\r\n                \"clientCpf\" : {\r\n                  \"type\" : \"string\",\r\n                  \"example\": \"33344455566\"\r\n                },\r\n                \"cnpjRaiz\" : {\r\n                  \"type\" : \"string\",\r\n                  \"example\": \"12345678\"\r\n                }\r\n              }\r\n            }\r\n          }\r\n        }\r\n      }\r\n    },\r\n    \"/path-two\" : {\r\n      \"get\" : {\r\n        \"responses\" : {\r\n          \"200\" : {\r\n            \"description\" : \"Status 200\",\r\n            \"schema\" : {\r\n              \"type\" : \"object\",\r\n              \"properties\" : {\r\n                \"data\" : {\r\n                  \"type\" : \"object\",\r\n                  \"properties\" : {\r\n                    \"six\" : {\r\n                      \"type\" : \"string\",\r\n                      \"example\": \"6\"\r\n                    },\r\n                    \"seven\" : {\r\n                      \"type\" : \"string\",\r\n                      \"example\": \"A77\"\r\n                    },\r\n                    \"anIp\" : {\r\n                      \"type\" : \"string\",\r\n                      \"example\": \"192.168.0.110\"\r\n                    }\r\n                  }\r\n                }\r\n              }\r\n            }\r\n          }\r\n        }\r\n      }\r\n    }\r\n  }\r\n}";
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
                new List<Supression> { new Supression { RuleName = ruleName, Target = "*" } });

            var settings = new RuleSettings()
            {
                StringCouldBeNumber = new StringCouldBeNumberConfig()
                {
                    CurrencySymbols = "$,.?,?,?.,?.?.,??,???,???.,¢,£,¥,€,AMD,Ar,AUD,B/.,BND,Br,Bs.,Bs.,Bs.S.,C$,CUC,D,Db,din.,Esc,ƒ,FOK,Fr,Fr.,Ft,G,GBP,GGP,INR,JOD,K,Kc,KM,kn,kr,Ks,Kz,L,Le,lei,m,MAD,MK,MRU,MT,Nfk,Nu.,NZD,P,PND,Q,R,R$,RM,Rp,Rs,RUB,S/.,SGD,Sh,Sl,so'm,T,T$,UM,USD,Vt,ZAR,ZK,zl",
                    ExceptionsRegex = @"(?i)(\W|^|\-)[\w.\-]{0,100}(cpf|cnpj)[\w.\-]{0,100}(\W|$|\-)"
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

            Assert.Equal(@"path:/path-one,method:get,response:200,content:application/json,response.property:one",
                results[0].Value);
            Assert.Equal(@"path:/path-one,method:get,response:200,content:application/json,response.property:two",
                results[1].Value);
            Assert.Equal(@"path:/path-one,method:get,response:200,content:application/json,response.property:three",
                results[2].Value);
            Assert.Equal(@"path:/path-one,method:get,response:200,content:application/json,response.property:four",
                results[3].Value);

            Assert.True(new StringCouldBeNumberRule(openApiContract, supressionEntireRule, ruleSettings, cache).Execute().Count == 0);
        }
    }
}
