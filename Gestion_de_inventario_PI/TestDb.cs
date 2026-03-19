using System;
using System.Collections.Generic;
using System.Linq;
using Gestion_de_inventario_PI.Models;
using System.Data.Entity;

namespace DbCheck
{
    class Program
    {
        static void Main()
        {
            try
            {
                using (var db = new Model1())
                using (var identity = new ApplicationDbContext())
                {
                    // Usuarios desde AspNetUsers (Identity)
                    var identityUsers = identity.Users.ToList();
                    var userLookup = identityUsers.ToDictionary(u => u.Id, u => u.Email);

                    Console.WriteLine($"Total Identity Users: {identityUsers.Count}");
                    foreach (var u in identityUsers)
                    {
                        Console.WriteLine($"- ID: {u.Id}, Email: {u.Email}");
                    }

                    // Últimos 5 movimientos con correo de Identity
                    var movements = db.movements
                                      .Include("Product")
                                      .OrderByDescending(m => m.datemovement)
                                      .Take(5)
                                      .ToList();

                    Console.WriteLine($"\nLast 5 Movements:");
                    foreach (var m in movements)
                    {
                        var email = (!string.IsNullOrEmpty(m.userid) && userLookup.ContainsKey(m.userid))
                            ? userLookup[m.userid]
                            : "NULL";
                        Console.WriteLine($"- Mov ID: {m.id_movement}, UserID: {m.userid}, Email: {email}");
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
