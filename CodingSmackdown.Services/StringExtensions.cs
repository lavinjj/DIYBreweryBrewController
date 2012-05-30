#region License
// Copyright (c) 2010 Ross McDermott
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation 
// files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, 
// modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the 
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR 
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, 
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
#endregion

using System;
using System.Text;
using Microsoft.SPOT;

namespace NetMf.CommonExtensions
{
    /// <summary>
    /// General string extensions
    /// </summary>
    public static class StringExtensions
    {
       
        /// <summary>
        /// Replace all occurances of the 'find' string with the 'replace' string.
        /// </summary>
        /// <param name="content">Original string to operate on</param>
        /// <param name="find">String to find within the original string</param>
        /// <param name="replace">String to be used in place of the find string</param>
        /// <returns>Final string after all instances have been replaced.</returns>
        public static string Replace(this string content, string find, string replace)
        {
            int startFrom = 0;
            int findItemLength = find.Length;

            int firstFound = content.IndexOf(find, startFrom);
            StringBuilder returning = new StringBuilder();

            string workingString = content;

            while ((firstFound = workingString.IndexOf(find, startFrom)) >= 0)
            {
                returning.Append(workingString.Substring(0, firstFound));
                returning.Append(replace);

                // the remaining part of the string.
                workingString = workingString.Substring(firstFound + findItemLength, workingString.Length - (firstFound + findItemLength));
            }

            returning.Append(workingString);

            return returning.ToString();

        }

       
    }
}
