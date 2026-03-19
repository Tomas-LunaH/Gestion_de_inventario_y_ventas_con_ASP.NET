namespace Gestion_de_inventario_PI.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class movement
    {
        [Key]
        public int id_movement { get; set; }

        public int? ProductId { get; set; }

        public DateTime datemovement { get; set; }

        [Required]
        [StringLength(50)]
        public string type_movement { get; set; }

        public int amount { get; set; }

        [StringLength(255)]
        public string motive { get; set; }

        [StringLength(128)]
        public string userid { get; set; }

        public virtual Product Product { get; set; }

     
    }
}
