using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Ownorent.Startup))]
namespace Ownorent
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
