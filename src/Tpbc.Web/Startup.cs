using Microsoft.Owin;
using Owin;
using Tpbc.Web;

[assembly: OwinStartup(typeof(Startup))]
namespace Tpbc.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.ConfigureAuth();
        }
    }
}
