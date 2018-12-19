# Manage V2 Apps for consumption in B2C

List and create applications.

Change tenantId and appId in the sample. 
appId should be created in appRegPortal


## Getting Started

### Quickstart

#### Create global administrator

* An global administrator account is required to run admin-level operations and to consent to application permissions.  (for example: admin@myb2ctenant.onmicrosoft.com)

#### Register the delegated permissions application

1. Sign in to the [Application Registration Portal](https://apps.dev.microsoft.com/) using your Microsoft account.
1. Select **Add an app**, and enter a friendly name for the application (such as **Console App for Microsoft Graph (Delegated perms)**). Click **Create**.
1. On the application registration page, select **Add Platform**. Select the **Native App** tile and save your change. The **delegated permissions** operations in this sample use permissions that are specified in the AuthenticationHelper.cs file. This is why you don't need to assign any permissions to the app on this page.
1. Assign the pp only permissions Application.ReadWrite.All
1. Note **Application Id** value for this app 

#### Build and run the sample

1. Open the sample solution in Visual Studio.
1. Build the sample.
1. Using cmd or PowerShell, navigate to <Path to sample code>/bin/Debug. Run the executable **B2CPolicyClient.exe**.
1. Sign in as a global administrator.  (for example: admin@myb2ctenant.onmicrosoft.com)
1. The output will show the results of calling the Graph API for trustFrameworkPolices.

