using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using NCoreUtils.Images.Internal;

namespace NCoreUtils.Images
{
    public static class FilterParser
    {
        private static readonly IReadOnlyList<IFilter> _noFilters = new IFilter[0];

        private static readonly Regex _regexBlur = new Regex("^blur\\(([0-9]+(\\.[0-9]+)?)\\)$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static IReadOnlyList<IFilter> Parse(string? input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return _noFilters;
            }
            var raws = input.Split(';', StringSplitOptions.RemoveEmptyEntries);
            var filters = new IFilter[raws.Length];
            for (var i = 0; i < raws.Length; ++i)
            {
                var m = _regexBlur.Match(raws[i]);
                if (m.Success)
                {
                    filters[i] = new Blur(double.Parse(m.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture));
                }
                else
                {
                    throw new ImageException("not_supported_filter", $"Unable to parse filter ${raws[i]}.");
                }
            }
            return filters;
        }
    }
}