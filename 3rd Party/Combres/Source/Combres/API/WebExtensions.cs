#region License

// Copyright 2010 Buu Nguyen (http://www.buunguyen.net/blog)
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://combres.codeplex.com

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Fasterflect;
using System.Globalization;
using System.Web.Routing;

namespace Combres
{
    /// <summary>
    /// Utility class providing extension methods for ASP.NET and ASP.NET MVC applications to
    /// work with the library.
    /// </summary>
    public static class WebExtensions
    {
        private const string UrlPattern = "{0}/{1}/{2}/";
        private const string PathSymbol = "@combres_path@";
        private const string JsTemplate = "<script type=\"text/javascript\" src=\"" + PathSymbol + "\"{0}></script>";
        private const string CssTemplate = "<link rel=\"stylesheet\" type=\"text/css\" href=\"" + PathSymbol + "\"{0}/>";

        /// <summary>
        /// Registers the combiner route to the <paramref name="routes"/> collection.
        /// This method must be invoked in <c>Application_Start</c> event.
        /// </summary>
        /// <param name="routes">The actual argument is typically <see cref="RouteTable.Routes"/>.</param>
        /// <param name="name">The route name.</param>
        public static void AddCombresRoute(this RouteCollection routes, string name)
        {
            var url = Configuration.GetCombresUrl();
            var adjustedUrl = url.Substring(2); // Remove the "~/"
            routes.Add(name, new Route(adjustedUrl + "/{name}/{version}/{*cacheVaryKeys}",
                                       new CombresRouteHandler()));
        }

        /// <summary>
        /// Retrieves the URL of the Combres engine as registered in XML data file.
        /// </summary>
        /// <param name="routes">The actual argument is typically <see cref="RouteTable.Routes"/>.</param>
        /// <returns>The URL of the Combres engine as registered in XML data file.</returns>
        public static string GetCombresUrl(this RouteCollection routes)
        {
            return Configuration.GetCombresUrl().ResolveUrl();
        }

        /// <summary>
        /// Generates the URL to the resource combiner for the specified <paramref name="setName"/>.
        /// </summary>
        /// <example>
        /// <![CDATA[<script type="text/javascript" src="<%= Html.CombresUrl("siteJsSet") %>"></script>]]>    
        /// </example>
        /// <param name="setName">The name of the resource set to be linked to.</param>
        /// <returns>The URL to the reousrce combiner for the specified <paramref name="setName"/>.</returns>
        public static string CombresUrl(string setName)
        {
            var settings = Settings;
            var set = GetResourceSet(setName);

            if (set.DebugEnabled && set.IgnorePipelineWhenDebug)
                throw new InvalidOperationException(
                    "This method cannot be used when DebugEnabled and IgnorePipelineWhenDebug properties of the resource set are turned-on.  Use one of CombresLink() overloads instead.");

            Predicate<ICacheVaryProvider> predicate = p => p.AppendKeyToUrl;
            var cacheVaryKeys = string.Join("/", set.GetCacheVaryStates(HttpContext.Current, predicate)
                                                    .Select(s => s.Key).ToArray());
            var fullUrl = string.Format(CultureInfo.InvariantCulture, UrlPattern, 
                                        settings.Url, 
                                        set.Name, 
                                        set.GetVersionString()
                                        ) + cacheVaryKeys;
            return fullUrl.ResolveUrl();
        }

        /// <summary>
        /// <para>
        /// Generates the link to the resource combiner for the specified 
        /// <paramref name="setName"/>.  If the set is a JavaScript set, 
        /// <![CDATA[<script type="text/javascript" src=...></script>]]> is returned.  
        /// If the set is a CSS set, <![CDATA[<link rel="stylesheet" type="text/css" href=... />]]> 
        /// is returned.
        /// </para>
        /// <para>
        /// If <see cref="ResourceSet.DebugEnabled"/> and <see cref="ResourceSet.IgnorePipelineWhenDebug"/>
        /// are both <c>true</c>, this method will generate normal links that completely bypass the Combres processing
        /// pipeline.  In that case, your resources will be served directly by IIS.
        /// </para>
        /// </summary>
        /// <example>
        /// <![CDATA[<%= Html.CombresLink("bootstrapJs") %>]]>    
        /// </example>
        /// <param name="setName">The name of the resource set to be linked to.</param>
        /// <returns>The link to the resource combiner for the specified <paramref name="setName"/>.</returns>
        public static string CombresLink(string setName)
        {
            return CombresLink(setName, null);
        }

