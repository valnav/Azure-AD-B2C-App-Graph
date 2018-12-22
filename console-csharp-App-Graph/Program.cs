using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace console_csharp_trustframeworkpolicy
{
    public class Program
    {
        static void Main(string[] args)
        {
             Console.Read();

            // validate parameters
            if (!CheckValidParameters(args))
                return;

            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            // Login as global admin of the Azure AD B2C tenant
            UserMode.LoginAsAdmin();

            // Graph client does not yet support trustFrameworkPolicy, so using HttpClient to make rest calls
            switch (args[0].ToUpper())
            {
                case "LIST":
                    UserMode.HttpGetApps(Constants.AppsUri);
                    break;
                case "CREATE1":
                    UserMode.CreateFullAppUsingMSGraphOnly(args[1]);
                    break;

                case "CREATE2":
                    UserMode.CreateFullAppUsingMSGraphAndAadGraph(Constants.AppsUri, args[1]);
                    break;

                default:
                    return;
            }
            
        }

        public static JObject GetContentAsJson(HttpResponseMessage response)
        {            
            string str = response.Content.ReadAsStringAsync().Result;
            return JObject.Parse(str);           
        }

        public static HttpResponseMessage RespondAndPrint(HttpRequestMessage request)
        {
            var response = GetResponse(request);

            return Program.Print(response);
        }

        public static HttpResponseMessage GetResponse(HttpRequestMessage request)
        {
            Program.Print(request);

            HttpClient httpClient = new HttpClient();
            Task<HttpResponseMessage> response = httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);
            return response.Result;
        }

        public static bool CheckValidParameters(string[] args)
        {
            if (Constants.ClientIdForUserAuthn.Equals("ENTER_YOUR_CLIENT_ID") ||
                Constants.Tenant.Equals("ENTER_YOUR_TENANT_NAME"))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("1. Open 'Constants.cs'");
                Console.WriteLine("2. Update 'ClientIdForUserAuthn'");
                Console.WriteLine("3. Update 'Tenant'");
                Console.WriteLine("");
                Console.WriteLine("See README.md for detailed instructions.");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("[press any key to exit]");
                Console.ReadKey();
                return false;
            }

            if (args.Length <= 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Please enter a command as the first argument.");
                Console.ForegroundColor = ConsoleColor.White;
                PrintHelp(args);
                return false;
            }

            switch (args[0].ToUpper())
            {
                case "LIST":
                    break;
                case "CREATE":
                    if (args.Length <= 1)
                    {
                        PrintHelp(args);
                        return false;
                    }
                    break;               
                case "HELP":
                    PrintHelp(args);
                    return false;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid command.");
                    Console.ForegroundColor = ConsoleColor.White;
                    PrintHelp(args);
                    return false;
            }
            return true;
        }

        public static HttpResponseMessage Print(HttpResponseMessage response)
        {        
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Error Calling the Graph API HTTP Status={0}", response.StatusCode);
            }

            Console.WriteLine(response.Headers);
            string contentString = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine(contentString);
            return response;
        }

        public static void Print(HttpRequestMessage request)
        {
            if(request != null)
            {
                Console.Write(request.Method + " ");
                Console.WriteLine(request.RequestUri);
                Console.WriteLine("");
            }
        }

        private static void PrintHelp(string[] args)
        {
            string appName = "B2CPolicyClient";
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("- Square brackets indicate optional arguments");
            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("List                     : {0} List", appName);
            Console.WriteLine("Create using MSGraphApis only                    : {0} Create1 [App Name]", appName);
            Console.WriteLine("Create using MSGraph App and AAd Graph SP Apis                     : {0} Create2 [App Name]", appName);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("");

            if(args.Length == 0)
            {
                Console.WriteLine("[press any key to exit]");
                Console.ReadKey();
            }
        }
    }
}
