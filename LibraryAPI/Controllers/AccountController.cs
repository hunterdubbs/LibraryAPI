using LibraryAPI.DAL;
using LibraryAPI.Domain;
using LibraryAPI.Domain.Requests;
using LibraryAPI.Domain.Responses;
using LibraryAPI.LogicProcessors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        protected ILibraryDataContext libraryDataContext;
        protected UserManager<ApplicationUser> userManager;
    
        public AccountController(ILibraryDataContext libraryDataContext, UserManager<ApplicationUser> userManager)
        {
            this.libraryDataContext = libraryDataContext;
            this.userManager = userManager;
        }

        [HttpGet]
        [Route("info")]
        public async Task<IActionResult> GetInfo()
        {
            string userID = ClaimsHelper.GetUserIDFromClaim(User);

            ApplicationUser user = await userManager.FindByIdAsync(userID);
            if (user == null) return BadRequest();

            AccountInfoResponse accountInfo = new AccountInfoResponse()
            {
                Username = user.UserName,
                Email = user.Email
            };
            return Ok(accountInfo);
        }

        [HttpPost]
        [Route("delete")]
        public async Task<IActionResult> DeleteAccount([FromBody][Required] DeleteAccountRequest request)
        {
            string userID = ClaimsHelper.GetUserIDFromClaim(User);

            ApplicationUser user = await userManager.FindByIdAsync(userID);
            if (user == null) return BadRequest("Account not found");
            if (user.UserName.ToUpper() == "EXAMPLE") return BadRequest("Demo account cannot be deleted");
            try
            {
                bool verified = await userManager.CheckPasswordAsync(user, request.Password);
                if (!verified) return BadRequest("Invalid password");
            }
            catch (Exception)
            {
                return BadRequest("Invalid password");
            }

            using(UnitOfWork unitOfWork = new UnitOfWork())
            {
                unitOfWork.Begin();

                libraryDataContext.PasswordResetCodeRepository.DeleteByUserID(userID);

                var libaries = libraryDataContext.LibraryRepository.GetAllByUser(userID);
                foreach (var library in libaries)
                {
                    libraryDataContext.TagRepository.DeleteAllByLibraryID(library.ID);
                }

                libraryDataContext.BookRepository.DeleteByUserID(userID);
                libraryDataContext.CollectionRepository.DeleteByUserID(userID);
                libraryDataContext.InviteRepository.DeleteByUserID(userID);
                libraryDataContext.PermissionRepository.DeleteByUserID(userID);
                libraryDataContext.LibraryRepository.DeleteByUserID(userID);

                unitOfWork.Commit();
            }

            await userManager.DeleteAsync(user);
            return Ok();
        }
    }
}
