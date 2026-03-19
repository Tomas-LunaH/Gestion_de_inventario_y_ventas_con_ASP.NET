using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Gestion_de_inventario_PI.Startup))]
namespace Gestion_de_inventario_PI
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
