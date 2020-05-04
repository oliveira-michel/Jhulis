using Jhulis.Core;
using Jhulis.Core.Helpers.Extensions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Jhulis.Core.Rules;
using Jhulis.Core.Resources;

namespace Safra.Gsa.QaSwagger.Test.Rules
{
    public class BaseUrlRuleTest
    {
        private OpenApiDocument openApiContract;
        private Supressions supressions;
        private Supressions supressionEntireRule;
        private IOptions<RuleSettings> ruleSettings;
        private const string ruleName = "BaseUrl";
        private OpenApiDocumentCache cache; 

        public BaseUrlRuleTest()
        {
            const string contract =
                "{\n  \"swagger\" : \"2.0\",\n  \"info\" : {\n    \"version\" : \"v1\",\n    \"title\" : \"Swagger Test\",\n  },\n  \"host\" : \"/api.safra.com.br\",\n  \"basePath\" : \"/recursos-humanos\",\n  \"consumes\" : [ \"application/json\" ],\n  \"produces\" : [ \"application/json\" ],\n  \"schemes\" : [ \"https\" ],\n  \"paths\" : {\n  }\n}";
            openApiContract = new OpenApiStringReader().Read(contract, out OpenApiDiagnostic _);
            
            supressions = new Supressions(new List<Supression>());
            
            supressionEntireRule = new Supressions(new List<Supression> {new Supression {RuleName = ruleName, Target = "*"}});
            
            cache = new OpenApiDocumentCache();
        }

        [Fact]
        public void Execute()
        {
            Assert.Equal(string.Empty,
              new BaseUrlRule(openApiContract, supressions, ruleSettings, cache).Execute().First().Value);
            
            Assert.Empty(new BaseUrlRule(openApiContract, supressionEntireRule, ruleSettings, cache).Execute());
        }
    }
}
