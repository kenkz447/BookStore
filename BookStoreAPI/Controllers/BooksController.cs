using BookStoreAPI.Data;
using BookStoreAPI.Data.Models;
using BookStoreAPI.Data.Service;
using BookStoreAPI.Data.ViewModels;
using BookStoreAPI.Data.ViewModels.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BookStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        //  private AppDbContext _context;
        private BooksService _booksService;
        private AppDbContext _appDbContext;

        public BooksController(BooksService booksService, AppDbContext appDbContext)
        {
            // _context = context;
            _booksService = booksService;
            _appDbContext = appDbContext;
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost]
        public async Task<IActionResult> AddBook([FromBody] BookVM book)
        {

            var bookResult = await _booksService.AddBook(book);
            return Ok(bookResult);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetBooks()
        {
            //Console.WriteLine("line 35 here");
            //  var id = request.Headers.GetValues("Authorization").FirstOrDefault();
            //  Console.WriteLine(id);
            return Ok(await _booksService.GetBooks());
        }

        [HttpGet]
        [Route("{id}")]
        public IActionResult GetBookById(int id)
        {
            var book = _booksService.GetBookById(id);
            if (book == null)
            {
                return NotFound($"Not Found Book with id {id}");
            }

            return Ok(book);
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> UpdateBook(int id, [FromBody] Book book)
        {
            if (id != book.Id)
                return BadRequest();

            if (!_appDbContext.Books.Any(b => b.Id == id))
                return NotFound($"Not found Book with id {id}");

            //set state for entity
            _appDbContext.Entry(book).State = EntityState.Modified;
            await _appDbContext.SaveChangesAsync();

            var bookResult = _appDbContext.Books.Where(n => n.Id == id).Include(s => s.Publisher).FirstOrDefault();
            return Ok(bookResult);
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<ActionResult<Book>> DeleteBook(int id)
        {
            var book = await _appDbContext.Books.FindAsync(id);
            if (book == null)
                return NotFound();
            _appDbContext.Books.Remove(book);
            await _appDbContext.SaveChangesAsync();

            return Ok(book);
        }
    }
}
