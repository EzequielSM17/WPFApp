using System;
using System.Collections.Generic;
using System.Text;

namespace Utils
{
    public static class Session
    {
        public static string JwtToken { get; set; } = string.Empty;
        public static string UserEmail { get; set; } = string.Empty;
    }
}
