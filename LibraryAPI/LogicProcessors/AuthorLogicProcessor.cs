using LibraryAPI.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryAPI.LogicProcessors
{
    public class AuthorLogicProcessor
    {
        protected ILibraryDataContext libraryDataContext;

        public AuthorLogicProcessor(ILibraryDataContext libraryDataContext)
        {
            this.libraryDataContext = libraryDataContext;
        }

        public Result<List<Author>> Search(string searchTerm)
        {
            Result<List<Author>> result = new Result<List<Author>>();
            result.Value = libraryDataContext.AuthorRepository.GetByFullNamePart(searchTerm);
            return result;
        }

        public Result<Author> CreateOrGetAuthor(Author author, string userID)
        {
            Result<Author> result = new Result<Author>();
            Author existingAuthor = libraryDataContext.AuthorRepository.GetByFirstLastName(author.FirstName, author.LastName);
            if(existingAuthor != null)
            {
                result.Value = existingAuthor;
                return result;
            }

            libraryDataContext.AuthorRepository.Add(author);
            result.Value = author;
            return result;
        }
    }
}
