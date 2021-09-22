using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStoreAPI.Data.Models
{
    public class Book
    {
        public int Id { get; set; }

        public string Title { get; set; }
        
        public string Description { get; set; }

        public bool IsRead { get; set; }

        private DateTime? dateRead;
        public DateTime? DateRead
        {
            get
            {
                return dateRead;
            }
            set
            {
                dateRead = (IsRead) ? value : null;
            }
        }

        private int? rate;
        public int? Rate
        {
            get
            {
                return rate;
            }
            set
            {
                rate = (IsRead) ? value : null;
            }
        }

        public string Genre { get; set; }

        public string CoverUrl { get; set; }

        public DateTime DateAdded { get; set; }

        //navigation properties
        public int? PublisherId { get; set; }
        public Publisher Publisher { get; set; } 
        public List<Book_Author> Book_Authors { get; set; } = new List<Book_Author>();
    }

}
