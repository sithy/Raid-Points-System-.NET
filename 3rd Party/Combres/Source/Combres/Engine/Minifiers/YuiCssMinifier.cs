﻿#region License
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

using System.Collections.Generic;
using System.Xml.Linq;
using Yahoo.Yui.Compressor;

namespace Combres.Minifiers
{
    /// <summary>
    /// CSS minifier which delegates the minification process to the YUI Compressor library (http://yuicompressor.codeplex.com/).
    /// </summary>
    public sealed class YuiCssMinifier : IResourceMinifier
    {
        /// <summary>
        /// Either StockYuiCompressor, MichaelAshRegexEnhancements, or Hybrid.
        /// </summary>
        public string CssCompressionType { get; set; }

        /// <summary>
        /// <p>Some source control tools don't like files containing lines longer than,
        /// say 8000 characters. The linebreak option is used in that case to split
        /// long lines after a specific column. It can also be used to make the code
        /// more readable, easier to debug.  Specify 0 to get a line break after 
        /// each rule in CSS.</p>
        /// 
        /// <p>Default is no line break.</p>
        /// </summary>
        public int? ColumnWidth { get; set; }

        /// <inheritdoc cref="IResourceMinifier.Minify" />
        public string Minify(Settings settings, ResourceSet resourceSet, string combinedContent)
        {
            var type = (CssCompressionType)CssCompressionType.ConvertToType(
                typeof(CssCompressionType), 
                Yahoo.Yui.Compressor.CssCompressionType.StockYuiCompressor);
            return CssCompressor.Compress(combinedContent, 
                ColumnWidth == null ? -1 : ColumnWidth.Value,
                type);
        }
    }
}
