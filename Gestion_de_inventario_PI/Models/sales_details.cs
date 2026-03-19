namespace Gestion_de_inventario_PI.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class sales_details
    {
        [Key]
        public int id_detail { get; set; }

        public int? saleid { get; set; }

        public int? productid { get; set; }

        public int amount { get; set; }

        public decimal unit_price { get; set; }

        public decimal subtotal { get; set; }

        public virtual Product Product { get; set; }

        public virtual Sale Sale { get; set; }
    }
}
