using System;
using System.Collections.Generic;
using System.Text;

namespace Jhulis.Core.Resources
{
    public static class RegexLibrary
    {
        public static string IpV4 => @"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$";
    }
}
