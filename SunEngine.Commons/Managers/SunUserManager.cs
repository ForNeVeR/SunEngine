using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SunEngine.Commons.DataBase;
using SunEngine.Commons.Models;
using SunEngine.Commons.Security;
using SunEngine.Commons.Utils;

namespace SunEngine.Commons.Managers
{
    public class SunUserManager : UserManager<User>
    {
        protected readonly DataBaseConnection db;
        
        public SunUserManager(DataBaseConnection db, IUserStore<User> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<User> passwordHasher, IEnumerable<IUserValidator<User>> userValidators, IEnumerable<IPasswordValidator<User>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<User>> logger) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            this.db = db;
            KeyNormalizer = Normalizer.Singleton;
        }    
        
        public async Task<User> FindUserByNameOrEmailAsync(string nameOrEmail)
        {
            User user;
            if (EmailValidator.IsValid(nameOrEmail))
            {
                user = await FindByEmailAsync(nameOrEmail)
                       ?? await FindByNameAsync(nameOrEmail); // if name is email like
            }
            else
            {
                user = await FindByNameAsync(nameOrEmail);
            }

            return user;
        }
        
        public virtual Task ChangeEmailAsync(int userId, string email)
        {
            return db.Users.Where(x => x.Id == userId).Set(x => x.Email, email)
                .Set(x => x.NormalizedEmail, Normalizer.Normalize(email))
                .UpdateAsync();
        }

        public virtual Task<bool> CheckEmailInDbAsync(string email, int userId)
        {
            return db.Users.AnyAsync(x => x.NormalizedEmail == Normalizer.Normalize(email) && x.Id != userId);
        }

        public virtual LongSession FindLongSession(LongSession longSession)
        {
            return db.LongSessions.FirstOrDefault(x => x.UserId == longSession.UserId &&
                                                       x.LongToken1 == longSession.LongToken1 &&
                                                       x.LongToken2 == longSession.LongToken2);
        }

        public virtual Task<User> FindByIdAsync(int id)
        {
            return db.Users.FirstOrDefaultAsync(x => x.Id == id);
        }

        public virtual Task<bool> IsUserInRoleAsync(int userId, string roleName)
        {
            var normalizedRoleName = Normalizer.Normalize(roleName);
            return db.UserRoles.AnyAsync(x => x.UserId == userId && x.Role.NormalizedName == normalizedRoleName);
        }

        public override async Task<IdentityResult> AddToRoleAsync(User user, string role)
        {
            if (Normalizer.Normalize(role) == RoleNames.UnregisteredNormalized)
                return IdentityResult.Failed(new IdentityError {Code = "Can_not_add_role_guest", Description = "Can not add guest role"});
            return await base.AddToRoleAsync(user, role);
        }
    }
}