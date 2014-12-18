namespace OAuthClient
{
    using DotNetOpenAuth.Messaging;
    using DotNetOpenAuth.OAuth2;
    using OAuthClient.SampleResourceServer;
    using System;
    using System.Net;
    using System.Net.Http;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Security;
    using System.Web.SessionState;

    /// <summary>
    /// Receives an OAuth 2.0 authorization to retrieve UserProfile information, retrieves it and logs the user on.
    /// </summary>
    public class Authorize : HttpTaskAsyncHandler, IRequiresSessionState
    {
        /// <summary>
        /// Gets or sets the authorization details for the logged in user.
        /// </summary>
        /// <value>The authorization details.</value>
        /// <remarks>
        /// Because this is a sample, we simply store the authorization information in memory with the user session.
        /// A real web app should store at least the access and refresh tokens in this object in a database associated with the user.
        /// </remarks>

        public override bool IsReusable
        {
            get
            {
                return true;
            }
        }

        public override async Task ProcessRequestAsync(HttpContext context)
        {
            var response = context.Response;
            var authorization =
                await AuthorizationServer.Client.ProcessUserAuthorizationAsync(
                    new HttpRequestWrapper(context.Request),
                    response.ClientDisconnectedToken);
            if (authorization != null)
            {
                await RetrieveResourcesAsync(context, authorization);
            }
            else
            {
                await RequestAuthorizationAsync(context);
            }
        }

        private async Task RetrieveResourcesAsync(HttpContext context, IAuthorizationState authorization)
        {
            var response = context.Response;
            string statusMessage = null;
            //Get the user's profile information
            try
            {
                UserProfile userProfile = await this.CallServiceAsync(client => client.GetUserProfile(), authorization, response.ClientDisconnectedToken);
                UserProfileSessionState.Current = userProfile;
                FormsAuthentication.SetAuthCookie(userProfile.EmailAddress, false);
                //Add anything else you need in the response here (ex. JSON data);
                return;
            }
            catch (SecurityAccessDeniedException ex)
            {
                statusMessage = ex.Message;
            }
            catch (MessageSecurityException ex)
            {
                statusMessage = ex.Message;
            }
            response.ClearContent();
            response.ClearHeaders();
            if (statusMessage != null)
            {
            }
            response.StatusCode = (int)HttpStatusCode.Unauthorized;
        }

        private async Task RequestAuthorizationAsync(HttpContext context)
        {
            //Specify the data set we are requesting authorization for.
            string[] scopes = { "UserProfile" };
            //Build a response that will redirect the client to the authorization server for logon.
            HttpResponseMessage request =
                 await AuthorizationServer.Client.PrepareRequestUserAuthorizationAsync(scopes, cancellationToken: context.Response.ClientDisconnectedToken);
            //Send the response to the client user agent.
            await request.SendAsync();
            context.Response.End();
        }

        private async Task<T> CallServiceAsync<T>(Func<DataApiClient, T> predicate, IAuthorizationState authorization, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new InvalidOperationException("No access token!");
            }

            var wcfClient = new DataApiClient();

            // Refresh the access token if it expires and if its lifetime is too short to be of use.
            if (authorization.AccessTokenExpirationUtc.HasValue)
            {
                if (await AuthorizationServer.Client.RefreshAuthorizationAsync(authorization, TimeSpan.FromSeconds(30)))
                {
                }
            }

            var httpRequest = (HttpWebRequest)WebRequest.Create(wcfClient.Endpoint.Address.Uri);
            ClientBase.AuthorizeRequest(httpRequest, authorization.AccessToken);

            var httpDetails = new HttpRequestMessageProperty();
            httpDetails.Headers[HttpRequestHeader.Authorization] = httpRequest.Headers[HttpRequestHeader.Authorization];
            using (var scope = new OperationContextScope(wcfClient.InnerChannel))
            {
                OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpDetails;
                return predicate(wcfClient);
            }
        }
    }
}