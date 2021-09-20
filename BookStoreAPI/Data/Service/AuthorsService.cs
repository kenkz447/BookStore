using BookStoreAPI.Data.Models;
using BookStoreAPI.Data.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStoreAPI.Data.Service
{
    public class AuthorsService
    {
        private AppDbContext _context;

        public AuthorsService(AppDbContext context)
        {
            _context = context;
        }

        public void addAuthor(AuthorVM author)
        {

            var _author = new Author()
            {
               FullName = author.Fullname
            };
            _context.Authors.Add(_author);
            _context.SaveChanges();
        }
    }
}
