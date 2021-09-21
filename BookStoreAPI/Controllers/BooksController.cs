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

        public BooksController(BooksService booksService)
        {
            // _context = context;
            _booksService = booksService;
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
    }
}
