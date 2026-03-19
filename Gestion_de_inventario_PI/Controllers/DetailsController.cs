using Gestion_de_inventario_PI.Models;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;

namespace Gestion_de_inventario_PI.Controllers
{
    [Authorize]
    public class DetailsController : Controller
    {
        private readonly Model1 db = new Model1();

        // Lista todas las ventas
        public ActionResult Index()
        {
            var ventas = db.Sales.OrderByDescending(v => v.date).ToList();
            return View(ventas);
        }

        // Detalles por venta (JSON para el modal)
        [HttpGet]
        public JsonResult ObtenerDetalles(int id)
        {
            var detalles = db.sales_details
                .Include("Product")
                .Where(d => d.saleid == id)
                .Select(d => new
                {
                    nombre = d.Product != null ? d.Product.name : "",
                    cantidad = d.amount,
                    precio = d.unit_price,
                    subtotal = d.subtotal
                }).ToList();

            return Json(detalles, JsonRequestBehavior.AllowGet);
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
