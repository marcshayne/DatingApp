using System;

namespace DatingApp.API.Models
{
    public class Photo
    {
        public int Id { get; set; }
        public string Url { get; set; } 
        public string Description { get; set; }
        public DateTime DateAdded { get; set; }
        public bool IsMain { get; set; }
        public string PublicId  { get; set; }
        
        // create relationship with User table. this will support cascaded delete
        public User User { get; set; }
        public int UserId { get; set; }

    }
}