namespace console_csharp_trustframeworkpolicy
{
    internal class Constants
    {
        // TODO: update "ClientIdForUserAuthn" with your app guid and "Tenant" with your tenant name
        //       see README.md for instructions

        // Client ID is the application guid used uniquely identify itself to the v2.0 authentication endpoint
        public const string ClientIdForUserAuthn = "01b81dbd-dd72-4dcd-8171-be08345aa9b6";

        // Your tenant Name, for example "myb2ctenant.onmicrosoft.com"
        public const string Tenant = "cpimtestabhiagr.onmicrosoft.com";

        // leave these as-is - URIs used for auth
        public const string AuthorityUri = "https://login.microsoftonline.com/" + Tenant + "/oauth2/v2.0/token";
        public const string RedirectUriForAppAuthn = "https://login.microsoftonline.com";

        // leave these as-is - Private Preview Graph URIs for custom trust framework policy
        public const string AppsUri = "https://graph.microsoft.com/beta/applications";
        public const string PatchAppsUri = "https://graph.microsoft.com/beta/applications/{0}";
        public const string SPUri = "https://graph.microsoft.com/beta/serviceprincipals";
        public const string OAuthPermissionGrantsUri = "https://graph.microsoft.com/beta/oauth2PermissionGrants";        
    }
}
