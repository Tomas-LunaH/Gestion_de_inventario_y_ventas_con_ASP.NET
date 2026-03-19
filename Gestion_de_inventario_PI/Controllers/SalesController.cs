using Gestion_de_inventario_PI.Models;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace Gestion_de_inventario_PI.Controllers
{
    [Authorize]
    public class SalesController : Controller
    {
        private Model1 db = new Model1();

        // 1. BÓVEDA SEGURA PARA STRIPE
        public static Dictionary<string, List<CarritoItem>> MemoriaSegura = new Dictionary<string, List<CarritoItem>>();

        // 2. MOLDE DEL CARRITO
        public class CarritoItem
        {
            public int id { get; set; }
            public string nombre { get; set; }
            public int cantidad { get; set; }
            public decimal precio { get; set; }
        }

      
        public ActionResult Index(string pago = null, string ticket = null)
        {
            var productos = db.Products.Where(p => p.status == true && p.stock > 0).ToList();

            if (pago == "exito" && ticket != null && MemoriaSegura.ContainsKey(ticket))
            {
                var carrito = MemoriaSegura[ticket];
                using (var transaccion = db.Database.BeginTransaction())
                {
                    try
                    {
                        // VALIDACIÓN DE STOCK ANTES DE DESCONTAR
                        var faltantes = new List<string>();
                        foreach (var item in carrito)
                        {
                            var productoBD = db.Products.Find(item.id);
                            if (productoBD == null)
                            {
                                faltantes.Add($"ID {item.id} no encontrado");
                                continue;
                            }

                            if (productoBD.stock < item.cantidad)
                            {
                                faltantes.Add($"{productoBD.name} (stock {productoBD.stock}, solicitado {item.cantidad})");
                            }
                        }

                        if (faltantes.Any())
                        {
                            transaccion.Rollback();
                            ViewBag.Mensaje = "Stock insuficiente para: " + string.Join(", ", faltantes);
                            MemoriaSegura.Remove(ticket);
                            return View(productos);
                        }

                        decimal subtotal = carrito.Sum(item => item.cantidad * item.precio);
                        decimal total = subtotal * 1.16m;

                        var currentUserId = User.Identity.GetUserId();

                        Sale nuevaVenta = new Sale
                        {
                            date = DateTime.Now,
                            Total = total,
                            payment = "Tarjeta (Stripe)",
                            userId = currentUserId
                        };
                        db.Sales.Add(nuevaVenta);
                        db.SaveChanges();

                        foreach (var item in carrito)
                        {
                            sales_details detalle = new sales_details
                            {
                                saleid    = nuevaVenta.id_sale,
                                productid = item.id,
                                amount    = item.cantidad,
                                unit_price = item.precio,
                                subtotal  = item.cantidad * item.precio
                            };
                            db.sales_details.Add(detalle);

                            movement nuevoMov = new movement
                            {
                                ProductId = item.id,
                                datemovement = DateTime.Now,
                                type_movement = "Venta",
                                amount = item.cantidad,
                                motive = "Venta Stripe",
                                userid = currentUserId
                            };
                            db.movements.Add(nuevoMov);

                            var productoBD = db.Products.Find(item.id);
                            if (productoBD != null) productoBD.stock -= item.cantidad;
                        }

                        db.SaveChanges();
                        transaccion.Commit();
                        MemoriaSegura.Remove(ticket);

                        ViewBag.Mensaje = "¡ÉXITO TOTAL! Pago verificado, venta guardada y stock descontado.";
                        ViewBag.SaleId = nuevaVenta.id_sale;
                    }
                    catch (Exception )
                    {
                        transaccion.Rollback();
                        ViewBag.Mensaje = "Ocurrió un error interno al procesar la compra. Inténtalo de nuevo o contacta a soporte.";
                        return View(productos);
                    }
                }
            }
            else if (pago == "cancelado")
            {
                ViewBag.Mensaje = "El pago con tarjeta fue cancelado por el usuario.";
            }
            else if (pago == "exito")
            {
                ViewBag.Mensaje = "Error: No se encontró el ticket de compra original en la memoria.";
            }

            return View(productos);
        }

        // COBRO LOCAL (EFECTIVO)

        [HttpPost]
        public JsonResult CobrarEfectivo(List<CarritoItem> carrito)
        {
            if (carrito == null || !carrito.Any()) return Json(new { success = false, message = "Carrito vacío." });

            using (var transaccion = db.Database.BeginTransaction())
            {
                try
                {
                    // ! PROTECCIÓN CONTRA MODIFICACIÓN DE HTML !
                    // Reasignamos el precio real del producto desde la base de datos
                    foreach (var item in carrito)
                    {
                        var productoReal = db.Products.Find(item.id);
                        if (productoReal != null)
                        {
                            item.precio = productoReal.saleprice; // El precio manda el servidor, no el navegador
                        }
                    }

                    // Validación de stock antes de cualquier movimiento
                    var faltantes = new List<string>();
                    foreach (var item in carrito)
                    {
                        var productoBD = db.Products.Find(item.id);
                        if (productoBD == null)
                        {
                            faltantes.Add($"ID {item.id} no encontrado");
                            continue;
                        }

                        if (productoBD.stock < item.cantidad)
                        {
                            faltantes.Add($"{productoBD.name} (stock {productoBD.stock}, solicitado {item.cantidad})");
                        }
                    }

                    if (faltantes.Any())
                    {
                        transaccion.Rollback();
                        return Json(new { success = false, message = "Stock insuficiente para: " + string.Join(", ", faltantes) });
                    }

                    decimal subtotal = carrito.Sum(item => item.cantidad * item.precio);
                    decimal total = subtotal * 1.16m;

                    var currentUserId = User.Identity.GetUserId();

                    Sale nuevaVenta = new Sale
                    {
                        date = DateTime.Now,
                        Total = total,
                        payment = "Efectivo",
                        userId = currentUserId
                    };
                    db.Sales.Add(nuevaVenta);
                    db.SaveChanges();

                    foreach (var item in carrito)
                    {
                        sales_details detalle = new sales_details
                        {
                            saleid    = nuevaVenta.id_sale,
                            productid = item.id,
                            amount    = item.cantidad,
                            unit_price = item.precio,
                                subtotal  = item.cantidad * item.precio
                            };
                            db.sales_details.Add(detalle);

                            movement nuevoMov = new movement
                            {
                                ProductId = item.id,
                                datemovement = DateTime.Now,
                                type_movement = "Venta",
                                amount = item.cantidad,
                                motive = "Venta a través de Caja",
                                userid = currentUserId
                            };
                            db.movements.Add(nuevoMov);

                        var productoBD = db.Products.Find(item.id);
                        if (productoBD != null) productoBD.stock -= item.cantidad;
                    }

                    db.SaveChanges();
                    transaccion.Commit();
                    return Json(new { success = true, message = "Venta en efectivo registrada correctamente.", saleId = nuevaVenta.id_sale });
                  
                }
                catch (Exception )
                {
                    transaccion.Rollback();

                    // Cualquier error
                    return Json(new { success = false, message = "Ocurrió un error interno en el servidor al procesar la solicitud. Por favor, contacte a soporte." });

                }
            }
        }

        // COBRO EXTERNO (CONEXIÓN API STRIPE)
        [HttpPost]
        public JsonResult GenerarPagoStripe(List<CarritoItem> carrito)
        {
            if (carrito == null || !carrito.Any()) return Json(new { success = false, message = "Carrito vacío." });

            string ticketVenta = Guid.NewGuid().ToString();
            MemoriaSegura[ticketVenta] = carrito;

            StripeConfiguration.ApiKey = ConfigurationManager.AppSettings["StripeSecretKey"];

            try
            {
                // ! PROTECCIÓN CONTRA MODIFICACIÓN DE HTML !
                // Aseguramos el precio también para Stripe
                foreach (var item in carrito)
                {
                    var productoReal = db.Products.Find(item.id);
                    if (productoReal != null)
                    {
                        item.precio = productoReal.saleprice; 
                    }
                }

                var domain = ConfigurationManager.AppSettings["AppPublicUrl"] 
                             ?? Request.Url?.GetLeftPart(UriPartial.Authority) 
                             ?? "https://localhost:44395";

                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                    SuccessUrl = domain + "/Sales/Index?pago=exito&ticket=" + ticketVenta,
                    CancelUrl = domain + "/Sales/Index?pago=cancelado",
                };

                foreach (var item in carrito)
                {
                    options.LineItems.Add(new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.precio * 1.16m * 100),
                            Currency = "mxn",
                            ProductData = new SessionLineItemPriceDataProductDataOptions { Name = item.nombre },
                        },
                        Quantity = item.cantidad,
                    });
                }

                var service = new SessionService();
                Session session = service.Create(options);
                return Json(new { success = true, url = session.Url });
            }
            catch (Exception )
            {
                return Json(new { success = false, message = "Ocurrió un error interno en el servidor al procesar la solicitud. Por favor, contacte a soporte." });
            }
        }
        
       
               // 6. IMPRIMIR TICKET (RECEIPT)
        public ActionResult ImprimirTicket(int id)
        {
            var venta = db.Sales.Find(id);
            if (venta == null)
            {
                return HttpNotFound("La venta no fue encontrada.");
            }

            var detalles = db.sales_details
                .Include("Product")
                .Where(d => d.saleid == id)
                .ToList();

            ViewBag.Venta = venta;
            return View(detalles);
        }

    }
}
