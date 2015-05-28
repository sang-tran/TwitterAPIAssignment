using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(KMSTwitter.Startup))]
[assembly: log4net.Config.XmlConfigurator(ConfigFile = "Web.config", Watch = true)]
namespace KMSTwitter
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
