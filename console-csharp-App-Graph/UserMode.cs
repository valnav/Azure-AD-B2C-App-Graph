using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graph;
using System.Net.Http;
using Newtonsoft.Json.Linq;
namespace console_csharp_trustframeworkpolicy
{
    internal class UserMode
    {
        public static GraphServiceClient client;


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

        public static HttpRequestMessage HttpGetApps(string uri)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            AuthenticationHelper.AddHeaders(request);
            return request;
        }
        
        public static HttpRequestMessage HttpPostApp(string uri, params string[] args)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);
            AuthenticationHelper.AddHeaders(request);
            //create app
            string jsonContent = B2CAppGraph.Properties.Resources.appTemplate.Replace("#appName#", args[0]);
            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = Program.RespondAndPrint(request);
            var jsonObject = Program.GetContentAsJson(response); //JObject.Parse(response.Content.ToString());
            JToken token;
            jsonObject.TryGetValue("appId", out token);
            string appId, sPId;
            appId = token.Value<string>();
            Console.WriteLine("newly created app: {0}", appId);
            if (token != null)
            {
                //create SP
                request = new HttpRequestMessage(HttpMethod.Post, Constants.SPUri);
                AuthenticationHelper.AddHeaders(request);
                jsonContent = B2CAppGraph.Properties.Resources.servicePrincipalTemplate.Replace("#appId#", appId);
                request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                response = Program.RespondAndPrint(request);
                jsonObject = Program.GetContentAsJson(response);

                jsonObject.TryGetValue("id", out token);
                sPId = token.Value<string>();

                //create oauthPermissionGrant
                request = new HttpRequestMessage(HttpMethod.Post, Constants.SPUri);
                AuthenticationHelper.AddHeaders(request);
                jsonContent = B2CAppGraph.Properties.Resources.oAuthPermissionGrantsTemplate.Replace("#appId#", appId);
                jsonContent = jsonContent.Replace("#sPId#", sPId);
                request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                response = Program.RespondAndPrint(request);

                //patch apps
                request = new HttpRequestMessage(new HttpMethod("PATCH"), string.Format(Constants.PatchAppsUri, appId));
                AuthenticationHelper.AddHeaders(request);
                jsonContent = B2CAppGraph.Properties.Resources.updateAppTemplate;
                request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                
            }

            return request;
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
