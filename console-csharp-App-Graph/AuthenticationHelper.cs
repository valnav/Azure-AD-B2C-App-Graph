﻿using System;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Identity.Client;

namespace console_csharp_trustframeworkpolicy
{
    class AuthenticationHelper
    {
        public static string[] Scopes = 
        {
            "User.Read",
            "Directory.Read.All",
            "Directory.ReadWrite.All",
            "Directory.AccessAsUser.All"
        };

        public static PublicClientApplication IdentityClientApp = new PublicClientApplication(Constants.ClientIdForUserAuthn);
        public static string TokenForUser = null;
        public static DateTimeOffset Expiration;

        private static GraphServiceClient graphClient = null;

        // Get an access token for the given context and resourceId. An attempt is first made to 
        // acquire the token silently. If that fails, then we try to acquire the token by prompting the user.
        public static GraphServiceClient GetAuthenticatedClientForUser()
        {
            // Create Microsoft Graph client.
            try
            {
                graphClient = new GraphServiceClient(
                    "https://graph.microsoft.com/beta",
                    new DelegateAuthenticationProvider(
                        async (requestMessage) =>
                        {
                            var token = await GetTokenForUserAsync();
                            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);
                        }));
                return graphClient;
            }

            catch (Exception ex)
            {
                Debug.WriteLine("Could not create a graph client: " + ex.Message);
            }

            return graphClient;
        }

        public static void AddHeaders(HttpRequestMessage requestMessage)
        {
            if(TokenForUser == null)
            {
                Debug.WriteLine("Call GetAuthenticatedClientForUser first");
            }

            try
            {
                //Console.WriteLine($"Token: Bearer {TokenForUser}");
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", TokenForUser);                
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Could not add headers to HttpRequestMessage: " + ex.Message);
            }
        }

        /// <summary>
        /// Get Token for User.
        /// </summary>
        /// <returns>Token for user.</returns>
        public static async Task<string> GetTokenForUserAsync()
        {
            AuthenticationResult authResult;
            try
            {
                authResult = await IdentityClientApp.AcquireTokenSilentAsync(Scopes, IdentityClientApp.Users.First());
                TokenForUser = authResult.AccessToken;
            }

            catch (Exception)
            {
                if (TokenForUser == null || Expiration <= DateTimeOffset.UtcNow.AddMinutes(5))
                {
                    authResult = await IdentityClientApp.AcquireTokenAsync(Scopes);

                    TokenForUser = authResult.AccessToken;
                    Expiration = authResult.ExpiresOn;
                }
            }

            return TokenForUser;
        }

        /// <summary>
        /// Signs the user out of the service.
        /// </summary>
        public static void SignOut()
        {
            foreach (var user in IdentityClientApp.Users)
            {
                IdentityClientApp.Remove(user);
            }
            graphClient = null;
            TokenForUser = null;
        }

    }
}
