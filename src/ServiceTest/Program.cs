namespace ServiceTest
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var client = new SvcTest.DataApiClient();
            client.Open();
            var profile = client.GetUserProfile();
            string x = profile.EmailAddress;
        }
    }
}