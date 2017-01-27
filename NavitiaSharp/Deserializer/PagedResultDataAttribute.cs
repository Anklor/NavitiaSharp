using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavitiaSharp.Deserializers
{
    /// <summary>
    /// Custom attribute used by deserializer
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, Inherited = false)]
    public sealed class PagedResultDataAttribute : Attribute
    {
        /// <summary>
        /// The name to use for the serialized element
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Sets if the property to Deserialize is an Attribute or Element (Default: false)
        /// </summary>
        public bool Attribute { get; set; }
    }
}
