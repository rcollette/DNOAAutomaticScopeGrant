﻿//-----------------------------------------------------------------------
// <copyright file="ConsumerBase.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth {
	using System;
	using System.Collections.Generic;
	using System.Net;
	using System.Web;
	using DotNetOAuth.ChannelElements;
	using DotNetOAuth.Messages;
	using DotNetOAuth.Messaging;
	using DotNetOAuth.Messaging.Bindings;

	/// <summary>
	/// Base class for <see cref="WebConsumer"/> and <see cref="DesktopConsumer"/> types.
	/// </summary>
	public class ConsumerBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="ConsumerBase"/> class.
		/// </summary>
		/// <param name="serviceDescription">The endpoints and behavior of the Service Provider.</param>
		/// <param name="tokenManager">The host's method of storing and recalling tokens and secrets.</param>
		protected ConsumerBase(ServiceProviderDescription serviceDescription, ITokenManager tokenManager) {
			if (serviceDescription == null) {
				throw new ArgumentNullException("serviceDescription");
			}
			if (tokenManager == null) {
				throw new ArgumentNullException("tokenManager");
			}

			this.WebRequestHandler = new StandardWebRequestHandler();
			ITamperProtectionChannelBindingElement signingElement = serviceDescription.CreateTamperProtectionElement();
			INonceStore store = new NonceMemoryStore(StandardExpirationBindingElement.DefaultMaximumMessageAge);
			this.Channel = new OAuthChannel(signingElement, store, tokenManager, new OAuthConsumerMessageTypeProvider(tokenManager), this.WebRequestHandler);
			this.ServiceProvider = serviceDescription;
		}

		/// <summary>
		/// Gets or sets the Consumer Key used to communicate with the Service Provider.
		/// </summary>
		public string ConsumerKey { get; set; }

		/// <summary>
		/// Gets the Service Provider that will be accessed.
		/// </summary>
		public ServiceProviderDescription ServiceProvider { get; private set; }

		/// <summary>
		/// Gets the persistence store for tokens and secrets.
		/// </summary>
		public ITokenManager TokenManager {
			get { return this.Channel.TokenManager; }
		}

		/// <summary>
		/// Gets or sets the object that processes <see cref="HttpWebRequest"/>s.
		/// </summary>
		/// <remarks>
		/// This defaults to a straightforward implementation, but can be set
		/// to a mock object for testing purposes.
		/// </remarks>
		internal IWebRequestHandler WebRequestHandler { get; set; }

		/// <summary>
		/// Gets or sets the channel to use for sending/receiving messages.
		/// </summary>
		internal OAuthChannel Channel { get; set; }

		/// <summary>
		/// Creates a web request prepared with OAuth authorization 
		/// that may be further tailored by adding parameters by the caller.
		/// </summary>
		/// <param name="endpoint">The URL and method on the Service Provider to send the request to.</param>
		/// <param name="accessToken">The access token that permits access to the protected resource.</param>
		/// <returns>The initialized WebRequest object.</returns>
		public WebRequest PrepareAuthorizedRequest(MessageReceivingEndpoint endpoint, string accessToken) {
			IDirectedProtocolMessage message = this.CreateAuthorizingMessage(endpoint, accessToken);
			HttpWebRequest wr = this.Channel.InitializeRequest(message);
			return wr;
		}

		/// <summary>
		/// Creates a web request prepared with OAuth authorization 
		/// that may be further tailored by adding parameters by the caller.
		/// </summary>
		/// <param name="endpoint">The URL and method on the Service Provider to send the request to.</param>
		/// <param name="accessToken">The access token that permits access to the protected resource.</param>
		/// <returns>The initialized WebRequest object.</returns>
		/// <exception cref="WebException">Thrown if the request fails for any reason after it is sent to the Service Provider.</exception>
		public Response PrepareAuthorizedRequestAndSend(MessageReceivingEndpoint endpoint, string accessToken) {
			IDirectedProtocolMessage message = this.CreateAuthorizingMessage(endpoint, accessToken);
			HttpWebRequest wr = this.Channel.InitializeRequest(message);
			return this.WebRequestHandler.GetResponse(wr);
		}

		/// <summary>
		/// Begins an OAuth authorization request and redirects the user to the Service Provider
		/// to provide that authorization.
		/// </summary>
		/// <param name="callback">
		/// An optional Consumer URL that the Service Provider should redirect the 
		/// User Agent to upon successful authorization.
		/// </param>
		/// <param name="requestParameters">Extra parameters to add to the request token message.  Optional.</param>
		/// <param name="redirectParameters">Extra parameters to add to the redirect to Service Provider message.  Optional.</param>
		/// <param name="token">The request token that must be exchanged for an access token after the user has provided authorization.</param>
		/// <returns>The pending user agent redirect based message to be sent as an HttpResponse.</returns>
		protected internal Response RequestUserAuthorization(Uri callback, IDictionary<string, string> requestParameters, IDictionary<string, string> redirectParameters, out string token) {
			// Obtain an unauthorized request token.
			var requestToken = new GetRequestTokenMessage(this.ServiceProvider.RequestTokenEndpoint) {
				ConsumerKey = this.ConsumerKey,
			};
			requestToken.AddNonOAuthParameters(requestParameters);
			var requestTokenResponse = this.Channel.Request<GrantRequestTokenMessage>(requestToken);
			this.TokenManager.StoreNewRequestToken(requestToken, requestTokenResponse);

			// Request user authorization.
			var requestAuthorization = new DirectUserToServiceProviderMessage(this.ServiceProvider.UserAuthorizationEndpoint) {
				Callback = callback,
				RequestToken = requestTokenResponse.RequestToken,
			};
			requestAuthorization.AddNonOAuthParameters(redirectParameters);
			token = requestAuthorization.RequestToken;
			return this.Channel.Send(requestAuthorization);
		}

		/// <summary>
		/// Creates a web request prepared with OAuth authorization 
		/// that may be further tailored by adding parameters by the caller.
		/// </summary>
		/// <param name="endpoint">The URL and method on the Service Provider to send the request to.</param>
		/// <param name="accessToken">The access token that permits access to the protected resource.</param>
		/// <returns>The initialized WebRequest object.</returns>
		protected internal AccessProtectedResourceMessage CreateAuthorizingMessage(MessageReceivingEndpoint endpoint, string accessToken) {
			if (endpoint == null) {
				throw new ArgumentNullException("endpoint");
			}
			if (String.IsNullOrEmpty(accessToken)) {
				throw new ArgumentNullException("accessToken");
			}

			AccessProtectedResourceMessage message = new AccessProtectedResourceMessage(endpoint) {
				AccessToken = accessToken,
				ConsumerKey = this.ConsumerKey,
			};

			return message;
		}

		/// <summary>
		/// Exchanges a given request token for access token.
		/// </summary>
		/// <param name="requestToken">The request token that the user has authorized.</param>
		/// <returns>The access token assigned by the Service Provider.</returns>
		protected GrantAccessTokenMessage ProcessUserAuthorization(string requestToken) {
			var requestAccess = new GetAccessTokenMessage(this.ServiceProvider.AccessTokenEndpoint) {
				RequestToken = requestToken,
				ConsumerKey = this.ConsumerKey,
			};
			var grantAccess = this.Channel.Request<GrantAccessTokenMessage>(requestAccess);
			this.TokenManager.ExpireRequestTokenAndStoreNewAccessToken(this.ConsumerKey, requestToken, grantAccess.AccessToken, grantAccess.TokenSecret);
			return grantAccess;
		}
	}
}
