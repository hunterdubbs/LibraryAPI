using LibraryAPI.DAL;
using LibraryAPI.Domain;
using LibraryAPI.LogicProcessors;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

namespace LibraryAPI
{
    public class SimpleTokenProvider : IUserTwoFactorTokenProvider<ApplicationUser>
    {
        private ILibraryDataContext libraryDataContext;

        public SimpleTokenProvider(ILibraryDataContext libraryDataContext)
        {
            this.libraryDataContext = libraryDataContext;
        }

        public Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<ApplicationUser> manager, ApplicationUser user)
        {
            bool accountValid = user.EmailConfirmed;
            return Task.FromResult(accountValid);
        }

        public Task<string> GenerateAsync(string purpose, UserManager<ApplicationUser> manager, ApplicationUser user)
        {
            string code = SecurityHelper.GenerateAlphanumericCode(8);
            SaltedHash saltedHash = SecurityHelper.GenerateSaltedHash(code);

            PasswordResetCode resetCode = new PasswordResetCode()
            {
                UserID = user.Id,
                Expires = DateTime.Now.AddMinutes(15),
                Hash = saltedHash.Hash,
                Salt = saltedHash.Salt
            };
            
            using(UnitOfWork uow = new UnitOfWork())
            {
                uow.Begin();
                libraryDataContext.PasswordResetCodeRepository.DeleteAllExpired();
                libraryDataContext.PasswordResetCodeRepository.Add(resetCode);
                uow.Commit();
            }

            return Task.FromResult(code);
        }

        public Task<bool> ValidateAsync(string purpose, string token, UserManager<ApplicationUser> manager, ApplicationUser user)
        {
            using(UnitOfWork uow = new UnitOfWork())
            {
                uow.Begin();
                libraryDataContext.PasswordResetCodeRepository.DeleteAllExpired();

                var validCodes = libraryDataContext.PasswordResetCodeRepository.GetAllByUserID(user.Id);

                foreach(var code in validCodes)
                {
                    if(SecurityHelper.CompareToSaltedHash(token, new SaltedHash() { Hash = code.Hash, Salt = code.Salt }))
                    {
                        libraryDataContext.PasswordResetCodeRepository.Delete(code.ID);
                        uow.Commit();
                        return Task.FromResult(true);
                    }
                }

                uow.Commit();
            }

            return Task.FromResult(false);
        }
    }
}
