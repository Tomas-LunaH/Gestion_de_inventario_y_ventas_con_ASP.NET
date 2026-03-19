using Gestion_de_inventario_PI.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace Gestion_de_inventario_PI.Controllers

{
    [Authorize]
    public class MovementsController : Controller
    {

        private Model1 db = new Model1();
      

        // 1. VER HISTORIAL DE MOVIMIENTOS
        public ActionResult Index()
        {
            ViewBag.ProductId = new SelectList(db.Products.Where(p => p.status == true), "id_product", "name");
            // Traemos todos los movimientos ordenados por fecha, incluyendo el nombre del producto
            var movimientos = db.movements.Include("Product").OrderByDescending(m => m.datemovement).ToList();

            // Resolvemos los correos de usuario desde AspNetUsers (Identity)
            var userIds = movimientos
                            .Where(m => !string.IsNullOrEmpty(m.userid))
                            .Select(m => m.userid)
                            .Distinct()
                            .ToList();

            using (var identityContext = new ApplicationDbContext())
            {
                var lookup = identityContext.Users
                                            .Where(u => userIds.Contains(u.Id))
                                            .ToDictionary(u => u.Id, u => u.Email);
                ViewBag.UserEmails = lookup;
            }

            return View(movimientos);
        }

        // 2. FORMULARIO PARA CREAR (GET)
        public ActionResult Create()
        {
            // Mandamos la lista de productos a la vista para el <select>
            ViewBag.ProductId = new SelectList(db.Products.Where(p => p.status == true), "id_product", "name");
            return View();
        }

        // 3. GUARDAR EL MOVIMIENTO (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(movement movimiento)
        {
            if (ModelState.IsValid)
            {
                // 1. Llenamos los datos automáticos
                movimiento.datemovement = DateTime.Now;
                movimiento.userid = User.Identity.GetUserId();

                // 2. Buscamos el producto para actualizar su stock
                var productoBD = db.Products.Find(movimiento.ProductId);

                if (productoBD != null)
                {
                    // LÓGICA DE SUMA O RESTA DE INVENTARIO
                    if (movimiento.type_movement == "Entrada" || movimiento.type_movement == "Devolucion")
                    {
                        productoBD.stock += movimiento.amount;
                    }
                    else if (movimiento.type_movement == "Merma" || movimiento.type_movement == "Ajuste")
                    {
                        productoBD.stock -= movimiento.amount; // Restamos del inventario
                    }

                    // 3. Guardamos todo
                    db.movements.Add(movimiento);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }

            // Si algo falla, recargamos la lista de productos y volvemos a mostrar la vista Index
            ViewBag.ProductId = new SelectList(db.Products.Where(p => p.status == true), "id_product", "name", movimiento.ProductId);
            var movimientos = db.movements.Include("Product").OrderByDescending(m => m.datemovement).ToList();

            var userIds = movimientos.Where(m => !string.IsNullOrEmpty(m.userid))
                                     .Select(m => m.userid)
                                     .Distinct()
                                     .ToList();

            using (var identityContext = new ApplicationDbContext())
            {
                var lookup = identityContext.Users
                                            .Where(u => userIds.Contains(u.Id))
                                            .ToDictionary(u => u.Id, u => u.Email);
                ViewBag.UserEmails = lookup;
            }

            return View("Index", movimientos);
        }

        // 4. ELIMINAR UN MOVIMIENTO (Y REVERTIR EL STOCK)
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public JsonResult Eliminar(int id)
        {
            try
            {
                var movimiento = db.movements.Find(id);
                if (movimiento == null) return Json(new { success = false, message = "No encontrado." });

                var productoBD = db.Products.Find(movimiento.ProductId);

                // Revertimos el stock (Si fue entrada, lo restamos. Si fue merma, lo sumamos)
                if (productoBD != null)
                {
                    if (movimiento.type_movement == "Entrada" || movimiento.type_movement == "Devolucion")
                        productoBD.stock -= movimiento.amount;
                    else if (movimiento.type_movement == "Merma" || movimiento.type_movement == "Ajuste")
                        productoBD.stock += movimiento.amount;
                }

                db.movements.Remove(movimiento);
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}




