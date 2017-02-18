namespace solver_TVHS_26_7
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class TVHS : DbContext
    {
        public TVHS()
            : base("name=TVHS")
        {
        }

        public virtual DbSet<Test> Tests { get; set; }
        public virtual DbSet<TestCase> TestCases { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
