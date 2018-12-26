using System;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Identity.Client;

namespace console_csharp_trustframeworkpolicy
{
    class AADGraphAuthenticationHelper
    {
        const string ResourceId = "https://graph.windows.net/";
        public static string[] Scopes = { ResourceId + "User.Read", ResourceId + "Directory.Read.All", ResourceId + "Directory.ReadWrite.All" };

        public static PublicClientApplication IdentityClientApp = new PublicClientApplication(Constants.ClientIdForUserAuthn);
        public static string TokenForUser = null;
        public static DateTimeOffset Expiration;

        public static void AddHeaders(HttpRequestMessage requestMessage)
        {
            if (TokenForUser == null)
            {
                Debug.WriteLine("Call GetTokenForUserAsync first");
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

            catch (Exception ex)
            {
                Debug.Write(ex.Message);
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
            TokenForUser = null;
        }

    }
}
