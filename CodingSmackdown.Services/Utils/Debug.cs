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