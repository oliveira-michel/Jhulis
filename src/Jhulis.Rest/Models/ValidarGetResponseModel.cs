using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Jhulis.Core;

namespace Jhulis.Rest.Models
{
    public class ValidarGetResponseModel
    {
        [EnumDataType(typeof(Status))]
        public string Result { get; set; }

        public List<ResultItemModel> ResultItens { get; set; }
    }

    public struct ResultItemModel{
        public string Rule { get; set; }
        public string Description { get; set; }
        public string Message { get; set; }
        
        [EnumDataType(typeof(Severity))]
        public string Severity { get; set; }
        public string Value { get; set; }
    }
}
