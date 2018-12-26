using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graph;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using B2CAppGraph;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;
using System.Net.Http.Headers;

namespace console_csharp_trustframeworkpolicy
{
    internal class UserMode
    {
        public static GraphServiceClient client;

        public const string MSGraphAppId = "00000003-0000-0000-c000-000000000000";

        public static bool CreateGraphClient()
        {
            try
            {
                //*********************************************************************
                // setup Microsoft Graph Client for delegated user.
                //*********************************************************************
                if (Constants.ClientIdForUserAuthn != "ENTER_YOUR_CLIENT_ID")
                {
                    client = AuthenticationHelper.GetAuthenticatedClientForUser();
                    return true;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("You haven't configured a value for ClientIdForUserAuthn in Constants.cs. Please follow the Readme instructions for configuring this application.");
                    Console.ResetColor();
                    Console.ReadKey();
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Acquiring a token failed with the following error: {0}", ex.Message);
                if (ex.InnerException != null)
                {
                    //You should implement retry and back-off logic per the guidance given here:http://msdn.microsoft.com/en-us/library/dn168916.aspx
                    //InnerException Message will contain the HTTP error status codes mentioned in the link above
                    Console.WriteLine("Error detail: {0}", ex.InnerException.Message);
                }
                Console.ResetColor();
                Console.ReadKey();
                return false;
            }
        }

        public static void HttpGetApps(string uri)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            AuthenticationHelper.AddHeaders(request);
            Program.RespondAndPrint(request);
        }

        /// <summary>
        /// Creates the full application using ms graph and aad graph.
        /// This api calls all the first steps on MSGraph - app creation
        /// Calls later two on AADGraph, SP creation, permission grant
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="appName">Name of the application.</param>
        public static void CreateFullAppUsingMSGraphAndAadGraph(string appName)
        {
            string appId = CreateAppFromMSGraph(appName);

            var aadGraphToken = AADGraphAuthenticationHelper.GetTokenForUserAsync().Result;
            //Console.WriteLine($"Token: Bearer {aadGraphToken}");

            // create SP
            string spId = CreateServicePrincipal(appId, Constants.AadGraphSPUri, aadGraphToken);

            string msGraphSPId = GetMsGraphSPId(Constants.AadGraphSPUri, aadGraphToken);
            Console.WriteLine("MsGraph SP: {0}", msGraphSPId);

            //create oauthPermissionGrant
            GrantConsent(spId, msGraphSPId, Constants.AadGraphOAuthPermissionGrantsUri, aadGraphToken);
        }

        /// <summary>
        /// Creates the full application using ms graph only.
        /// This api calls all the three steps on MSGraph - app creation, SP creation, permission grant
        /// </summary>
        /// <param name="appName">Name of the application.</param>
        public static void CreateFullAppUsingMSGraphOnly(string appName)
        {
            string token = AuthenticationHelper.TokenForUser;
            string appId = CreateAppFromMSGraph(appName);

            // create SP
            string spId = CreateServicePrincipal(appId, Constants.MSGraphSPUri, token);

            string msGraphSPId = GetMsGraphSPId(Constants.MSGraphSPUri, token);
            Console.WriteLine("MsGraph SP: {0}", msGraphSPId);

            //create oauthPermissionGrant
            GrantConsent(spId, msGraphSPId, Constants.MSGraphOAuthPermissionGrantsUri, token);
        }

        /// <summary>
        /// Grants the consent.
        /// </summary>
        /// <param name="msGraphSPId">The ms graph sp identifier.</param>
        /// <param name="uri">The URI.</param>
        /// <param name="authtoken">The authtoken.</param>
        private static void GrantConsent(string spId, string msGraphSPId, string uri, string authtoken)
        {
            Console.WriteLine("----------Grant consent-----------------");

            var request = new HttpRequestMessage(HttpMethod.Post, uri);

            AddAuthZHeader(request, authtoken);

            var jsonContent = B2CAppGraph.Properties.Resources.oAuthPermissionGrantsTemplate;
            jsonContent = jsonContent
                .Replace("#spId#", spId)
                .Replace("#MSGraphSPID#", msGraphSPId);
            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = Program.RespondAndPrint(request);
        }

        /// <summary>
        /// Creates the service principal.
        /// </summary>
        /// <param name="appId">The application identifier.</param>
        /// <param name="uri">The URI.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        private static string CreateServicePrincipal(string appId, string uri, string authtoken)
        {
            Console.WriteLine("----------Creating SP-----------------");

            var request = new HttpRequestMessage(HttpMethod.Post, uri);

            AddAuthZHeader(request, authtoken);

            var jsonContent = B2CAppGraph.Properties.Resources.servicePrincipalTemplate.Replace("#appId#", appId);
            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = Program.RespondAndPrint(request);
            var jsonObject = Program.GetContentAsJson(response);
            jsonObject.TryGetValue("id", out JToken token);
            string sPId = token.Value<string>();
            Console.WriteLine("newly created SP: {0}", sPId);
            return sPId;
        }


        /// <summary>
        /// The app creation always happens in MSGraph Beta. since v2 apps can't be created in AADGraph
        /// </summary>
        /// <param name="appName">Name of the application.</param>
        /// <returns></returns>
        private static string CreateAppFromMSGraph(string appName)
        {
            Console.WriteLine("----------Creating App-----------------");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, Constants.AppsUri);
            AuthenticationHelper.AddHeaders(request);

            // create app
            var jsonContent = B2CAppGraph.Properties.Resources.appTemplate.Replace("#appName#", appName);
            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = Program.RespondAndPrint(request);
            var jsonObject = Program.GetContentAsJson(response);

            JToken token;
            jsonObject.TryGetValue("appId", out token);
            var appId = token.Value<string>();

            if (string.IsNullOrWhiteSpace(appId))
            {
                throw new Exception("App wasn't created");
            }

            Console.WriteLine("newly created app: {0}", appId);

            return appId;
        }

        private static JsonSerializerSettings GetJsonSerializerSettings()
        {
            return new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        private static void AddAuthZHeader(HttpRequestMessage requestMessage, string token)
        {
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        /// <summary>
        /// Gets the ms graph sp identifier.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="authtoken">The authtoken.</param>
        /// <returns></returns>
        private static string GetMsGraphSPId(string uri, string authtoken)
        {
            Console.WriteLine("----------Get MSGraph SP-----------------");

            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            AddAuthZHeader(request, authtoken);
            string response = Program.GetResponse(request).Content.ReadAsStringAsync().Result;

            var wrapper = JsonConvert.DeserializeObject<ODataListWrapper<List<ServicePrincipal>>>(response, GetJsonSerializerSettings());
            var spList = wrapper.Value;
            var graphSp = spList.FirstOrDefault(x => x.AppId.Equals(MSGraphAppId));
            if (graphSp == null)
            {
                throw new Exception($"Service principal for MSgraph app {MSGraphAppId} is not found in tenant");
            }

            return graphSp.Id;
        }

        public static void LoginAsAdmin()
        {
            Console.WriteLine("Login as a global admin of the tenant (example: admin@myb2c.onmicrosoft.com");
            Console.WriteLine("=============================");

            if (CreateGraphClient())
            {
                User user = client.Me.Request().GetAsync().Result;
                Console.WriteLine("Current user:    Id: {0}  UPN: {1}", user.Id, user.UserPrincipalName);
            }
        }
    }
}
