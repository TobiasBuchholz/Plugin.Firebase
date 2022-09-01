using System.Text;
using System.Text.RegularExpressions;

namespace System;

public static class StringExtensions
{
    public static string ReplaceChar(this string @this, char newChar, int charIndex)
    {
        var stringBuilder = new StringBuilder(@this);
        stringBuilder[charIndex] = newChar;
        return stringBuilder.ToString();
    }

    public static string StripHTML(this string input)
    {
        return Regex.Replace(input, "<.*?>", string.Empty);
    }

    public static bool IsValidEmailAddress(this string email)
    {
        return email != null && Regex.IsMatch(email, @"^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$");
    }

    public static string TruncateAroundWord(this string @this, string word, int length)
    {
        var pos = @this.ToLower().IndexOf(word.ToLower(), StringComparison.CurrentCulture);
        var start = Math.Max(0, pos - length);
        var end = Math.Min(pos + length, @this.Length);
        return @this.Substring(start, end - start);
    }

    public static string Truncate(this string @this, int length)
    {
        return @this.Length > length ? @this.Substring(0, length) : @this;
    }

    public static string SubstringToEnd(this string @this, int startIndex)
    {
        return @this.Substring(startIndex, @this.Length - startIndex);
    }

    public static bool StartsWithSafe(this string @this, string value)
    {
        return @this != null && value != null && @this.StartsWith(value);
    }

    public static string FromUnderscoreToCamelCase(this string name)
    {
        if(string.IsNullOrEmpty(name) || !name.Contains("_")) {
            return name;
        }

        var array = name.Split('_');
        for(var i = 0; i < array.Length; i++) {
            var s = array[i];
            var first = string.Empty;
            var rest = string.Empty;

            if(s.Length > 0) {
                first = char.ToUpperInvariant(s[0]).ToString();
            }

            if(s.Length > 1) {
                rest = s.Substring(1).ToLowerInvariant();
            }
            array[i] = first + rest;
        }

        var newName = string.Join("", array);
        if(newName.Length > 0) {
            newName = char.ToLowerInvariant(newName[0]) + newName.Substring(1);
        } else {
            newName = name;
        }
        return newName;
    }

    /// Ensures the given string is returned as a valid url. It serves as a workaround for a problem at navigation
    /// with shell, which converts normal url to https:/url.to/file (single slash).
    public static string FixBrokenUrl(this string @this)
    {
        return Regex.Replace(@this, "^(https?:)/+(.*)$", "$1//$2");
    }

    public static bool EqualsSafe(this string @this, string other)
    {
        if(@this == null && other == null) {
            return true;
        } else if(@this == null) {
            return false;
        } else if(other == null) {
            return false;
        } else {
            return @this.Equals(other);
        }
    }

    public static bool IsPositiveNumber(this string @this)
    {
        return @this != null && int.TryParse(@this, out var value) && value > 0;
    }
}