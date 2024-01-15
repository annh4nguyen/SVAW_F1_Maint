using System;
using Microsoft.Owin;
using Owin;

namespace avSVAW
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
            //ConfigureAuth(app);
        }

       
    }
}
