namespace OAuthResourceServer
{
    using Code;
    using System.Security.Principal;
    using System.ServiceModel;

    /// <summary>
    /// The WCF service API.
    /// </summary>
    /// <remarks>
    /// Note how there is no code here that is bound to OAuth or any other
    /// credential/authorization scheme.  That's all part of the channel/binding elsewhere.
    /// And the reference to OperationContext.Current.ServiceSecurityContext.PrimaryIdentity
    /// is the user being impersonated by the WCF client.
    /// In the OAuth case, it is the user who authorized the OAuth access token that was used
    /// to gain access to the service.
    /// </remarks>
    public class DataApi : IDataApi
    {
        private IIdentity User
        {
            get { return OperationContext.Current.ServiceSecurityContext.PrimaryIdentity; }
        }

        public UserProfile GetUserProfile()
        {
            UserProfile profile = new UserProfile() { FirstName = "First", LastName = "Last", EmailAddress = this.User.Name, PersonId = 1, CompanyId = 2 };
            return profile;
        }
    }
}