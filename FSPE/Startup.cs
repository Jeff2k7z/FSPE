using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(FSPE.Startup))]
namespace FSPE
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
