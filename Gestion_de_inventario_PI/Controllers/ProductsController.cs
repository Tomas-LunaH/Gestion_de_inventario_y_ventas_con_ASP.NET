using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Gestion_de_inventario_PI.Models;

namespace Gestion_de_inventario_PI.Controllers
{
    [Authorize]
    public class ProductsController : Controller

    {
        private Model1 db = new Model1();
        [Authorize]

        // GET: Products
        public ActionResult Index()
        {
            var products = db.Products.Include(p => p.Category);
            ViewBag.Categorias = db.Categories.OrderBy(c => c.Name).ToList();
            return View(products.ToList());
        }

        // GET: Products/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // GET: Products/Create
        public ActionResult Create()
        {
            ViewBag.categoryId = new SelectList(db.Categories, "Id_category", "Name");
            return View();
        }

        // POST: Products/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id_product,code,name,buyingprice,saleprice,stock,image,status,categoryId,Descrption")] Product product, HttpPostedFileBase imagenArchivo)
        {
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrWhiteSpace(product.code) && db.Products.Any(p => p.code == product.code))
                {
                    ModelState.AddModelError("code", "Ya existe un producto con ese código");
                }
            }

            if (ModelState.IsValid)
            {
                if (imagenArchivo != null && imagenArchivo.ContentLength > 0)
                {
                    string fileName = System.IO.Path.GetFileName(imagenArchivo.FileName);
                    string path = System.IO.Path.Combine(Server.MapPath("~/Content/images/"), fileName);
                    
                    string dir = Server.MapPath("~/Content/images/");
                    if (!System.IO.Directory.Exists(dir))
                    {
                        System.IO.Directory.CreateDirectory(dir);
                    }

                    imagenArchivo.SaveAs(path);
                    product.image = "~/Content/images/" + fileName;
                }

                product.status = true; // Por defecto todo producto nuevo nace ACTIVO
                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.categoryId = new SelectList(db.Categories, "Id_category", "Name", product.categoryId);
            return View(product);
        }

        // GET: Products/Edit/5
        [Authorize(Roles = "Administrador")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            ViewBag.categoryId = new SelectList(db.Categories, "Id_category", "Name", product.categoryId);
            return View(product);
        }

        // POST: Products/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id_product,code,name,buyingprice,saleprice,stock,image,status,categoryId,Descrption")] Product product, HttpPostedFileBase imagenArchivo)
        {
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrWhiteSpace(product.code) && db.Products.Any(p => p.id_product != product.id_product && p.code == product.code))
                {
                    ModelState.AddModelError("code", "Ya existe un producto con ese código");
                }
            }

            if (ModelState.IsValid)
            {
                if (imagenArchivo != null && imagenArchivo.ContentLength > 0)
                {
                    string fileName = System.IO.Path.GetFileName(imagenArchivo.FileName);
                    string path = System.IO.Path.Combine(Server.MapPath("~/Content/images/"), fileName);
                    
                    string dir = Server.MapPath("~/Content/images/");
                    if (!System.IO.Directory.Exists(dir))
                    {
                        System.IO.Directory.CreateDirectory(dir);
                    }

                    imagenArchivo.SaveAs(path);
                    product.image = "~/Content/images/" + fileName;
                }

                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.categoryId = new SelectList(db.Categories, "Id_category", "Name", product.categoryId);
            return View(product);
        }

        // GET: Products/Delete/5
        [Authorize(Roles = "Administrador")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: Products/Delete/5
        [Authorize(Roles = "Administrador")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);

            // Verifica si el producto está siendo usado en alguna venta o movimiento
            bool tieneVentas = db.sales_details.Any(sd => sd.productid == id);
            bool tieneMovimientos = db.movements.Any(im => im.ProductId == id);

            if (tieneVentas || tieneMovimientos)
            {
                // Soft-Delete: Inactivar en lugar de borrar
                product.status = false;
                db.Entry(product).State = EntityState.Modified;
                TempData["DeleteWarning"] = "El producto tiene ventas o movimientos registrados. Ha sido desactivado en lugar de eliminado permanentemente.";
            }
            else
            {
                // Hard-Delete: Borrar de verdad
                db.Products.Remove(product);
            }

            try 
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                TempData["DeleteError"] = "No se pudo eliminar el producto: " + ex.Message;
            }
            
            return RedirectToAction("Index");
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
