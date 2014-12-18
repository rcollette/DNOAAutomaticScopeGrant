namespace OAuthResourceServer.Code
{
    using System.ServiceModel;
    using System.ServiceModel.Web;

    [ServiceContract]
    public interface IDataApi
    {
        [OperationContract, WebGet(UriTemplate = "/userprofile", ResponseFormat = WebMessageFormat.Json)]
        UserProfile GetUserProfile();
    }
}