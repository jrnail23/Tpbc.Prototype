using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Tpbc.Web.Models.Identity;

namespace Tpbc.Web.Identity
{
    public class InMemoryUserStore : IUserStore<ApplicationUser>,
        IUserPasswordStore<ApplicationUser>,
        IUserLoginStore<ApplicationUser>,
        IQueryableUserStore<ApplicationUser>,
        IUserClaimStore<ApplicationUser>,
        IUserEmailStore<ApplicationUser>,
        IUserLockoutStore<ApplicationUser, string>,
        IUserPhoneNumberStore<ApplicationUser>,
        IUserTwoFactorStore<ApplicationUser, string>,
        IUserSecurityStampStore<ApplicationUser, string>
    {
        private static readonly ConcurrentDictionary<string, ApplicationUser> UserData =
            new ConcurrentDictionary<string, ApplicationUser>();
        private static readonly ConcurrentDictionary<string, IList<ApplicationUserLogin>> LoginData
            = new ConcurrentDictionary<string, IList<ApplicationUserLogin>>();
        private static readonly ConcurrentDictionary<string, IList<Claim>> ClaimsData =
            new ConcurrentDictionary<string, IList<Claim>>();
        public IQueryable<ApplicationUser> Users => UserData.Values.AsQueryable();

        public Task<IList<Claim>> GetClaimsAsync(ApplicationUser user)
        {
            return Task.FromResult(ClaimsData.ContainsKey(user.Id)
                ? ClaimsData[user.Id] ?? new List<Claim>()
                : new List<Claim>());
        }

        public Task AddClaimAsync(ApplicationUser user, Claim claim)
        {
            ClaimsData.AddOrUpdate(user.Id,
                id => new List<Claim> {claim},
                (id, claims) =>
                {
                    claims.Add(claim);
                    return claims;
                });
            return Task.FromResult(0);
        }

        public Task RemoveClaimAsync(ApplicationUser user, Claim claim)
        {
            IList<Claim> claims;
            ClaimsData.TryRemove(user.Id, out claims);

            return Task.FromResult(0);
        }

        public Task SetEmailAsync(ApplicationUser user, string email)
        {
            user.Email = email;
            return Task.FromResult(0);
        }

        public Task<string> GetEmailAsync(ApplicationUser user)
        {
            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(ApplicationUser user)
        {
            return Task.FromResult(user.EmailConfirmed);
        }

        public Task SetEmailConfirmedAsync(ApplicationUser user, bool confirmed)
        {
            user.EmailConfirmed = confirmed;
            return Task.FromResult(0);
        }

        public Task<ApplicationUser> FindByEmailAsync(string email)
        {
            return
                Task.FromResult(
                    UserData.Values.SingleOrDefault(
                        u => u.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase)));
        }

        /// <summary>
        ///     Returns the DateTimeOffset that represents the end of a user's lockout, any time in the past should be considered
        ///     not locked out.
        /// </summary>
        /// <param name="user" />
        /// <returns />
        public virtual Task<DateTimeOffset> GetLockoutEndDateAsync(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.LockoutEndDateUtc.HasValue
                ? new DateTimeOffset(DateTime.SpecifyKind(user.LockoutEndDateUtc.Value,
                    DateTimeKind.Utc))
                : new DateTimeOffset());
        }

