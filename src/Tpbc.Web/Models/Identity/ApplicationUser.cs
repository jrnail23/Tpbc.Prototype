using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace Tpbc.Web.Models.Identity
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser :
        IdentityUser
            <string, IdentityUserLogin<string>, IdentityUserRole<string>, IdentityUserClaim<string>>, IUser
    {
        public ApplicationUser(string userName)
        {
            UserName = userName;
        }

        public ApplicationUser()
        {
            Id = Guid.NewGuid()
                .ToString();
        }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(
            UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity =
                await
                    manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }
}