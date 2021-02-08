using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Jhulis.Core;
using Jhulis.Core.Exceptions;
using Jhulis.Core.Helpers.Extensions;
using Jhulis.Rest.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Jhulis.Adapters;

namespace Jhulis.Rest.Controllers
{
    [ApiController]
    public class ContractController : ControllerBase
    {
        private readonly IOptions<RuleSettings> ruleSettings;

        public ContractController(IOptions<RuleSettings> ruleSettings)
        {
            this.ruleSettings = ruleSettings;
        }

        /// <summary>
        /// Validate an Open API Specification contract.
        /// </summary>
        /// <param name="request">An contract and (optional) supressions to avoid some rules.</param>
        /// <returns>List of results with the unmet rules and values who broken them.</returns>
        /// <response code="200">Return the list of results</response>
        /// <response code="400">If the request is in invalid format or contract isn't and valid Open API Specification.</response>   
        [HttpPost("full-validate")]
        [Consumes("application/json")]
        [Produces("application/json", "text/plain")]
        public ActionResult<ValidarGetResponseModel> Validate([FromBody] ValidarGetRequestModel request)
        {
            if (request == null || request.Content == null)
            {
                //TODO Criar um lançamento de exception 400 como o de baixo
            }

            Result result;

            try
            {
                result = new Processor(ruleSettings).Validate(request.Content, request.Supressions.ToSupressions());
            }
            catch (InvalidOpenApiDocumentException e)
            {
                //TODO Criar este lançamento de exception
                //ValidationException.Throw("rest-400");

                //TODO precisa evoluir o "error-code" para poder passar parâmetros {0}
                //Isto é tático
                Response.StatusCode = StatusCodes.Status400BadRequest;
                result = new Result
                {
                    ResultItens = new List<ResultItem>
                    {
                        new ResultItem
                        {
                            Description = "O contrato não é um Open API Specification válido.",
                            Details = e.Message,
                            Rule = "GenericError",
                            Severity = Severity.Error
                        }
                    }
                };
            }

            if (HttpContext.Request.Headers.ContainsKey("Accept")
                && HttpContext.Request.Headers["Accept"] == "text/plain")
                return Content(result.ToText());
            else
                return result.ToValidarResponseGetModel();
        }

        [HttpPost("validate")]
        [Consumes("text/plain")]
        [Produces("application/json", "text/plain")]
        public async Task<ActionResult<ValidarGetResponseModel>> PreValidate()
        {
            Result result;

            try
            {
                using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    string contract = await reader.ReadToEndAsync().ConfigureAwait(false);

                    result = new Processor(ruleSettings).Validate(contract);
                }
            }
            catch (InvalidOpenApiDocumentException e)
            {
                //TODO precisa evoluir o "error-code" para poder passar parâmetros {0}
                //Isto é tático
                Response.StatusCode = StatusCodes.Status400BadRequest;
                result = new Result
                {
                    ResultItens = new List<ResultItem>
                    {
                        new ResultItem
                        {
                            Description = "O contrato não é um Open API Specification válido.",
                            Details = e.Message,
                            Rule = "GenericError",
                            Severity = Severity.Error
                        }
                    }
                };
            }

            if (HttpContext.Request.Headers.ContainsKey("Accept")
                && HttpContext.Request.Headers["Accept"] == "text/plain")
                return Content(result.ToText());
            else
                return result.ToValidarResponseGetModel();
        }

        [HttpPost("escape")]
        [Consumes("text/plain")]
        [Produces("text/plain")]
        public async Task<ActionResult<string>> Escape()
        {
            string result;
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                string contract = await reader.ReadToEndAsync().ConfigureAwait(false);

                result = HttpUtility.JavaScriptStringEncode(contract);
            }
            return Content(result);
        }
    }
}