        /// <summary>
        ///     Locks a user out until the specified end date (set to a past date, to unlock a user)
        /// </summary>
        /// <param name="user" />
        /// <param name="lockoutEnd" />
        /// <returns />
        public virtual Task SetLockoutEndDateAsync(ApplicationUser user, DateTimeOffset lockoutEnd)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            user.LockoutEndDateUtc = lockoutEnd == DateTimeOffset.MinValue
                ? new DateTime?()
                : lockoutEnd.UtcDateTime;
            return Task.FromResult(0);
        }

        /// <summary>
        ///     Used to record when an attempt to access the user has failed
        /// </summary>
        /// <param name="user" />
        /// <returns />
        public virtual Task<int> IncrementAccessFailedCountAsync(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            ++user.AccessFailedCount;
            return Task.FromResult(user.AccessFailedCount);
        }

        /// <summary>
        ///     Used to reset the account access count, typically after the account is successfully accessed
        /// </summary>
        /// <param name="user" />
        /// <returns />
        public virtual Task ResetAccessFailedCountAsync(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            user.AccessFailedCount = 0;
            return Task.FromResult(0);
        }

        /// <summary>
        ///     Returns the current number of failed access attempts.  This number usually will be reset whenever the password is
        ///     verified or the account is locked out.
        /// </summary>
        /// <param name="user" />
        /// <returns />
        public virtual Task<int> GetAccessFailedCountAsync(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.AccessFailedCount);
        }

        /// <summary>
        ///     Returns whether the user can be locked out.
        /// </summary>
        /// <param name="user" />
        /// <returns />
        public virtual Task<bool> GetLockoutEnabledAsync(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.LockoutEnabled);
        }

        /// <summary>
        ///     Sets whether the user can be locked out.
        /// </summary>
        /// <param name="user" />
        /// <param name="enabled" />
        /// <returns />
        public virtual Task SetLockoutEnabledAsync(ApplicationUser user, bool enabled)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            user.LockoutEnabled = enabled;
            return Task.FromResult(0);
        }

        /// <summary>
        ///     Returns the user associated with this login
        /// </summary>
        /// <returns />
        public virtual async Task<ApplicationUser> FindAsync(UserLoginInfo login)
        {
            if (login == null)
                throw new ArgumentNullException(nameof(login));

            var provider = login.LoginProvider;
            var key = login.ProviderKey;
            var userLogin = LoginData.Values.SelectMany(d => d)
                .FirstOrDefault(l => l.LoginProvider == provider && l.ProviderKey == key);
            ApplicationUser user;
            if (userLogin != null)
            {
                var userId = userLogin.UserId;
                user = await GetUserAggregateAsync(u => u.Id.Equals(userId));
            }
            else
                user = default(ApplicationUser);
            return user;
        }

        /// <summary>
        ///     Add a login to the user
        /// </summary>
        /// <param name="user" />
        /// <param name="login" />
        /// <returns />
        public virtual Task AddLoginAsync(ApplicationUser user, UserLoginInfo login)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (login == null)
                throw new ArgumentNullException(nameof(login));

            var instance = Activator.CreateInstance<ApplicationUserLogin>();
            instance.UserId = user.Id;
            instance.ProviderKey = login.ProviderKey;
            instance.LoginProvider = login.LoginProvider;
            var entity = instance;
            LoginData.AddOrUpdate(user.Id,
                id => new List<ApplicationUserLogin> {entity},
                (id, logins) =>
                {
                    logins.Add(entity);
                    return logins;
                });
            return Task.FromResult(0);
        }

        /// <summary>
        ///     Remove a login from a user
        /// </summary>
        /// <param name="user" />
        /// <param name="login" />
        /// <returns />
        public virtual Task RemoveLoginAsync(ApplicationUser user, UserLoginInfo login)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (login == null)
                throw new ArgumentNullException(nameof(login));
            var provider = login.LoginProvider;
            var key = login.ProviderKey;
            ApplicationUserLogin entry;
            if (true)
            {
                entry = ((IEnumerable<ApplicationUserLogin>) user.Logins).SingleOrDefault(ul =>
                {
                    if (ul.LoginProvider == provider)
                        return ul.ProviderKey == key;
                    return false;
                });
            }

            if (entry != null)
                LoginData[user.Id].Remove(entry);

            return Task.FromResult(0);
        }

        /// <summary>
        ///     Get the logins for a user
        /// </summary>
        /// <param name="user" />
        /// <returns />
        public virtual async Task<IList<UserLoginInfo>> GetLoginsAsync(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return
                await
                    Task.FromResult(
                        user.Logins.Select(l => new UserLoginInfo(l.LoginProvider, l.ProviderKey))
                            .ToList());
        }

        public Task SetPasswordHashAsync(ApplicationUser user, string passwordHash)
        {
            user.PasswordHash = passwordHash;
            return Task.FromResult(0);
        }

        public Task<string> GetPasswordHashAsync(ApplicationUser user)
        {
            return Task.FromResult(FindByIdAsync(user.Id)
                .Result.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(ApplicationUser user)
        {
            return Task.FromResult(user.PasswordHash != null);
        }

        public virtual Task SetPhoneNumberAsync(ApplicationUser user, string phoneNumber)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            user.PhoneNumber = phoneNumber;
            return Task.FromResult(0);
        }

        /// <summary>
        ///     Get a user's phone number
        /// </summary>
        /// <param name="user" />
        /// <returns />
        public virtual Task<string> GetPhoneNumberAsync(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.PhoneNumber);
        }

        /// <summary>
        ///     Returns whether the user phoneNumber is confirmed
        /// </summary>
        /// <param name="user" />
        /// <returns />
        public virtual Task<bool> GetPhoneNumberConfirmedAsync(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.PhoneNumberConfirmed);
        }

        /// <summary>
        ///     Set PhoneNumberConfirmed on the user
        /// </summary>
        /// <param name="user" />
        /// <param name="confirmed" />
        /// <returns />
        public virtual Task SetPhoneNumberConfirmedAsync(ApplicationUser user, bool confirmed)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            user.PhoneNumberConfirmed = confirmed;
            return Task.FromResult(0);
        }

        public virtual Task SetSecurityStampAsync(ApplicationUser user, string stamp)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            user.SecurityStamp = stamp;
            return Task.FromResult(0);
        }

        /// <summary>
        ///     Get the security stamp for a user
        /// </summary>
        /// <param name="user" />
        /// <returns />
        public virtual Task<string> GetSecurityStampAsync(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.SecurityStamp);
        }

        public void Dispose() {}

        public Task CreateAsync(ApplicationUser user)
        {
            UserData.AddOrUpdate(user.Id, id => user, (id, u) => user);
            return Task.FromResult(0);
        }

        public Task UpdateAsync(ApplicationUser user)
        {
            UserData[user.Id] = user;
            return Task.FromResult(0);
        }

        public Task DeleteAsync(ApplicationUser user)
        {
            ApplicationUser u;
            UserData.TryRemove(user.Id, out u);
            return Task.FromResult(0);
        }

        public Task<ApplicationUser> FindByIdAsync(string userId)
        {
            return Task.FromResult(UserData[userId]);
        }

        public Task<ApplicationUser> FindByNameAsync(string userName)
        {
            return Task.FromResult(UserData.Values.SingleOrDefault(u => u.UserName == userName));
        }

        /// <summary>
        ///     Set whether two factor authentication is enabled for the user
        /// </summary>
        /// <param name="user" />
        /// <param name="enabled" />
        /// <returns />
        public virtual Task SetTwoFactorEnabledAsync(ApplicationUser user, bool enabled)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            user.TwoFactorEnabled = enabled;
            return Task.FromResult(0);
        }

        /// <summary>
        ///     Gets whether two factor authentication is enabled for the user
        /// </summary>
        /// <param name="user" />
        /// <returns />
        public virtual Task<bool> GetTwoFactorEnabledAsync(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.TwoFactorEnabled);
        }

        protected virtual async Task<ApplicationUser> GetUserAggregateAsync(
            Expression<Func<ApplicationUser, bool>> filter)
        {
            return await Task.FromResult(Users.FirstOrDefault(filter));
        }
    }
}