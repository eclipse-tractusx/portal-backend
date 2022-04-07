using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    /// <summary>
    /// Base properties for all entities.
    /// </summary>
    public class BaseEntity
    {
        /// <summary>
        /// Primary key of the entity.
        /// </summary>
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Date of entity creation.
        /// </summary>
        [Column("date_created")]
        public DateTime? DateCreated { get; set; }

        /// <summary>
        /// Date of most recent entity modification.
        /// </summary>
        [Column("date_last_changed")]
        public DateTime? DateLastChanged { get; set; }
    }
}
