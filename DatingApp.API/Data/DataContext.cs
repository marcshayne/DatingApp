using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }
        // tell the datacontext class about our entities
        // Values will be the  table name
        public DbSet<Value> Values { get; set; }
        public DbSet<User> Users { get; set; }
    }
}