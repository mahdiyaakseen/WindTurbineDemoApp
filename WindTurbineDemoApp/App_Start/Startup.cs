using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Web.Http;

using Microsoft.Owin;

using Newtonsoft.Json;

using Owin;

[assembly: OwinStartup(typeof(WindTurbineDemoApp.Startup))]

namespace WindTurbineDemoApp
{
    /// <summary>
    /// OWIN startup class.
    /// </summary>
    public partial class Startup
    {

        /// <summary>
        /// Gets a flag that specifies if the application is running in App Store mode.
        /// </summary>
        internal static bool AppStoreMode
        {
            get { return Convert.ToBoolean(ConfigurationManager.AppSettings["application:useAppStore"]); }
        }


        /// <summary>
        /// Configures the OWIN application.
        /// </summary>
        /// <param name="app">The OWIN application.</param>
        public void Configuration(IAppBuilder app)
        {
            // Middleware that will add Data Core connection settings to the OWIN environment for each call.
            app.UseDataCoreConnectionSettings();

            if (AppStoreMode)
            {
                ConfigureAppStoreAuthentication(app);
            }

            ConfigureWebApi(app);
        }


        /// <summary>
        /// Configures Web API.
        /// </summary>
        /// <param name="app">The OWIN application.</param>
        private void ConfigureWebApi(IAppBuilder app)
        {
            var httpConfig = new HttpConfiguration();

            // Remove the XML formatter - we'll use JSON only.
            httpConfig.Formatters.Remove(httpConfig.Formatters.XmlFormatter);

            // JSON formatting options.
            httpConfig.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            httpConfig.Formatters.JsonFormatter.SerializerSettings.Formatting = Formatting.Indented;

            // Use attribute routing for Web API controllers.
            httpConfig.MapHttpAttributeRoutes();

            // Add Web API to the OWIN pipeline.
            app.UseWebApi(httpConfig);
        }
    }
}
