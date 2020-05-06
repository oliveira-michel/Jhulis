using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Jhulis.Rest.Models
{
    public class ValidarGetRequestModel
    {
        [Required]
        public string Content { get; set; }
        public List<Supression> Supressions { get; set; }
    }

    public class Supression
    {
        [Required]
        public string RuleName { get; set; }
        [Required]
        public string Target { get; set; }
        public string Justification { get; set; } 
    }
}
