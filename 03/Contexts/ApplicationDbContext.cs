using _03.Entities;
using _03.Entities.Seguridad;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace _03.Contexts
{
    //IdentityDbContext<ApplicationUser> se puede trabajar igual como DbContext
    //la diferencia es que se puede trabajar con la tabla por defaul 
    //que tiene el propio framework que tiene .Net


    //public class ApplicationDbContext: DbContext
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }

        //protected override void OnModelCreating(ModelBuilder builder)
        //{
        //    var roleAdmin = new IdentityRole()
        //    {
        //        Id = "89086180-b978-4f90-9dbd-a7040bc93f41",
        //        Name = "admin",
        //        NormalizedName = "admin"
        //    };
        //    builder.Entity<IdentityRole>().HasData(roleAdmin);
        //    base.OnModelCreating(builder);
        //}
        public DbSet<Autor> Autores { get; set; }
        public DbSet<Libro> Libros { get; set; }
    }
}
