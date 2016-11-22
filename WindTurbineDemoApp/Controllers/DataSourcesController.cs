using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;

using Microsoft.AspNet.Identity;

namespace WindTurbineDemoApp.Controllers
{
    /// <summary>
    /// Web API controller that will query Data Core to get the available data sources.
    /// </summary>
    [Authorize]
    public class DataSourcesController : ApiController
    {

        /// <summary>
        /// Gets the available data sources from Data Core.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token for the request.</param>
        /// <returns>
        /// The content of a successful request will be an object describing the caller and the data sources that the 
        /// user has authorized the application to access.
        /// </returns>
        [HttpGet]
        [Route("~/")]
        [ResponseType(typeof(GetDataSourcesResponse))]
        public async Task<IHttpActionResult> GetDataSources(CancellationToken cancellationToken)
        {
            var owinContext = Request.GetOwinContext();
            var dataCoreConnectionSettings = owinContext.GetDataCoreConnectionSettings();
            var dataCoreClient = new DataCore.Client.DataCoreHttpClient(dataCoreConnectionSettings);

            var dataSources = await dataCoreClient.GetDataSourcesAsync(cancellationToken).ConfigureAwait(false);
            if (cancellationToken.IsCancellationRequested)
            {
                return StatusCode(System.Net.HttpStatusCode.NoContent); // 204
            }

            var result = new GetDataSourcesResponse()
            {
                AuthenticationType = owinContext.Authentication.User.Identity.AuthenticationType,
                UserName = owinContext.Authentication.User.Identity.GetUserName(),
                DataCoreUrl = dataCoreConnectionSettings.Address.ToString(),
                DataSources = dataSources.OrderBy(x => x.Name.DisplayName).Select(x => x.Name).ToArray()
            };
            return Ok(result); // 200
        }


        /// <summary>
        /// Describes a response from the <see cref="GetDataSources(CancellationToken)"/> method.
        /// </summary>
        public class GetDataSourcesResponse
        {

            /// <summary>
            /// Gets or sets the authentication type that was used to authenticate the caller.
            /// </summary>
            public string AuthenticationType { get; set; }

            /// <summary>
            /// Gets or sets the caller's user name.
            /// </summary>
            public string UserName { get; set; }

            /// <summary>
            /// Gets or sets the Data Core URL that was queried.
            /// </summary>
            public string DataCoreUrl { get; set; }

            /// <summary>
            /// Gets or sets the available data sources returned by Data Core.
            /// </summary>
            public IEnumerable<DataCore.Client.Model.ComponentName> DataSources { get; set; }

        }

    }
}