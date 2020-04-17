using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext _context;
        public DatingRepository(DataContext context)
        {
            _context = context;

        }

        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<Like> GetLike(int userId, int recipientId)
        {
             return await _context.Likes.FirstOrDefaultAsync(u => 
                    u.LikerId == userId && u.LikeeId == recipientId);
        }

        public async Task<Photo> GetMainPhotoForUser(int userId)
        {
            return await _context.Photos.Where(u => u.UserId == userId)
                .FirstOrDefaultAsync(p => p.IsMain);
        }

        public async Task<Photo> GetPhoto(int id)
        {
            var photo = await _context.Photos.FirstOrDefaultAsync(p => p.Id == id);

            return photo;
        }

        public async Task<User> GetUser(int id)
        {
            var user = await _context.Users.Include(p => p.Photos).FirstOrDefaultAsync(u => u.Id == id);

            return user;  // null if there is not a user          
        }

        // public async Task<IEnumerable<User>> GetUsers()
        // {
        //     var users = await _context.Users.Include(p => p.Photos).ToListAsync();
        //     return users;
        // }
        public async Task<PagedList<User>> GetUsers(UserParams userParams)   // getting paged users
        {
          
            var users = _context.Users.Include(p => p.Photos)
            .OrderByDescending(u => u.LastActive)
            .AsQueryable(); // return AsQueryable (isntead of ICollection) to be able to use the Where statement
            // Filters
            users = users.Where(u => u.Id != userParams.UserId); // L.145 filter out current user
            users = users.Where(u => u.Gender == userParams.Gender); // L.145 filter out current user

            // L.155
            if(userParams.Likers)
            {
                var userLikers = await GetUserLikes(userParams.UserId, userParams.Likers);
                users = users.Where(u => userLikers.Contains(u.Id));

            }
            if(userParams.Likees)
            {
                 var userLikees = await GetUserLikes(userParams.UserId, userParams.Likers);
                users = users.Where(u => userLikees.Contains(u.Id));
            }


            if (userParams.MinAge !=18 || userParams.MaxAge !=99) // L.146 age filtering
            {
                var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
                var maxDob = DateTime.Today.AddYears(-userParams.MinAge);
                users = users.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);
            } 

                        
            // L.148 Order by
            if(!string.IsNullOrEmpty(userParams.OrderBy))   
            {
                switch(userParams.OrderBy)
                {
                    case "created": 
                        users = users.OrderByDescending(u => u.Created);
                        break;
                    default:
                        users = users.OrderByDescending(u => u.LastActive);
                        break;
                }
            }  

            return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
        }

        // Get list of likers or likee L.155
        private async Task<IEnumerable<int>> GetUserLikes(int id, bool likers)
        {
            var user = await _context.Users.Include(x =>x.Likers).Include(x => x.Likees)
                        .FirstOrDefaultAsync(u  => u.Id ==  id);
            if(likers)
            {
                return user.Likers.Where(u => u.LikeeId ==id).Select(i => i.LikerId);
            }
            else
            {
                return user.Likees.Where(u => u.LikerId ==id).Select(i => i.LikeeId);
            }

        }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0; // if it is >0 it returns true else false
        }
    }
}