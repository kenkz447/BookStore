using BookStoreAPI.Data.Models;
using BookStoreAPI.Data.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStoreAPI.Data.Service
{
    public class PublishersService
    {
        private AppDbContext _context;

        public PublishersService(AppDbContext context)
        {
            _context = context;
        }

        public void addPublisher(PublisherVM publisher)
        {

            var _publisher = new Publisher()
            {
                Name = publisher.Name
            };
            _context.Publishers.Add(_publisher);
            _context.SaveChanges();
        }

        public async Task<object> GetPublishers()
        {
            return await _context.Publishers.Include(s=> s.Books).ToListAsync();
        }
    }
}
