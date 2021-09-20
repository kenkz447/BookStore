using BookStoreAPI.Data.Service;
using BookStoreAPI.Data.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublishersController : ControllerBase
    {
        private PublishersService _publishersService;

        public PublishersController(PublishersService publishersService)
        {
            _publishersService = publishersService;
        }

        [HttpPost]
        public IActionResult AddPublisher([FromBody] PublisherVM publisher)
        {
            _publishersService.addPublisher(publisher);
            return Ok();
        }

        public async Task<IActionResult> GetPublishers()
        {
            return Ok(await _publishersService.GetPublishers());
        }
    }
}
