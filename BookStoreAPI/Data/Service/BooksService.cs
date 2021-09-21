using BookStoreAPI.Data.Models;
using BookStoreAPI.Data.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStoreAPI.Data.Service
{
    public class BooksService
    {
        private AppDbContext _context;

        public BooksService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Book> AddBook(BookVM book)
        {

            var _book = new Book()
            {
                Title = book.Title,
                Description = book.Description,
                IsRead = book.IsRead,
                DateRead = book.DateRead.Value,
                Rate = book.Rate.Value,
                Genre = book.Genre,
                CoverUrl = book.CoverUrl,
                DateAdded = DateTime.Now,
                PublisherId = book.PublisherId,
            };
            _context.Books.Add(_book);
            _context.SaveChanges();

            //Add Book to Publisher
            var publisher = _context.Publishers.FirstOrDefault(b => b.Id == book.PublisherId);
            publisher.Books.Add(_book);
            _context.SaveChanges();

            foreach (var id in book.AuthorIds)
            {
                var _book_author = new Book_Author()
                {
                    BookId = _book.Id,
                    AuthorId = id
                };
                _context.Book_Authors.Add(_book_author);
                _context.SaveChanges();
            }

            var bookResult = await _context.Books.Where(s => s.Id == _book.Id)
                                                 .Include(s => s.Publisher).FirstOrDefaultAsync();

            return bookResult;
        }

        public async Task<List<Book>> GetBooks()
        {
            return await _context.Books.Include(s => s.Publisher).ToListAsync();  //ToList();
        }
        public BookAuthorVM GetBookById(int id)
        {
            var _bookAuthors = _context.Books.Where(n => n.Id == id).Select(n => new BookAuthorVM()
            {
                Title = n.Title,
                Description = n.Description,
                IsRead = n.IsRead,
                DateRead = n.DateRead.Value,
                Rate = n.Rate.Value,
                Genre = n.Genre,
                CoverUrl = n.CoverUrl,
                PublisherName = n.Publisher.Name,
                Authors = n.Book_Authors.Select(n => n.Author).ToList()
            }).FirstOrDefault();

            return _bookAuthors;
        }

        //public async Task<Book> UpdateBook(int id)
        //{
                
        //}
    }
}
