using System;

namespace dotNetConsole.Auth
{
    public interface IAuthenticationConfig
    {
        /// <summary>
        /// Graph API endpoint, could be public Azure (default) or a Sovereign cloud (US government, etc ...)
        /// </summary>
        string ApiUrl { get; set; }
        /// <summary>
        /// URL of the authority
        /// </summary>
        string Authority { get; }
        /// <summary>
        /// Name of a certificate in the user certificate store
        /// </summary>
        /// <remarks>Daemon applications can authenticate with AAD through two mechanisms: ClientSecret
        /// (which is a kind of application password: the property above)
        /// or a certificate previously shared with AzureAD during the application registration 
        /// (and identified by this CertificateName property)
        /// <remarks> 
        string CertificateName { get; set; }
        /// <summary>
        /// Guid used by the application to uniquely identify itself to Azure AD
        /// </summary>
        string ClientId { get; set; }
        /// <summary>
        /// Client secret (application password)
        /// </summary>
        /// <remarks>Daemon applications can authenticate with AAD through two mechanisms: ClientSecret
        /// (which is a kind of application password: this property)
        /// or a certificate previously shared with AzureAD during the application registration 
        /// (and identified by the CertificateName property belows)
        /// <remarks> 
        string ClientSecret { get; set; }
        /// <summary>
        /// instance of Azure AD, for example public Azure or a Sovereign cloud (Azure China, Germany, US government, etc ...)
        /// </summary>
        string Instance { get; set; }
        /// <summary>
        /// The Tenant is:
        /// - either the tenant ID of the Azure AD tenant in which this application is registered (a guid)
        /// or a domain name associated with the tenant
        /// - or 'organizations' (for a multi-tenant application)
        /// </summary>
        string Tenant { get; set; }

        AuthenticationConfig ReadFromJsonFile(string path);
    }
}
