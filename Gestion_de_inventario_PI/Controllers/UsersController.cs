using System.Linq;
using System.Web.Mvc;
using System.Collections.Generic;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Gestion_de_inventario_PI.Models; // Asegúrate de que sea tu namespace

namespace Gestion_de_inventario_PI.Controllers
{
    // NADIE ENTRA AQUÍ EXCEPTO EL JEFE
    [Authorize(Roles = "Administrador")]
    public class UsersController : Controller
    {
        // Conexión a la base de datos secreta de seguridad
        private ApplicationDbContext context = new ApplicationDbContext();

        // 1. VER TODOS LOS USUARIOS
        public ActionResult Index()
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            var users = context.Users.ToList();
            var userRolesList = new List<UserWithRoleVM>();

            foreach (var user in users)
            {
                var rolesForUser = userManager.GetRoles(user.Id);
                var strRole = rolesForUser.FirstOrDefault() ?? "Sin Rol";

                userRolesList.Add(new UserWithRoleVM
                {
                    Id = user.Id,
                    Email = user.Email,
                    RoleName = strRole
                });
            }

            return View(userRolesList);
        }



        // 3. GUARDAR EL NUEVO USUARIO (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string Correo, string Password, string Rol)
        {
            if (ModelState.IsValid)
            {
                var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

                // Preparamos al usuario
                var user = new ApplicationUser { UserName = Correo, Email = Correo };

                // Lo guardamos con su contraseña encriptada
                var result = userManager.Create(user, Password);

                if (result.Succeeded)
                {
                    // Si se creó bien, le asignamos el rol (Administrador o Empleado)
                    userManager.AddToRole(user.Id, Rol);
                    return RedirectToAction("Index");
                }
                else
                {
                    // Si falla (ej. contraseña muy corta), mostramos el error
                    TempData["Error"] = result.Errors.FirstOrDefault();
                    return RedirectToAction("Index");
                }
            }

            TempData["Error"] = "Los datos ingresados no son válidos.";
            return RedirectToAction("Index");
        }

        // 4. ELIMINAR USUARIO
        [HttpPost]
        public JsonResult Eliminar(string id)
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            var user = userManager.FindById(id);

            if (user != null)
            {
                userManager.Delete(user);
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Usuario no encontrado" });
        }

       
    }
}