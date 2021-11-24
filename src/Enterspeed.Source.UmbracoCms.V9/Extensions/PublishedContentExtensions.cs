﻿using System.Globalization;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Enterspeed.Source.UmbracoCms.V9.Extensions
{
    public static class PublishedContentExtensions
    {
        public static string GetUrl(this IPublishedContent content, string culture = null, UrlMode mode = UrlMode.Default)
        {
            if (string.IsNullOrWhiteSpace(culture))
            {
                return content.Url(mode: mode);
            }

            try
            {
                // We want to make sure they are correctly cased, as 'en-us' will become 'en-US'
                var normalizedCultureName = CultureInfo.GetCultureInfo(culture).Name;
                return content.Url(normalizedCultureName, mode);
            }
            catch (CultureNotFoundException)
            {
            }

            return content.Url(culture, mode);
        }
    }
}