# Getting Started

## Add the App Store NuGet Feed to Visual Studio

The project contains a `NuGet.Config` file that enables this project to use the App Store's NuGet package source automatically.

To enable the package source in all Visual Studio projects, follow these steps:

1. Open the `Tools > NuGet Package Manager > Package Manager Settings` item in the menu.
2. Add a package source that points to `https://appstore.intelligentplant.com/NuGet/nuget`. 

## Register Your Application with the App Store

Visit the [App Store developer home page](https://appstore.intelligentplant.com/Developer/Home) to register your application.

Register the `/signin-ip` route as an authorized redirect URL for the application.  This will allow the App Store to authenticate sign in requests made by your application.  For example, if the base URL for your application is `http://localhost:12345`, you should add `http://localhost:12345/signin-ip` as an authorized redirect URL.

## Update Web.config

Open `Web.config` and update the `appStore:clientId` and `appStore:clientSecret` settings in the `appSettings` section to use the application ID and application secret generated when you registered the application with the App Store.

## Run Your Application

Press F5 to compile and run the application.  

You should be redirected to the App Store to sign in and then presented with the App Store consent screen for the application.  Select the data sources that the application will be authorized to access.

Once you have been granted consent, you will be redirected back to your application.  You will see JSON that describes who you are, as well as details about the data sources that you granted the application consent to access.

For example:

    {
      "AuthenticationType": "ApplicationCookie",
      "UserName": "a.person@myorganisation.com",
      "DataCoreUrl": "https://appstore.intelligentplant.com/gestalt",
      "DataSources": [
        {
          "Name": "MyHistorian",
          "Namespace": "29814739C35945AB1E1E69611B003505E0959175892D8168D9DB3D15A8917F0B",
          "QualifiedName": "29814739C35945AB1E1E69611B003505E0959175892D8168D9DB3D15A8917F0B.MyHistorian",
          "DisplayName": "MyHistorian (App Store Connect - A Person)"
        }
      ]
    }

## Using Local Data Core and Windows Authentication

If your application will run on an isolated network that cannot access the App Store, you can disable App Store mode and configure your application to connect to a local Data Core or Gestalt instead, using Windows authentication:

1. In the Visual Studio properties for the project, enable Windows authentication.
2. In `Web.config`, set the `application:useAppStore` setting in the `appSettings` section to `false`.
3. In `Web.config`, set the `dataCore:endpoint` setting in the `appSettings` section to the local Data Core or Gestalt URL that you want to query, e.g. `http://myserver/gestalt`.

If you are running the application in IIS instead of IIS Express, you must use IIS Manager to enable Windows authentication for the application.  If you are running the application in IIS and you want to call Data Core using the authenticated caller's credentials, you can enable impersonation using IIS Manager.

## Notes

* You can use the static `Startup.AppStoreMode` property throughout your application to determine if you are running in App Store mode or not (e.g. to decide if you want to make an App Store API call to bill the user).
* To persist OAuth access tokens, create your own implementation of the `IOAuthTokenStore` interface and replace the `IOAuthTokenStore` registration at the start of `ConfigureAppStoreAuthentication` in `Startup.Auth.cs`.
* When running in App Store mode, the Intelligent Plant login provider middleware is used to automatically handle sign-in and sign-out requests.  You can remove this middleware registration in `Startup.Auth.cs` if you intend to use ASP.NET Identity to manage user accounts for your application.
* By default, session cookies are not persisted between browser sessions when using the Intelligent Plant login manager middleware.  You can change this behaviour in `Startup.Auth.cs`.