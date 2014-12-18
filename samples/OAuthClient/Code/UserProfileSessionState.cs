namespace OAuthClient
{
    using OAuthClient.SampleResourceServer;
    using System.Web;

    internal static class UserProfileSessionState
    {
        internal static UserProfile Current
        {
            get
            {
                return (UserProfile)HttpContext.Current.Session["UserProfile"];
            }

            set
            {
                HttpContext.Current.Session["UserProfile"] = value;
            }
        }
    }
}