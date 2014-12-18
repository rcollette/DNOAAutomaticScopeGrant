namespace OAuthClient
{
    using DotNetOpenAuth.OAuth2;
    using System;

    internal static class AuthorizationServer
    {
        private static readonly AuthorizationServerDescription _authorizationServerDescription;
        private static readonly WebServerClient _client;

        static AuthorizationServer()
        {
            /// <summary>
            /// The details about the sample OAuth-enabled WCF service that this sample client calls into.
            /// </summary>
            _authorizationServerDescription = new AuthorizationServerDescription
            {
                //These will need to be made configurable.
                TokenEndpoint = new Uri("http://localhost:50172/OAuth/Token"),
                AuthorizationEndpoint = new Uri("http://localhost:50172/OAuth/Authorize"),
            };
            //The consumer name and secret must be negotiated/back channeled between the authorization provider and the client.
            _client = new WebServerClient(_authorizationServerDescription, "sampleconsumer", "samplesecret");
        }

        public static AuthorizationServerDescription Description
        {
            get
            {
                return _authorizationServerDescription;
            }
        }

        public static WebServerClient Client
        {
            get
            {
                return _client;
            }
        }
    }
}