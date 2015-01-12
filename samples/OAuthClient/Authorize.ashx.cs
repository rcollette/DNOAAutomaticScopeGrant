namespace OAuthClient
{
    using DotNetOpenAuth.Messaging;
    using DotNetOpenAuth.OAuth2;
    using OAuthClient.SampleResourceServer;
    using System.Net;
    using System.Net.Http;
    using System.ServiceModel.Security;
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
            // Using DotNetOpenAuth, check if this request is the result of a redirect from the authorization server which will contain an authorization token.
            var authorization =
                await AuthorizationServer.Client.ProcessUserAuthorizationAsync(
                    new HttpRequestWrapper(context.Request),
                    response.ClientDisconnectedToken);
            // If this is not a redirect from the authorization server
            if (authorization == null)
            {
                // Request authorization from the authorization server.
                await RequestAuthorizationAsync(context);
            }
            else
            {
                // If this is an authorization redirect, get the resources.
                await RetrieveResourcesAsync(context, authorization);
            }
        }

        private async Task RetrieveResourcesAsync(HttpContext context, IAuthorizationState authorization)
        {
            var response = context.Response;
            string statusMessage = null;
            // Get the user's profile information
            try
            {
                var client = new ProfileServiceClient();
                UserProfile userProfile = await client.CallAsync(c => c.GetUserProfile(), authorization, response.ClientDisconnectedToken);
                //Do whatever you need to with the userProfile here
                FormsAuthentication.SetAuthCookie(userProfile.EmailAddress, false);
                // Add anything else you need in the response here (ex. JSON data);
                context.Response.Write("Success.");
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
            response.StatusCode = (int)HttpStatusCode.Forbidden;
        }

        private async Task RequestAuthorizationAsync(HttpContext context)
        {
            // Specify the data set we are requesting authorization for.
            string[] scopes = { "http://www.acme.com/ProfileService/IProfileService/GetUserProfile" };
            // Build a response that will redirect the client to the authorization server for logon.
            HttpResponseMessage httpResponseMessage =
                 await AuthorizationServer.Client.PrepareRequestUserAuthorizationAsync(scopes, cancellationToken: context.Response.ClientDisconnectedToken);
            // Send the response to the client user agent.
            await httpResponseMessage.SendAsync();
            context.Response.End();
        }
    }
}