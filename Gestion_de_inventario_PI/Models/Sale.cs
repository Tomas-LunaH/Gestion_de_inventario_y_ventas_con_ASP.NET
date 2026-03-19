namespace Gestion_de_inventario_PI.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Sale
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Sale()
        {
            sales_details = new HashSet<sales_details>();
        }

        [Key]
        public int id_sale { get; set; }

        public DateTime date { get; set; }

        public decimal Total { get; set; }

        [Required]
        [StringLength(50)]
        public string payment { get; set; }

        public decimal pay { get; set; }

        public decimal change { get; set; }

        public bool status { get; set; }

        [StringLength(128)]
        public string userId { get; set; }


   

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<sales_details> sales_details { get; set; }
    }
}
