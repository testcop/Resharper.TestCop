// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2014
// --

using System;
using System.Collections.Generic;
using System.Linq;

namespace TestCop.Plugin.Extensions
{
    public static class StringExtensions
    {
        public static bool In(this String refString, params string[] values)
        {
            return values.Contains(refString);
        }

        public static bool EndsWith(this String refString, IEnumerable<string> values)
        {
            return values.Any(refString.EndsWith);
        }

        public static bool StartsWith(this String refString, IEnumerable<string> values)
        {
            return values.Any(refString.StartsWith);
        }

        public static string RemoveTrailing(this String refString, string value)
        {
            if(refString.EndsWith(value))
            {
                return refString.Substring(0, refString.Length - value.Length);
            }
            return refString;            
        }

        public static string Flip(this String refString, bool remove, string value)
        {
            if (refString.EndsWith(value))
            {
                if (remove)
                {
                    return refString.RemoveTrailing(value);
                }
                return refString;
            }

            if(remove)
            {
                return refString;
            }
            return refString+value;
        }

        public static string AppendIfNotNull(this String refString, string separator, string value)        
        {            
            return string.IsNullOrEmpty(value) 
                ? refString
                : refString + (refString.Length>0 ? separator :"") + value;
        }
    
        public static string AppendIfMissing(this String refString, string value)
        {
            return refString.EndsWith(value) ? refString : refString + value;
        }
        
        public static string RemoveLeading(this String refString, string value)
        {       
            if (refString.StartsWith(value))
            {
                return refString.Substring(value.Length);
            }            
            return refString;
        }
        
        public static string RemoveLeading(this String refString, string[] values)
        {
            foreach (string value in values.OrderByDescending())
            {
                if(refString.StartsWith(value))
                {
                    return refString.RemoveLeading(value);
                }
            }
            return refString;
        }
    }
}
