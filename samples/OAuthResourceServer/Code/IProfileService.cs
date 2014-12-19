namespace OAuthResourceServer.Code
{
    using System.ServiceModel;

    [ServiceContract(Namespace = "http://www.acme.com/ProfileService/")]
    public interface IProfileService
    {
        [OperationContract()]
        UserProfile GetUserProfile();
    }
}