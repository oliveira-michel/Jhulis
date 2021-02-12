using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.OpenApi.Models;

namespace Jhulis.Core.Exceptions
{
    public class InvalidOpenApiDocumentException : Exception
    {
        public InvalidOpenApiDocumentException(IList<OpenApiError> errors) : base(FormattedError(errors))
        {
        }

        private static string FormattedError(IList<OpenApiError> errors)
        {
            var sb = new StringBuilder();
            sb.Append("An error has ocurred during the Open API document parsing. Please, check if the document structure is valid. ");
            sb.AppendLine("Details:");

            foreach (OpenApiError error in errors)
                sb.AppendLine($"[{error.Pointer}] {error.Message}");

            return sb.ToString();
        }
    }
}
