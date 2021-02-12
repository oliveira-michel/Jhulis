using Jhulis.Core.Rules;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Jhulis.Core.Helpers.Extensions;
using System.Collections.Generic;
using Jhulis.Core.Exceptions;
using Microsoft.OpenApi.Readers.Exceptions;
using System;

namespace Jhulis.Core
{
    public class Processor
    {
        private readonly IOptions<RuleSettings> ruleSettings;
        private OpenApiDocumentCache cache = new OpenApiDocumentCache();

        public Processor(IOptions<RuleSettings> ruleSettings)
        {
            this.ruleSettings = ruleSettings;
        }

        public Result Validate(string oasContent, List<Supression> supressionList = null)
        {
            var analisysResult = new Result();

            OpenApiDocument openApiContract;
            try
            {
                openApiContract = new OpenApiStringReader().Read(oasContent, out OpenApiDiagnostic diagnostic);

                if (diagnostic.Errors.Count > 0)
                    throw new InvalidOpenApiDocumentException(diagnostic.Errors);
            }
            catch (InvalidOpenApiDocumentException e)
            {
                analisysResult.ResultItens.Add(
                    new ResultItem
                    {
                        Description = "O contrato não é um Open API Specification válido.",
                        Details = e.Message,
                        Rule = "GenericError",
                        Severity = Severity.Error
                    }
                );
                return analisysResult;
            }
            catch (OpenApiUnsupportedSpecVersionException e)
            {
                analisysResult.ResultItens.Add(
                   new ResultItem
                    {
                        Description = "Esta versão do Open API Specification não é suportada.",
                        Details = e.Message,
                        Rule = "GenericError",
                        Severity = Severity.Error
                    }
                );
                return analisysResult;
            }
            catch (Exception e)
            {
                analisysResult.ResultItens.Add(
                   new ResultItem
                   {
                       Description = "Algum problema no contrato impediu o Jhulis de seguir com a análise.",
                       Details = e.Message,
                       Rule = "GenericError",
                       Severity = Severity.Error
                   }
                );
                return analisysResult;
            }

            var supressions = new Supressions(supressionList);

            //TODO revisar o modelo de "percorrer em loop desde o path", pois é possível validar as definitions na raiz do contrato e não revalidá-las toda as vezes path por path.

            //Error
            analisysResult.ResultItens.TryAddEmptiableRange(new VersionFormatRule(openApiContract, supressions, ruleSettings, cache).Execute());
            analisysResult.ResultItens.TryAddEmptiableRange(new BaseUrlRule(openApiContract, supressions, ruleSettings, cache).Execute());
            analisysResult.ResultItens.TryAddEmptiableRange(new PathTrailingSlashRule(openApiContract, supressions, ruleSettings, cache).Execute());
            analisysResult.ResultItens.TryAddEmptiableRange(new DoubleSlashesRule(openApiContract, supressions, ruleSettings, cache).Execute());
            analisysResult.ResultItens.TryAddEmptiableRange(new ContentEnvelopeRule(openApiContract, supressions, ruleSettings, cache).Execute());

            //Warnings
            analisysResult.ResultItens.TryAddEmptiableRange(new NestingDepthRule(openApiContract, supressions, ruleSettings, cache).Execute());
            analisysResult.ResultItens.TryAddEmptiableRange(new PathCaseRule(openApiContract, supressions, ruleSettings, cache).Execute());
            analisysResult.ResultItens.TryAddEmptiableRange(new PathPluralRule(openApiContract, supressions, ruleSettings, cache).Execute());
            analisysResult.ResultItens.TryAddEmptiableRange(new PathParameterRule(openApiContract, supressions, ruleSettings, cache).Execute());
            analisysResult.ResultItens.TryAddEmptiableRange(new PathAndIdStructureRule(openApiContract, supressions, ruleSettings, cache).Execute());
            analisysResult.ResultItens.TryAddEmptiableRange(new PathWithCrudNamesRule(openApiContract, supressions, ruleSettings, cache).Execute());
            analisysResult.ResultItens.TryAddEmptiableRange(new IdPropertyResponseRule(openApiContract, supressions, ruleSettings, cache).Execute());
            analisysResult.ResultItens.TryAddEmptiableRange(new PropertyCaseRule(openApiContract, supressions, ruleSettings, cache).Execute());
            analisysResult.ResultItens.TryAddEmptiableRange(new OperationSuccessResponseRule(openApiContract, supressions, ruleSettings, cache).Execute());
            analisysResult.ResultItens.TryAddEmptiableRange(new Empty200Rule(openApiContract, supressions, ruleSettings, cache).Execute());
            analisysResult.ResultItens.TryAddEmptiableRange(new ContentIn204Rule(openApiContract, supressions, ruleSettings, cache).Execute());
            analisysResult.ResultItens.TryAddEmptiableRange(new Http3xxWithoutLocationHeaderRule(openApiContract, supressions, ruleSettings, cache).Execute());
            analisysResult.ResultItens.TryAddEmptiableRange(new PaginationEnvelopeFormatRule(openApiContract, supressions, ruleSettings, cache).Execute());
            analisysResult.ResultItens.TryAddEmptiableRange(new MessagesEnvelopeFormatRule(openApiContract, supressions, ruleSettings, cache).Execute());
            analisysResult.ResultItens.TryAddEmptiableRange(new ErrorResponseFormatRule(openApiContract, supressions, ruleSettings, cache).Execute());
            analisysResult.ResultItens.TryAddEmptiableRange(new ValidResponseCodesRule(openApiContract, supressions, ruleSettings, cache).Execute());
            analisysResult.ResultItens.TryAddEmptiableRange(new Http201WithoutLocationHeaderRule(openApiContract, supressions, ruleSettings, cache).Execute());
            analisysResult.ResultItens.TryAddEmptiableRange(new ResponseWithout4xxAnd500Rule(openApiContract, supressions, ruleSettings, cache).Execute());
            analisysResult.ResultItens.TryAddEmptiableRange(new ArrayOnNoResourceIdEndpointRule(openApiContract, supressions, ruleSettings, cache).Execute());

            //Information
            analisysResult.ResultItens.TryAddEmptiableRange(new InfoContactRule(openApiContract, supressions, ruleSettings, cache).Execute());
            analisysResult.ResultItens.TryAddEmptiableRange(new DescriptionRule(openApiContract, supressions, ruleSettings, cache).Execute());
            analisysResult.ResultItens.TryAddEmptiableRange(new DescriptionQualityRule(openApiContract, supressions, ruleSettings, cache).Execute());
            analisysResult.ResultItens.TryAddEmptiableRange(new EmptyExamplesRule(openApiContract, supressions, ruleSettings, cache).Execute());
            //TODO Checar se os exemplos são válidos em relação aos schemas. Verificar também o tipo de dado deles. Inspiração: EmptyExamplesRule > Newtonsoft.Json. Lembrete: se vazio, ignora.

            //Hints
            analisysResult.ResultItens.TryAddEmptiableRange(new PropertyStartingWithTypeRule(openApiContract, supressions, ruleSettings, cache).Execute());
            analisysResult.ResultItens.TryAddEmptiableRange(new PrepositionsRule(openApiContract, supressions, ruleSettings, cache).Execute());
            analisysResult.ResultItens.TryAddEmptiableRange(new PropertyNamingMatchingPathRule(openApiContract, supressions, ruleSettings, cache).Execute());
            analisysResult.ResultItens.TryAddEmptiableRange(new StringCouldBeNumberRule(openApiContract, supressions, ruleSettings, cache).Execute());
            analisysResult.ResultItens.TryAddEmptiableRange(new DateWithoutFormatRule(openApiContract, supressions, ruleSettings, cache).Execute());
            analisysResult.ResultItens.TryAddEmptiableRange(new Http200WithoutPaginationRule(openApiContract, supressions, ruleSettings, cache).Execute());
            analisysResult.ResultItens.TryAddEmptiableRange(new HealthCheckRule(openApiContract, supressions, ruleSettings, cache).Execute());

            return analisysResult;
        }
    }
}
