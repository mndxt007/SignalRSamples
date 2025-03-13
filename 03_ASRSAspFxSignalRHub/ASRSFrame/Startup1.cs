using Microsoft.Owin;
using Owin;
using System;
using System.Threading.Tasks;

[assembly: OwinStartup(typeof(ASRSFrame.Startup1))]

namespace ASRSFrame
{
    public class Startup1
    {
        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=316888
            app.MapAzureSignalR(this.GetType().FullName);
            app.MapSignalR();
        }
    }
}
