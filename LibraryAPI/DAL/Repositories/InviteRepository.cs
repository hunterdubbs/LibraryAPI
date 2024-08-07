﻿using LibraryAPI.Domain;
using LibraryAPI.Domain.Enum;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace LibraryAPI.DAL.Repositories
{
    public class InviteRepository : RepositoryBase
    {
        public InviteRepository() : base() { }

        public InviteRepository(UnitOfWork unitOfWork) : base(unitOfWork) { }

        public void Add(Invite invite)
        {
            DbCommand cmd = CreateCommand(@"INSERT INTO tLibraryInvite(iLibraryID, sInviterID, sRecipientID, iPermissionLevel, dtSent) VALUES (@iLibraryID, @sInviterID, @sRecipientID, @iPermissionLevel, @dtSent) RETURNING iID");
            cmd.Parameters.Add(CreateParameter("@iLibraryID", invite.LibraryID));
            cmd.Parameters.Add(CreateParameter("@sInviterID", invite.InviterID));
            cmd.Parameters.Add(CreateParameter("@sRecipientID", invite.RecipientID));
            cmd.Parameters.Add(CreateParameter("@iPermissionLevel", (int)invite.PermissionLevel));
            cmd.Parameters.Add(CreateParameter("@dtSent", invite.Sent));
            invite.ID = (int)cmd.ExecuteScalar();
        }

        public void Delete(int id)
        {
            DbCommand cmd = CreateCommand(@"DELETE FROM tLibraryInvite WHERE iID=@iID");
            cmd.Parameters.Add(CreateParameter("@iID", id));
            cmd.ExecuteNonQuery();
        }

        public void DeleteByUserID(string userID)
        {
            DbCommand cmd = CreateCommand(@"DELETE FROM tLibraryInvite WHERE sInviterID=@sUserID OR sRecipientID=@sUserID");
            cmd.Parameters.Add(CreateParameter("@sUserID", userID));
            cmd.ExecuteNonQuery();
        }

        public List<Invite> GetAllByRecipient(string recipientID)
        {
            DbCommand cmd = CreateCommand(@"SELECT i.*, s.UserName as sInviterUsername, r.UserName as sRecipientUsername, l.sName as sLibraryName FROM tLibraryInvite i INNER JOIN AspNetUsers s ON i.sInviterID=s.Id INNER JOIN AspNetUsers r ON i.sRecipientID=r.Id INNER JOIN tLibrary l ON i.iLibraryID=l.iID WHERE i.sRecipientID=@sRecipientID");
            cmd.Parameters.Add(CreateParameter("@sRecipientID", recipientID));
            return ExtractData(cmd);
        }

        public List<Invite> GetAllByLibrary(int libraryID)
        {
            DbCommand cmd = CreateCommand(@"SELECT i.*, s.UserName as sInviterUsername, r.UserName as sRecipientUsername, l.sName as sLibraryName FROM tLibraryInvite i INNER JOIN AspNetUsers s ON i.sInviterID=s.Id INNER JOIN AspNetUsers r ON i.sRecipientID=r.Id INNER JOIN tLibrary l ON i.iLibraryID=l.iID WHERE i.iLibraryID=@iLibraryID");
            cmd.Parameters.Add(CreateParameter("@iLibraryID", libraryID));
            return ExtractData(cmd);
        }

        public Invite GetByID(int id)
        {
            DbCommand cmd = CreateCommand(@"SELECT i.*, s.UserName as sInviterUsername, r.UserName as sRecipientUsername, l.sName as sLibraryName FROM tLibraryInvite i INNER JOIN AspNetUsers s ON i.sInviterID=s.Id INNER JOIN AspNetUsers r ON i.sRecipientID=r.Id INNER JOIN tLibrary l ON i.iLibraryID=l.iID WHERE i.iID=@iID");
            cmd.Parameters.Add(CreateParameter("@iID", id));
            return ExtractData(cmd).FirstOrDefault();
        }

        private List<Invite> ExtractData(DbCommand cmd)
        {
            List<Invite> results = new List<Invite>();
            using(DbDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Invite result = new Invite();
                    result.ID = ReadInt(reader, "iID");
                    result.LibraryID = ReadInt(reader, "iLibraryID");
                    result.InviterID = ReadString(reader, "sInviterID");
                    result.RecipientID = ReadString(reader, "sRecipientID");
                    result.PermissionLevel = (PermissionType)ReadInt(reader, "iPermissionLevel");
                    result.Sent = ReadDateTime(reader, "dtSent");
                    result.InviterUsername = ReadString(reader, "sInviterUsername");
                    result.RecipientUsername = ReadString(reader, "sRecipientUsername");
                    result.LibraryName = ReadString(reader, "sLibraryName");
                    results.Add(result);
                }
            }

            return results;
        }
    }
}
