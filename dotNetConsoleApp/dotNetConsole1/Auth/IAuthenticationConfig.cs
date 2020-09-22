namespace dotNetConsole1.Auth
{
    public interface IAuthenticationConfig
    {
        string ApiUrl { get; set; }
        string Authority { get; }
        string CertificateName { get; set; }
        string ClientId { get; set; }
        string ClientSecret { get; set; }
        string Instance { get; set; }
        string Tenant { get; set; }

        AuthenticationConfig ReadFromJsonFile(string path);
    }
}