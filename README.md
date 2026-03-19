# Gestion_de_inventario_PI

Sistema de inventario y punto de venta basado en ASP.NET MVC 5 (Framework 4.7.2) con Identity, Entity Framework 6 y Stripe Checkout. Este README prioriza detalles técnicos para levantar, auditar y desplegar el proyecto sin fricción.Este proyecto funge como MVP (Producto Mínimo Viable) para la automatización de procesos comerciales, integrando seguridad empresarial, validaciones de stock en tiempo real y pasarela de pagos.

## Stack y capas
- ASP.NET MVC 5 (.NET Framework 4.7.2) + Razor Views.
- Entity Framework 6.5 (COde-First).
- ASP.NET Identity (Autenticación de roles y encriptación de contraseñas)
- SQL Server / LocalDB para desarrollo.
- Stripe Checkout (SDK 50.x) para pagos con tarjeta.
- Frontend: Bootstrap 5.2, jQuery 3.7, Chart.js (CDN), Ionicons (CDN).

## Topología del repo
- `Gestion_de_inventario_PI/` proyecto web.
  - `Controllers/` lógica MVC (Dashboard, Products, Sales, Movements, Users, etc.).
  - `Models/` entidades EF (`Product`, `Sale`, `movement`, `sales_details`, `Category`) y Identity (`ApplicationUser`).
  - `Views/` Razor para cada módulo + `_Layout.cshtml`.
  - `App_Start/` configuración (bundles, filtros, rutas, Identity).
  - `Content/` estilos (custom `style.css`, Bootstrap) e imágenes.
  - `Scripts/` JS estático (Bootstrap, jQuery, `main.js`).
  - `Web.config` configuración principal (connection strings, appSettings).
- `packages/` dependencias NuGet restauradas (no editar a mano).

## Requisitos
- Windows con Visual Studio 2022 o dotnet 4.7.2 targeting pack.
- SQL Server Express o LocalDB.
- PowerShell 5+.
- Clave de Stripe (secret key) y URL pública accesible vía HTTPS para callbacks.

## Variables de entorno y secretos
Configúralas antes de ejecutar (o en user-secrets si usas VS):
- `private.config` add key="StripeSecret" value="sk_test_tu_clave_aqui" "
- `APP_PUBLIC_URL` add key="AppPublicUrl" value="https://localhost:44395"" ```
- `Manager_Inventary_PI` (opcional) : cadena de conexión override para `Model_db`.



## Base de datos y migraciones
Actualmente el DbContext `Model1` persiste productos, ventas, movimientos, categorías y detalles de venta. Identity vive en `ApplicationDbContext` sobre `DefaultConnection`.

## Ejecución en desarrollo
1) Restaurar paquetes NuGet (VS los restaura al abrir la solución).  
2) Configurar variables `STRIPE_SECRET` y `APP_PUBLIC_URL`.  
3) Crear DB si no existe (`Update-Database` para ambos contextos o adjuntar `.mdf` en `App_Data` solo para dev).  
4) Ejecutar en IIS Express (SSL en puerto 44395 según `Web.config`) 
5) Navegar a `https://localhost:44395/` y autenticarse. Roles se manejan con Identity.

## Pruebas y comprobaciones mínimas
- **Login/Logout**: `/Account/Login` debe redirigir a `/Dashboard` tras éxito.
- **Venta en efectivo**: `/Sales` con carrito > genera `Sale`, `sales_details` y `movement` con stock decrementado y `UserId` del usuario autenticado.
- **Stripe sandbox**: pago de prueba debe redirigir con `pago=exito` y crear la venta; usa tarjeta 4242 4242 4242 4242.
- **Movements audit**: `/Movements` debe mostrar correo (Identity) en la columna Usuario.
- **Categorías y productos**: CRUD básico y stock crítico en Dashboard.

## Despliegue
- Preferido: Azure App Service / IIS detrás de HTTPS.  
- Establecer variables de aplicación (`private.config`, `APP_PUBLIC_URL`, `InventoryDb` si se usa override).  
- Migraciones: correr `Update-Database` antes del swap.  
- Publicar con `msbuild /p:DeployOnBuild=true` o pipeline CI/CD.  
- Habilitar logging (Application Insights o Serilog) y health-check (`/` o `/Dashboard`).  

## Roadmap y Trabajo Futuro (V 2.0)
Como parte del ciclo de vida del software y gestión de deuda técnica, las siguientes características están contempladas para la siguiente mejora de desarrollo:

- Implementación de Webhooks de Stripe: Sustituir la persistencia en memoria del carrito (diccionario estático actual) por una confirmación asíncrona mediante Webhooks, eliminando la dependencia de la URL de retorno del cliente.
- Tokens CSRF en peticiones AJAX: Agregar el atributo [ValidateAntiForgeryToken] en los endpoints de cobro e inyectar el token en los headers de las peticiones de jQuery para prevenir falsificación de solicitudes entre sitios.



---
Desarrollador: Angel Luna Hernandez Alumno de 5to Tetramestre - Desarrollo de Software Multiplataforma (UTTN).
Última actualización de arquitectura: Marzo 2026.
