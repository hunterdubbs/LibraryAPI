using LibraryAPI.DAL;
using LibraryAPI.Domain;
using LibraryAPI.Domain.Enum;
using LibraryAPI.Domain.Requests;
using LibraryAPI.Domain.Responses;
using LibraryAPI.LogicProcessors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class PermissionController : ControllerBase
    {
        protected ILibraryDataContext libraryDataContext;
        protected PermissionLogicProcessor permissionLogicProcessor;

        public PermissionController(ILibraryDataContext libraryDataContext, PermissionLogicProcessor permissionLogicProcessor)
        {
            this.permissionLogicProcessor = permissionLogicProcessor;
            this.libraryDataContext = libraryDataContext;
        }

        [HttpGet]
        [Route("library/{libraryID}")]
        public IActionResult GetPermissionsOnLibrary([FromRoute] int libraryID)
        {
            string userID = ClaimsHelper.GetUserIDFromClaim(User);
            bool permissionDenied = false;
            Result<LibraryPermissionResponse> result = new Result<LibraryPermissionResponse>();

            using(UnitOfWork uow = new UnitOfWork())
            {
                result = permissionLogicProcessor.GetLibraryPermissions(libraryID, userID, out permissionDenied);
            }

            if (!result.Succeeded)
            {
                if (permissionDenied) return Unauthorized();
                return StatusCode(500, result.Error);
            }

            return Ok(result.Value);
        }

        [HttpPost]
        [Route("remove")]
        public IActionResult RemovePermissionOnLibrary([FromBody] RemovePermissionRequest request)
        {
            string userID = ClaimsHelper.GetUserIDFromClaim(User);

            using (UnitOfWork uow = new UnitOfWork())
            {
                uow.Begin();

                Result result = permissionLogicProcessor.RemoveLibraryPermission(request.LibraryID, request.UserID, userID, out bool permissionDenied);

                if (result.Succeeded)
                {
                    uow.Commit();
                    return Ok();
                }
                else
                {
                    if (permissionDenied) return Forbid();
                    return BadRequest(result.Error);
                }
            }
        }

        [HttpGet]
        [Route("user/search")]
        public IActionResult SearchUsers([FromQuery] string searchTerm)
        {
            string userID = ClaimsHelper.GetUserIDFromClaim(User);
            Result<List<InvitableUser>> result = new Result<List<InvitableUser>>();

            using(UnitOfWork uow = new UnitOfWork())
            {
                var users = libraryDataContext.UserRepository.SearchUsersByUsername(searchTerm);
                users.RemoveAll(u => u.UserId == userID);
                result.Value = users;
            }

            return Ok(result.Value);
        }

        [HttpPost]
        [Route("invite/create")]
        public IActionResult CreateInvite([FromBody] CreateInviteRequest request)
        {
            string userID = ClaimsHelper.GetUserIDFromClaim(User);

            if (request.PermissionType != PermissionType.Viewer && request.PermissionType != PermissionType.Editor) return BadRequest("Invalid permission type");

            Invite invite = new Invite()
            {
                InviterID = userID,
                RecipientID = request.RecipientID,
                LibraryID = request.LibraryID,
                PermissionLevel = request.PermissionType,
                Sent = DateTime.Now
            };

            using(UnitOfWork uow = new UnitOfWork())
            {
                uow.Begin();
                Result result = permissionLogicProcessor.CreateInvite(invite, userID, out bool permissionDenied);

                if (result.Succeeded)
                {
                    uow.Commit();
                    return Ok();
                }
                else
                {
                    if (permissionDenied) return Forbid();
                    return BadRequest(result.Error);
                }
            }
        }

        [HttpGet]
        [Route("invites")]
        public IActionResult GetInvites()
        {
            string userID = ClaimsHelper.GetUserIDFromClaim(User);
            Result<List<Invite>> result = new Result<List<Invite>>();

            using(UnitOfWork uow = new UnitOfWork())
            {
                result.Value = libraryDataContext.InviteRepository.GetAllByRecipient(userID);
            }

            return Ok(result.Value);
        }

        [HttpDelete]
        [Route("invite/{inviteID}")]
        public IActionResult DeleteInvite([FromRoute] int inviteID)
        {
            string userID = ClaimsHelper.GetUserIDFromClaim(User);

            using(UnitOfWork uow = new UnitOfWork())
            {
                uow.Begin();

                Result result = permissionLogicProcessor.DeleteInvite(inviteID, userID, out bool permissionDenied);

                if (result.Succeeded)
                {
                    uow.Commit();
                    return Ok();
                }
                else
                {
                    if (permissionDenied) return Forbid();
                    return BadRequest(result.Error);
                }
            }
        }

        [HttpPost]
        [Route("invite/{inviteID}/accept")]
        public IActionResult AcceptInvite([FromRoute] int inviteID)
        {
            string userID = ClaimsHelper.GetUserIDFromClaim(User);

            using(UnitOfWork uow = new UnitOfWork()){
                uow.Begin();

                Result result = permissionLogicProcessor.AcceptInvite(inviteID, userID, out bool permissionDenied);

                if (result.Succeeded)
                {
                    uow.Commit();
                    return Ok();
                }
                else
                {
                    if (permissionDenied) return Forbid();
                    return BadRequest(result.Error);
                }
            }
        }

        [HttpPost]
        [Route("invite/{inviteID}/reject")]
        public IActionResult Reject([FromRoute] int inviteID)
        {
            string userID = ClaimsHelper.GetUserIDFromClaim(User);

            using (UnitOfWork uow = new UnitOfWork())
            {
                uow.Begin();

                Result result = permissionLogicProcessor.RejectInvite(inviteID, userID, out bool permissionDenied);

                if (result.Succeeded)
                {
                    uow.Commit();
                    return Ok();
                }
                else
                {
                    if (permissionDenied) return Forbid();
                    return BadRequest(result.Error);
                }
            }
        }
    }
}
