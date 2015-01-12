namespace OAuthAuthorizationServer.Controllers
{
    using OAuthAuthorizationServer.Code;
    using OAuthAuthorizationServer.Models;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using System.Web.Security;

    [HandleError]
    public class AccountController : Controller
    {
        // **************************************
        // URL: /Account/LogOn
        // **************************************
        public ActionResult LogOn()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> LogOn(LogOnModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                // Here, we fake user authentication.  The original DotNetOpenAuthExamples had the user
                // enter an OpenId URL, authenticate with the open id provider and then return to the client.
                // We want to be the authentication authority in this example.   In reality, we would probably
                // be using a MembershipProvider instance or other form of authentication.
                // Make sure we have a user account for this guy.
                string identifier = model.UserName; // convert to string so LinqToSQL expression parsing works.
                if (MvcApplication.DataContext.Users.FirstOrDefault(u => u.OpenIDClaimedIdentifier == identifier) == null)
                {
                    MvcApplication.DataContext.Users.InsertOnSubmit(new User
                    {
                        OpenIDFriendlyIdentifier = identifier,
                        OpenIDClaimedIdentifier = identifier,
                    });
                }
                // The FormsAuthentication cookie is set in the current response context.
                FormsAuthentication.SetAuthCookie(identifier, false);
                // The returnUrl contains the URL that was originally requested before LogOn was prompted.
                // In this case it is the request to the authorization server where the user responds yes/no to the request for the resource (ie.the user's data)
                return this.Redirect(returnUrl ?? Url.Action("Index", "Home"));
            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // **************************************
        // URL: /Account/LogOff
        // **************************************
        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home");
        }
    }
}