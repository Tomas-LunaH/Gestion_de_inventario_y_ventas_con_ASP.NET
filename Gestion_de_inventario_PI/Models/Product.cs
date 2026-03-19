namespace Gestion_de_inventario_PI.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Product
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Product()
        {
            movements = new HashSet<movement>();
            sales_details = new HashSet<sales_details>();
        }

        [Key]
        public int id_product { get; set; }

        [StringLength(50)]
        public string code { get; set; }

        [Required]
        [StringLength(150)]
        public string name { get; set; }

        public decimal buyingprice { get; set; }

        public decimal saleprice { get; set; }

        public int stock { get; set; }

        [StringLength(500)]
        public string image { get; set; }

        public bool status { get; set; }

        public int? categoryId { get; set; }

        public string Descrption { get; set; }

        public virtual Category Category { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<movement> movements { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<sales_details> sales_details { get; set; }
    }
}
