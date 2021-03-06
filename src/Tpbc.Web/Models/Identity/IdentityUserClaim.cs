﻿namespace Tpbc.Web.Models.Identity
{
    public class IdentityUserClaim<TKey>
    {
        /// <summary>
        ///     Primary key
        /// </summary>
        public virtual int Id { get; set; }
        /// <summary>
        ///     User Id for the user who owns this login
        /// </summary>
        public virtual TKey UserId { get; set; }
        /// <summary>
        ///     Claim type
        /// </summary>
        public virtual string ClaimType { get; set; }
        /// <summary>
        ///     Claim value
        /// </summary>
        public virtual string ClaimValue { get; set; }
    }
}