        /// <summary>
        /// <para>
        /// Generates the link to the resource combiner for the specified 
        /// <paramref name="setName"/> with additional <paramref name="htmlAttributes"/>.  
        /// If the set is a JavaScript set, 
        /// <![CDATA[<script type="text/javascript" src=... [attributes]></script>]]> is returned.  
        /// If the set is a CSS set, <![CDATA[<link rel="stylesheet" type="text/css" href=... [attributes]/>]]> 
        /// is returned.
        /// </para>
        /// <para>
        /// If <see cref="ResourceSet.DebugEnabled"/> and <see cref="ResourceSet.IgnorePipelineWhenDebug"/>
        /// are both <c>true</c>, this method will generate normal links that completely bypass the Combres processing
        /// pipeline.  In that case, your resources will be served directly by IIS.
        /// </para>
        /// </summary>
        /// <example>
        /// <![CDATA[<%= Html.CombresLink("bootstrapJs")%>]]>    
        /// </example>
        /// <param name="setName">The name of the resource set to be linked to.</param>
        /// <param name="htmlAttributes">The HTML attributes to be added within the generated tag.  This can
        /// either be an object (e.g. anonymous object) or instance of <see cref="IDictionary{TKey,TValue}"/> 
        /// of <see cref="string"/>, <see cref="string"/>.</param>
        /// <returns>The link to the resource combiner for the specified <paramref name="setName"/>.</returns>
        public static string CombresLink(string setName, object htmlAttributes)
        {
            var set = GetResourceSet(setName);
            var template = BuildLinkTemplate(set.Type, htmlAttributes);
            if (!set.DebugEnabled || !set.IgnorePipelineWhenDebug)
                return template.Replace(PathSymbol, CombresUrl(set.Name));

            var tags = new StringBuilder();
            foreach (var resource in set)
            {
                var path = GetResourceUrl(resource);
                tags.Append(template.Replace(PathSymbol, path))
                    .Append(Environment.NewLine);
            }
            return tags.ToString();
        }

        /// <summary>
        /// If remote resource, return the URL as-is.
        /// If local resource (static or dynamic), append the URL with a content hash
        /// to make sure the browser always requests the latest contents.
        /// </summary>
        private static string GetResourceUrl(Resource resource)
        {
            if (!resource.IsInSameApplication)
                return resource.Path.ToAbsoluteUrl();
            var url = resource.Mode == ResourceMode.Dynamic
                          ? resource.Path.ToAbsoluteUrl()
                          : resource.Path.ResolveUrl();
            var contentHash = resource.ReadFromCache(true).GetHash();
            return url + (url.Contains("?") ? "&" : "?") + contentHash;
        }

        private static string BuildLinkTemplate(ResourceType type, object attributes)
        {
            var attributeString = new StringBuilder();
            if (attributes != null)
            {
                IEnumerable<KeyValuePair<string, string>> pairs;
                if (attributes is IEnumerable<KeyValuePair<string, string>>)
                {
                    pairs = attributes as IEnumerable<KeyValuePair<string, string>>;
                }
                else
                {
                    var properties = attributes.GetType().Properties();
                    pairs =
                        properties.Select(
                            prop => new KeyValuePair<string, string>(prop.Name, (string) prop.Get(attributes)));
                }
                pairs.ToList().ForEach(pair =>
                                       attributeString
                                           .Append(" ")
                                           .Append(pair.Key)
                                           .Append("=")
                                           .Append("\"")
                                           .Append(pair.Value)
                                           .Append("\""));
            }
            var templateToUse = type == ResourceType.JS
                                    ? JsTemplate
                                    : CssTemplate;
            return string.Format(CultureInfo.InvariantCulture, templateToUse, attributeString);
        }

        private static ResourceSet GetResourceSet(string setName)
        {
            var set = Settings[setName];
            if (set == null)
                throw new ResourceSetNotFoundException(setName);
            return set;
        }

        private static Settings Settings
        {
            get { return Configuration.GetSettings(); }
        }
    }
}