//-----------------------------------------------------------------------
// <copyright file="VitaDockConsumer.cs" company="Karsten Olsen">
//     Copyright (c) Karsten Olsen. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Example.OAuth
{
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth;
	using MedisanaSpace.Model;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using Validation;

	/// <summary>
	/// A consumer capable of communicating with VitaDock Data APIs.
	/// <para>
	/// Examining the DotNetOpenAuth framework and in particular the Ouath v1.0 consumer classes
	/// it becomes clear that the provided library cannot work with VitaDock api without some
	/// adoptation. DotNetOpenAuth consumer provides a good infrastructure for the Oauth authorization
	/// processes and message handling. in particular the WebConsumer with HMac-Sha1 signing seems
	/// to be a close fit, but VitaDock does have the following requirements that is not provided:
	/// </para><para>
	/// 1) Nonce: should be 36 characters and not 8 as provided by the default MessageHandler
	/// But is defined as customizable.
	/// Default value set in
	/// C:\Users\Karsten\Documents\GitHub\DotNetOpenAuth\src\DotNetOpenAuth.OAuth.Consumer\OAuth\OAuth1HttpMessageHandlerBase.cs
	/// Could be customized by implementing own Consumer and by overriding the CreateMessageHandler method
	/// where the OAuth1HttpMessageHandlerBase.NonceLength should be set to 36 characters
	/// </para><para>
	/// 2) HMAC-SHA256 The framework provided a HMAC-SHA1 signing method, but not SHA256. It is easily achieved
	/// by implementing HmacSha256SigningBindingElement.cs by simply copying from the existing HmacSha1SigningBindingElement
	/// and adapting a few lines
	/// </para><para>
	/// 3) Timestamp VitaDock requires timestamps to be specified in milliseconds and not seconds since 1.jan 1970.
	/// The DotNetOpenAuth framework implements this funtionality in OAuth1HttpMessageHandlerBase.
	/// It does this in ToTimeStamp (Which is private) and which cannot be overriden. But since
	/// to TimeStamp is only called from inside the messagehandler base it could be achieved by overriding other
	/// methods. It could be achieved be overriding the ApplyAuthorization method.
	/// </para><para>
	/// 4) No realm. VitaDock do not expect an Oauth_real parameter. It might be nescessary to find a way to remove this???
	/// 5) No callback url - remember unclean modifications to library.
	/// </para><para>
	/// Ideas:
	/// 1) Check response
	/// 2) Check to see if generated baseparameter/signature string could be used in java program and if generated
	///  strings from java could be used in net - is it just the encryption algorithm that is wrong?
	/// 3) Check to see if content length really should be 0 on first request - what does the java program send?
	///  examine http requeste object in java/.net before sending any differences in headers or content?
	/// 4) Use wireshark to see what is actually being sent from the .net program
	/// </para>
	/// </summary>
	public class VitaDockConsumer : Consumer
	{
		private const string VitaDockBaseAuthUrl = "https://vitacloud.medisanaspace.com";

		private const string VitaDockThermoString = "/data/thermodocks";

		/// <summary>
		/// The description of VitaDock's OAuth protocol URIs for use with actually reading/writing
		/// a user's private VitaDock data.
		/// </summary>
		private static readonly ServiceProviderDescription ServiceDescription = new ServiceProviderDescription(
			VitaDockBaseAuthUrl + "/auth/unauthorizedaccesses",
			VitaDockBaseAuthUrl + "/desiredaccessrights/request",
			VitaDockBaseAuthUrl + "/auth/accesses/verify");

		public VitaDockConsumer()
		{
			this.ServiceProvider = ServiceDescription;
			this.ConsumerKey = ConfigurationManager.AppSettings["VitaDockConsumerKey"];
			this.ConsumerSecret = ConfigurationManager.AppSettings["VitaDockConsumerSecret"];
			this.TemporaryCredentialStorage = HttpContext.Current != null
												  ? (ITemporaryCredentialStorage)new CookieTemporaryCredentialStorage()
												  : new MemoryTemporaryCredentialStorage();
		}

		/// <summary>
		/// Requests authorization from VitaDock to access data.
		/// </summary>
		/// <param name="callback">The callback Uri</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// A task that completes with the asynchronous operation.
		/// </returns>
		public Task<Uri> RequestUserAuthorizationAsync(Uri callback, CancellationToken cancellationToken = default(CancellationToken))
		{
			return this.RequestUserAuthorizationAsync(callback, null, cancellationToken);
		}

		/// <summary>
		/// Creates a message handler that conforms to the VitaDock API and which can
		/// sign outbound requests with a previously obtained authorization.
		/// </summary>
		/// <param name="accessToken">The access token to authorize outbound HTTP requests with.</param>
		/// <param name="innerHandler">The inner handler that actually sends the HTTP message on the network.</param>
		/// <returns>
		/// A message handler.
		/// </returns>
		/// <remarks>
		/// Use this to Creates a messagehandler that conforms that
		/// 1) uses HMAC-SHA256 for signing messages
		/// 2) which signs with a Nonce of 36 characters.
		/// 3) which does not send the oauth realm parameter
		/// 4) which does not include the oauth callback parameter
		/// 5) which does not require the oauth calbackconfirm parameter in response
		/// 6) and which uses timestamps that are specifying millisecond rather than seconds
		/// </remarks>
		public override OAuth1HttpMessageHandlerBase CreateMessageHandler(AccessToken accessToken = default(AccessToken), HttpMessageHandler innerHandler = null)
		{
			Verify.Operation(this.ConsumerKey != null, "Required Property ConsumerKey Not Yet Preset", "ConsumerKey");

			innerHandler = innerHandler ?? this.HostFactories.CreateHttpMessageHandler();
			VitaDockMessageHandler handler = new VitaDockMessageHandler(innerHandler);

			handler.ConsumerKey = this.ConsumerKey;
			handler.ConsumerSecret = this.ConsumerSecret;
			handler.AccessToken = accessToken.Token;
			handler.AccessTokenSecret = accessToken.Secret;

			return handler;
		}

		public async Task<List<Thermodock>> GetLast10DaysOfThermoDocksDataAsync(AccessToken accessToken, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (string.IsNullOrEmpty(accessToken.Token))
			{
				throw new ArgumentNullException("accessToken.Token");
			}

			using (var httpClient = this.CreateHttpClient(accessToken))
			{
				httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

				using (var response = await httpClient.GetAsync(
					VitaDockBaseAuthUrl + VitaDockThermoString
					+ "?max=50&date_since=" + System.DateTime.UtcNow.AddDays(-10).ToString("yyyy-MM-dd"),
					cancellationToken))
				{
					response.EnsureSuccessStatusCode();
					string jsonString = await response.Content.ReadAsStringAsync();
					var json = JArray.Parse(jsonString);
					var ser = JsonSerializer.Create();

					var result = new List<Thermodock>();
					ser.Populate(json.CreateReader(), result);

					return result;
				}
			}
		}

		/// <summary>
		/// Prepares an OAuth message that begins an authorization request that will
		/// redirect the user to the VitaDock website to provide that authorization.
		/// </summary>
		/// <param name="callback">The absolute URI that the Service Provider should redirect the
		/// User Agent to upon successful authorization, or <c>null</c> to signify an out of band return.</param>
		/// <param name="requestParameters">Extra parameters to add to the request token message.  Optional.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The URL to direct the user agent to for user authorization.
		/// </returns>
		public async new Task<Uri> RequestUserAuthorizationAsync(Uri callback = null, IEnumerable<KeyValuePair<string, string>> requestParameters = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			// Requires.NotNull(callback, "callback"); - VitaDock API does not work if this is present
			Verify.Operation(this.ConsumerKey != null, "ConsumerKey is not preset", "ConsumerKey");
			Verify.Operation(this.TemporaryCredentialStorage != null, "TemporaryCredentialStorage is not preset", "TemporaryCredentialStorage");
			Verify.Operation(this.ServiceProvider != null, "ServiceProvider is not preset", "ServiceProvider");

			// Obtain temporary credentials before the redirect.
			using (var client = this.CreateHttpClient(new AccessToken()))
			{
				var requestUri = new UriBuilder(this.ServiceProvider.TemporaryCredentialsRequestEndpoint);

				// VitaDock specifies that callback paramter must not be sent
				OauthHelperUtility.AppendQueryArgs(requestUri, requestParameters);
				var request = new HttpRequestMessage(this.ServiceProvider.TemporaryCredentialsRequestEndpointMethod, requestUri.Uri);
				using (var response = await client.SendAsync(request, cancellationToken))
				{
					response.EnsureSuccessStatusCode();
					cancellationToken.ThrowIfCancellationRequested();

					// Parse the response and ensure that it meets the requirements of the OAuth 1.0 spec.
					string content = await response.Content.ReadAsStringAsync();
					var responseData = HttpUtility.ParseQueryString(content);
					string identifier = responseData["oauth_token"];
					string secret = responseData["oauth_token_secret"];
					Validation.Requires.That(!string.IsNullOrEmpty(identifier), "oauth_token_secret", "Token was missing in message");
					Validation.Requires.That(!string.IsNullOrEmpty(secret), "oauth_token_secret", "Token was missing in message");

					// Save the temporary credential we received so that after user authorization
					// we can use it to obtain the access token.
					cancellationToken.ThrowIfCancellationRequested();
					this.TemporaryCredentialStorage.SaveTemporaryCredential(identifier, secret);

					// Construct the user authorization URL so our caller can direct a browser to it.
					var authorizationEndpoint = new UriBuilder(this.ServiceProvider.ResourceOwnerAuthorizationEndpoint);
					authorizationEndpoint.AppendQueryArgument("oauth_token", identifier);
					return authorizationEndpoint.Uri;
				}
			}
		}

		public async Task<string> LogBloodSugarTestData(AccessToken accessToken, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (string.IsNullOrEmpty(accessToken.Token))
			{
				throw new ArgumentNullException("accessToken.Token");
			}

			using (var httpClient = this.CreateHttpClient(accessToken))
			{
				var request = new HttpRequestMessage();
				request.Method = HttpMethod.Post;
				request.RequestUri = new Uri(VitaDockBaseAuthUrl + VitaDockThermoString + "/generate");

				using (var response = await httpClient.SendAsync(request, cancellationToken))
				{
					response.EnsureSuccessStatusCode();
					string guid = await response.Content.ReadAsStringAsync();
					return guid;
				}
			}
		}
	}
}