using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Test.Models;

namespace Test.Data
{
    public class TestContext : DbContext
    {
        public TestContext (DbContextOptions<TestContext> options)
            : base(options)
        {
        }

        public DbSet<Test.Models.Certification> Certification { get; set; } = default!;
        public DbSet<Test.Models.PdfFile> PdfFile { get; set; } = default!;
        public DbSet<Test.Models.User> User { get; set; } = default!;
        public DbSet<Test.Models.Role> Role { get; set; } = default!;
    }
}
