


using Microsoft.Extensions.Configuration;

namespace VerifyEmailForgotPasswordTutorial.Data
{
    public class DataContext:DbContext
    {

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
            
        }

       public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer("Server=WIN-OAMUUJJ1LER\\SQLEXPRESS;database=LoginBase;Trusted_Connection=true");
        }

       
    }
}
