﻿using System;
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
        
        public static void CreateFullAppUsingMSGraphAndAadGraph(string uri, string appName)
        {
            string appId = CreateAppFromMSGraph(appName);

        }

        public static void CreateFullAppUsingMSGraphOnly(string appName)
        {
            string appId = CreateAppFromMSGraph(appName);

            if (appId != null)
            {
                // create SP
                var request = new HttpRequestMessage(HttpMethod.Post, Constants.SPUri);
                AuthenticationHelper.AddHeaders(request);
                var jsonContent = B2CAppGraph.Properties.Resources.servicePrincipalTemplate.Replace("#appId#", appId);
                request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var response = Program.RespondAndPrint(request);
                var jsonObject = Program.GetContentAsJson(response);
                jsonObject.TryGetValue("id", out JToken token);
                var sPId = token.Value<string>();
                Console.WriteLine("newly created SP: {0}", sPId);

                string msGraphSPId = GetMsGraphSPId();
                Console.WriteLine("MsGraph SP: {0}", msGraphSPId);

                //create oauthPermissionGrant
                request = new HttpRequestMessage(HttpMethod.Post, Constants.OAuthPermissionGrantsUri);
                AuthenticationHelper.AddHeaders(request);
                jsonContent = B2CAppGraph.Properties.Resources.oAuthPermissionGrantsTemplate;
                jsonContent = jsonContent
                    .Replace("#spId#", sPId)
                    .Replace("#MSGraphSPID#", msGraphSPId);
                request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                response = Program.RespondAndPrint(request);
            }
        }

        private static string CreateAppFromMSGraph(string appName)
        {
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

        private static string GetMsGraphSPId()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, Constants.SPUri);
            AuthenticationHelper.AddHeaders(request);
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
