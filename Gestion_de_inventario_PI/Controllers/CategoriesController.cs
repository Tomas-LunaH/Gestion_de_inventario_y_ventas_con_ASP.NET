using System;
using System.Linq;
using System.Web.Mvc;
using Gestion_de_inventario_PI.Models;

namespace Gestion_de_inventario_PI.Controllers
{
    [Authorize]
    public class CategoriesController : Controller
    {
        private readonly db_model db = new db_model();

        [HttpGet]
        public ActionResult List()
        {
            var data = db.Categories
                .OrderBy(c => c.Name)
                .Select(c => new { c.Id_category, c.Name, c.Description })
                .ToList();
            return Json(new { success = true, data }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Create(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Json(new { success = false, message = "El nombre es requerido" });

            var cat = new Category
            {
                Name = name.Trim(),
                Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim()
            };
            db.Categories.Add(cat);
            db.SaveChanges();
            return Json(new { success = true, id = cat.Id_category });
        }

        [HttpPost]
        public ActionResult Update(int id, string name, string description)
        {
            var cat = db.Categories.Find(id);
            if (cat == null)
                return Json(new { success = false, message = "Categoría no encontrada" });
            if (string.IsNullOrWhiteSpace(name))
                return Json(new { success = false, message = "El nombre es requerido" });

            cat.Name = name.Trim();
            cat.Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
            db.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var cat = db.Categories.Find(id);
            if (cat == null)
                return Json(new { success = false, message = "Categoría no encontrada" });

            db.Categories.Remove(cat);
            db.SaveChanges();
            return Json(new { success = true });
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
