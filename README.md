# AzureBilling
An ASP.NET Core project for fetching Azure billing details.

# Usage
This is an example project for connecting to Azure Active Directory (AAD) to retrieve an access token and using that token to get rate card and billing information about resource groups in a subscription.

The following parameters are needed to connect.

```C#
        // This is the ID of the client being used to access the AAD, it can be found as Application ID under Azure Active Directory > App registrations > YOUR_APP_NAME
        // This app is created as part of the process in https://github.com/Azure-Samples/billing-dotnet-ratecard-api
        private static string clientId = "";
        // This is the key for the client ID above, it can be found as Application ID under Azure Active Directory > App registrations > YOUR_APP_NAME > Keys
        // This key is only visible when first created so make sure you note it down then!
        private static string clientSecret = "";
        // This is the ID of the AAD to be accessed, it can be found as Directory ID under Properties in the Azure Active Directory Blade in the portal
        private static string tennantId = "";
        private static string resource = "https://management.core.windows.net/";
        private static string authority = "https://login.windows.net";
        private static string billingService = "https://management.azure.com";
        // This is the ID of the subscription that you want to view billing information for, it can be found in the Subscriptions blade of the portal
        private static string subscriptionId = "";
```

This is based on these example <a href="https://github.com/Azure-Samples/billing-dotnet-ratecard-api/tree/master/ConsoleApp-Billing-RateCard/ConsoleProj">rate card</a> and <a href="https://github.com/Azure-Samples/billing-dotnet-usage-api/tree/master/ConsoleApp-Billing-Usage/ConsoleProj">billing</a> projects.
