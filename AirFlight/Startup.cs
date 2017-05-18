using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(AirFlight.Startup))]
namespace AirFlight
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
