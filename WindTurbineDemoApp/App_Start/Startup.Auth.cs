using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

using IntelligentPlant.Authentication;

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;

using Owin;

namespace WindTurbineDemoApp
{
    /// <summary>
    /// OWIN startup class.
    /// </summary>
    public partial class Startup
    {

        /// <summary>
        /// Base URL for the App Store.
        /// </summary>
        internal static readonly string AppStoreBaseUrl = ConfigurationManager.AppSettings["appStore:baseUrl"];


        /// <summary>
        /// Configures App Store authentication for the application.
        /// </summary>
        /// <param name="app">The OWIN application.</param>
        private void ConfigureAppStoreAuthentication(IAppBuilder app)
        {
            // TODO: *** create an IOAuthTokenStore implementation that persists tokens ***
            app.CreatePerOwinContext<IOAuthTokenStore>(() => new MemoryOAuthTokenStore());

            // Allow authentication via an application cookie that gets set when the user authenticates with the App 
            // Store using OAuth.
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                CookieName = ".WindTurbineDemoApp.Session",
                // Logins handled by login provider middleware configured below.
                LoginPath = new PathString(LoginProviderOptions.DefaultLoginPath),
                // Logouts handled by login provider middleware configured below.
                LogoutPath = new PathString(LoginProviderOptions.DefaultLogoutPath),
                // Inactive sessions expire after 30 days.  Note that we have to explicitly tell the application to 
                // make sessions persistent - see the login provider configuration below for an explanation of how to 
                // do this.
                ExpireTimeSpan = TimeSpan.FromDays(30),
                SlidingExpiration = true,

            });

            // Allow temporary storage of information received from an external service (e.g. App Store) in a cookie.
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // App Store authentication options.
            var appStoreAuthenticationOptions = new AppStoreAuthenticationOptions()
            {
                BaseUrl = AppStoreBaseUrl,
                ClientId = ConfigurationManager.AppSettings["appStore:clientId"],
                ClientSecret = ConfigurationManager.AppSettings["appStore:clientSecret"],
                Scope = new List<string>() {
                    "UserInfo", // Request access to App Store user profile.
                    "DataRead", // Request access to App Store Connect data; user can control which sources the application can access.
                    //"AccountDebit" // Request ability to bill for usage.
                },
                Prompt = "consent", // Always display the consent screen; remove this property to enable automatic re-consent.
                Provider = new AppStoreAuthenticationProvider()
                {
                    // Persist the token when the user logs in.
                    OnAuthenticated = context => context.OwinContext.Get<IOAuthTokenStore>().SetTokenDetails(context.Id, context.AccessTokenDetails, context.OwinContext.Request.CallCancelled)
                }
            };
            app.UseAppStoreAuthentication(appStoreAuthenticationOptions);

            // We are not using ASP.NET Identity user management, so use the login provider middleware to manage 
            // login and logout requests for us.
            app.UseLoginProvider(new LoginProviderOptions()
            {
                ChallengeAuthenticationType = AppStoreAuthenticationOptions.DefaultAuthenticationType,
                SignInAsAuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                OnBeforeSignIn = context =>
                {
                    // TODO: *** Decide if you want to persist sessions between browser sessions ***
                    // Set IsPersistent to true to persist the session cookie between browser sessions.
                    context.Properties.IsPersistent = false;
                    return Task.FromResult<object>(null);
                }
            });

            // Middleware that will run after authentication has finished and will retrieve the OAuth token for the 
            // caller and set an appropriate Authenticate delegate on the HttpConnectionSettings for the caller.
            app.UseOAuthTokenWithDataCore();

        }

    }
}