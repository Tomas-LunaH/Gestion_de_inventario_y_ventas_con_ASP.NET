namespace Gestion_de_inventario_PI.Models
{
    public class ProductoVendidoVM
    {
        public string Nombre    { get; set; }
        public string Categoria { get; set; }
        public int    Stock     { get; set; }
        public int    Unidades  { get; set; }
        public decimal Monto   { get; set; }
    }
}
