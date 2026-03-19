using Gestion_de_inventario_PI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace Gestion_de_inventario_PI.Controllers
{
    [Authorize]
    [NoCache]
    public class DashboardController : Controller
    {
        private Model1 db = new Model1();
        // GET: Dashboard
        public ActionResult Index()
        {
            // --- Stock Crítico (< 15 unidades) ---
            int stockCritico = db.Products.Count(p => p.stock < 10 && p.status == true);

            // --- Valor del Inventario ---
            decimal valorInventario = db.Products
                .Where(p => p.status == true)
                .Sum(p => (decimal?)(p.saleprice * p.stock)) ?? 0;

            // ---  Ventas Hoy ---
            var hoy = DateTime.Today;
            var manana = hoy.AddDays(1);
            decimal ventasHoy = db.Sales
                .Where(s => s.date >= hoy && s.date < manana)
                .Sum(s => (decimal?)s.Total) ?? 0;

            // --- Utilidad Bruta Hoy (ingresos - costo) ---
            var detallesHoy = db.sales_details
                .Where(d => d.Sale.date >= hoy && d.Sale.date < manana)
                .Select(d => new
                {
                    d.subtotal,
                    costoUnitario = d.Product.buyingprice,
                    d.amount
                }).ToList();

            decimal utilidadHoy = detallesHoy.Sum(d => d.subtotal - (d.costoUnitario * d.amount));

            // Inicio de semana: lunes a lunes
            int diasDesdelunes = ((int)hoy.DayOfWeek == 0) ? 6 : (int)hoy.DayOfWeek - 1;
            var inicioSemana = hoy.AddDays(-diasDesdelunes);
            var topProductos = db.sales_details
                .Where(d => d.Sale.date >= inicioSemana && d.Sale.date < manana)
                .GroupBy(d => d.productid)
                .Select(g => new
                {
                    productid = g.Key,
                    totalUnidades = g.Sum(x => x.amount),
                    totalMonto    = g.Sum(x => x.subtotal)
                })
                .OrderByDescending(x => x.totalUnidades)
                .Take(10)
                .ToList();

            var productIds = topProductos.Select(x => x.productid).ToList();
            var productos = db.Products
                .Where(p => productIds.Contains(p.id_product))
                .Select(p => new { p.id_product, p.name, p.stock, p.status,
                                   categoria = p.Category != null ? p.Category.Name : "Sin categoría" })
                .ToList();

            var topProductosViewModel = topProductos.Select(t =>
            {
                var prod = productos.FirstOrDefault(p => p.id_product == t.productid);
                return new ProductoVendidoVM
                {
                    Nombre    = prod?.name ?? "Desconocido",
                    Categoria = prod?.categoria ?? "—",
                    Stock     = prod?.stock ?? 0,
                    Unidades  = t.totalUnidades,
                    Monto     = t.totalMonto
                };
            }).ToList();

            var mx = new System.Globalization.CultureInfo("es-MX");
            ViewBag.StockCritico        = stockCritico;
            ViewBag.ValorInventario     = valorInventario.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
            ViewBag.VentasHoy           = ventasHoy.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
            ViewBag.UtilidadHoy         = utilidadHoy.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
            ViewBag.ValorInventarioStr  = string.Format(mx, "${0:N2}", valorInventario);
            ViewBag.VentasHoyStr        = string.Format(mx, "${0:N2}", ventasHoy);
            ViewBag.UtilidadHoyStr      = string.Format(mx, "${0:N2}", utilidadHoy);
            ViewBag.TopProductos        = topProductosViewModel;

            var categories = db.Categories.Include(c => c.Products).OrderBy(c => c.Name).ToList();
            return View(categories);
        }

        [HttpGet]
        public JsonResult ListarCategorias()
        {
            var data = db.Categories
                .OrderBy(c => c.Name)
                .Select(c => new { c.Id_category, c.Name })
                .ToList();
            return Json(new { success = true, data }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CrearCategoria(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return Json(new { success = false, message = "El nombre es requerido" });
            }

            var nuevaCategoria = new Category { Name = nombre.Trim() };
            db.Categories.Add(nuevaCategoria);
            db.SaveChanges();
            return Json(new { success = true, id = nuevaCategoria.Id_category, nombre = nuevaCategoria.Name });
        }

        [HttpPost]
        public JsonResult ActualizarCategoria(int id, string nombre, string descripcion)
        {
            var categoria = db.Categories.Find(id);
            if (categoria == null)
                return Json(new { success = false, message = "Categoría no encontrada" });
            if (string.IsNullOrWhiteSpace(nombre))
                return Json(new { success = false, message = "El nombre es requerido" });

            categoria.Name = nombre.Trim();
            categoria.Description = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim();
            db.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost]
        public JsonResult EliminarCategoria(int id)
        {
            var categoria = db.Categories.Find(id);
            if (categoria != null)
            {
                bool tieneProductos = db.Products.Any(p => p.categoryId == id && p.status == true);
                if (tieneProductos)
                {
                    return Json(new { success = false, message = "No puedes borrar una categoría que tiene productos activos." });
                }

                db.Categories.Remove(categoria);
                db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Error al encontrar la categoría." });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
