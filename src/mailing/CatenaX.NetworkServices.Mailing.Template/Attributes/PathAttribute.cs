using System;

namespace CatenaX.NetworkServices.Mailing.Template.Attributes
{
    /// <summary>
    /// Attribute used for adding path metadata to a member.
    /// </summary>
    public class PathAttribute : Attribute
    {
        /// <summary>
        /// Path metadata of this attribute.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="path">Path to be attached to this attribute.</param>
        public PathAttribute(string path)
        {
            this.Path = path;
        }
    }
}
