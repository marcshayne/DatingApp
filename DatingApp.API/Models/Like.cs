namespace DatingApp.API.Models
{
    public class Like
    {
        public int LikerId { get; set; }  // the one who likes
        public int LikeeId { get; set; }  // the one being liked
        public User Liker { get; set; }
        public User Likee { get; set; }
    }
}