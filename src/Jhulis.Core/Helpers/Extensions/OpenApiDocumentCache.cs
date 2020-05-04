using System.Collections.Generic;

namespace Jhulis.Core.Helpers.Extensions
{
    public class OpenApiDocumentCache
    {
        public IEnumerable<OpenApiDocumentExtensions.Response> Responses { get; set; }
        public IEnumerable<OpenApiDocumentExtensions.Property> Properties { get; set; }
    }
}
