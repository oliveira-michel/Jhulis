using System.Collections.Generic;

namespace Jhulis.Core.Exceptions
{
    public class RuleNotFoundException : KeyNotFoundException
    {
        public RuleNotFoundException(string message) : base(message) { }
    }
}
