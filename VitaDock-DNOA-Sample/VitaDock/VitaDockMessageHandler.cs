//-----------------------------------------------------------------------
// <copyright file="VitaDockMessageHandler.cs" company="Karsten Olsen">
//     Copyright (c) Karsten Olsen. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Example.OAuth
{
	using DotNetOpenAuth.OAuth;
	using System.Net.Http;
	using System.Security.Cryptography;
	using System.Text;

	/// <summary>
	/// A base class for VitaDock messagehandling. It signs
	/// outgoing HTTP requests per the OAuth 1.0 "3.4 Signature" in RFC 5849.
	/// with slight modifications for vitdadock.
	/// Nonce length 36, and using HMAC-SHA256
	/// </summary>
	/// <remarks>
	/// An implementation of http://tools.ietf.org/html/rfc5849#section-3.4
	/// </remarks>
	public class VitaDockMessageHandler : OAuth1HttpMessageHandlerBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="VitaDockMessageHandler"/> class.
		/// </summary>
		/// <param name="innerHandler">The inner handler which is responsible for processing
		/// the HTTP response messages.</param>
		public VitaDockMessageHandler(HttpMessageHandler innerHandler)
			: base(innerHandler, new DotNetOpenAuth.Messaging.TimestampMillisecondsEncoder())
		{
			this.NonceLength = 36;
		}

		/// <summary>
		/// Gets the signature method to include in the oauth_signature_method parameter.
		/// </summary>
		/// <value>
		/// The signature method.
		/// </value>
		protected override string SignatureMethod
		{
			get { return "HMAC-SHA256"; }
		}

		/// <summary>
		/// Calculates the signature for the specified buffer.
		/// </summary>
		/// <param name="signedPayload">The payload to calculate the signature for.</param>
		/// <returns>
		/// The signature.
		/// </returns>
		protected override byte[] Sign(byte[] signedPayload)
		{
			var key = Encoding.ASCII.GetBytes(this.GetConsumerAndTokenSecretString());

			using (var algorithm = new HMACSHA256(key))
			{
				return algorithm.ComputeHash(signedPayload);
			}
		}
	}
}