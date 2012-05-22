using System;
using Microsoft.SPOT;

namespace System.Diagnostics
{
    public class Debug
    {
        [Conditional("TRACE")]
        public static void WriteLine(string text)
        {
            Microsoft.SPOT.Trace.Print(text);
        }
    }
}