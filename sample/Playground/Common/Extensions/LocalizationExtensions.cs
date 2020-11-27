using System;
using System.Linq;

namespace Playground.Common.Extensions
{
    public static class LocalizationExtensions
    {
        public static string WithParams<T>(this string @this, params T[] parameters)
        {
            if(@this == null || parameters == null) {
                return @this;
            } else if(parameters.Length == 1 && @this.Contains("%s")) {
                return @this.Replace("%s", parameters[0]?.ToString());
            } else {
                var result = @this;
                for(var i = 0; i < parameters.Count(); i++) {
                    result = result.Replace($"%{i+1}$s", parameters[i]?.ToString());
                }
                return result;
            }
        }
    }
}