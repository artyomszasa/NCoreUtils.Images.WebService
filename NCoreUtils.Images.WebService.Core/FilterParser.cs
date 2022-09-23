using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using NCoreUtils.Images.Internal;

namespace NCoreUtils.Images
{
    public static class FilterParser
    {
        private static readonly IReadOnlyList<IFilter> _noFilters = Array.Empty<IFilter>();

        private static readonly Regex _regexBlur = new("^blur\\(([0-9]+(\\.[0-9]+)?)\\)$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex _regexWaterMark = new("^watermark\\((.*?),(\\d+),(\\d+),(\\d+)\\)$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static IReadOnlyList<IFilter> Parse(IResourceFactory resourceFactory, string? input)
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
                var m2 = _regexWaterMark.Match(raws[i]);
                if (m.Success)
                {
                    filters[i] = new Blur(double.Parse(m.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture));
                }
                else if (m2.Success)
                {
                    var uri = new Uri(m2.Groups[1].Value);
                    var source = resourceFactory.CreateSource(uri, () => CoreFunctions.NotSupportedUri<IImageSource>(uri));
                    var x = int.Parse(m2.Groups[3].Value, NumberStyles.Number, CultureInfo.InvariantCulture);
                    var y = int.Parse(m2.Groups[4].Value, NumberStyles.Number, CultureInfo.InvariantCulture);
                    var gravity = (WaterMarkGravity)int.Parse(m2.Groups[2].Value, NumberStyles.Number, CultureInfo.InvariantCulture);
                    if (x != 0 & y != 0)
                    {
                        filters[i] = new WaterMark(source, gravity, x, y);
                    }
                    else
                    {
                        filters[i] = new WaterMark(source, gravity);
                    }
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