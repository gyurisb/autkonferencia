using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Events.Startup))]
namespace Events
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
