namespace OAuthAuthorizationServer.Models
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    public class LogOnModel
    {
        [Required]
        [DisplayName("User Name")]
        public string UserName { get; set; }

        [Required]
        [DisplayName("Password")]
        public string Password { get; set; }

        [DisplayName("Remember me?")]
        public bool RememberMe { get; set; }
    }
}