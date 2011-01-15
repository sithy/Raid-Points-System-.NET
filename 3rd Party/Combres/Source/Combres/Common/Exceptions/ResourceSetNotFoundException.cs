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
using System.Runtime.Serialization;

namespace Combres
{
    /// <summary>
    /// Represents an error occuring when a resource set is requested while not existing
    /// in the XML definition file.
    /// </summary>
    [Serializable]
    internal sealed class ResourceSetNotFoundException : CombresException
    {
        public string ResourceSetName { get; set; }

        public ResourceSetNotFoundException() { }
        public ResourceSetNotFoundException(string setName) : this(setName, null) { }
        public ResourceSetNotFoundException(string setName, Exception inner) 
            : base(string.Format("Resource set '{0}' cannot be found", setName), inner)
        {
            ResourceSetName = setName;
        }
        protected ResourceSetNotFoundException(SerializationInfo info, StreamingContext ctx) : base(info, ctx) { }
    }
}