using System;
using System.Text.RegularExpressions;

namespace CodeSearcher.BusinessLogic.Common
{
    public static class Helper
    {
        public static String WildcardResolver(String value)
        {
            return Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*");
        }
    }
}
