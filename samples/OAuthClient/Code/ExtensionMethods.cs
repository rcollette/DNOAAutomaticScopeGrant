using DotNetOpenAuth.OAuth2;
using OAuthClient.SampleResourceServer;
using System;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using System.Threading.Tasks;

namespace OAuthClient
{
    public static class ExtensionMethods
    {
        //Unfortunately, a method like the following is not possible because TChannel cannot be inferred and
        //there is not a generic ClientBase class.
        //public static async Task<T> CallAsync<TClient, TChannel, T>(
        //   this TClient wcfClient,
        //   Func<TClient, Task<T>> predicate,
        //   IAuthorizationState authorization,
        //   CancellationToken cancellationToken)
        //   where TClient : ClientBase<TChannel>
        //   where TChannel : class

        internal static async Task<T> CallAsync<T>(
            this ProfileServiceClient wcfClient,
            Func<ProfileServiceClient, T> predicate,
            IAuthorizationState authorization,
            CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new InvalidOperationException("No access token!");
            }

            // Refresh the access token if it expires and if its lifetime is too short to be of use.
            if (authorization.AccessTokenExpirationUtc.HasValue)
            {
                await AuthorizationServer.Client.RefreshAuthorizationAsync(authorization, TimeSpan.FromSeconds(30));
            }
            var httpRequest = (HttpWebRequest)WebRequest.Create(wcfClient.Endpoint.Address.Uri);
            ClientBase.AuthorizeRequest(httpRequest, authorization.AccessToken);

            var httpDetails = new HttpRequestMessageProperty();
            httpDetails.Headers[HttpRequestHeader.Authorization] = httpRequest.Headers[HttpRequestHeader.Authorization];

            using (var scope = new OperationContextScope(wcfClient.InnerChannel))
            {
                OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpDetails;
                bool isError = true;
                try
                {
                    T result = predicate(wcfClient);
                    // If there is an error on the channel, the close call will throw an exception.
                    wcfClient.Close();
                    isError = false;
                    return result;
                }
                finally
                {
                    // If we have an error on the channel, we cannot close the channel so we abort.
                    // Exceptions that occur will still be raised unmodified.
                    if (isError)
                    {
                        wcfClient.Abort();
                    }
                }
            }
        }
    }
}