namespace OAuthClient
{
    using DotNetOpenAuth.OAuth2;
    using System.Web;

    internal static class AuthorizationSessionState
    {
        internal static IAuthorizationState Current
        {
            get
            {
                return (IAuthorizationState)HttpContext.Current.Session["Authorization"];
            }

            set
            {
                HttpContext.Current.Session["Authorization"] = value;
            }
        }
    }
}