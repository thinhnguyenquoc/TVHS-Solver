namespace solver_TVHS_26_7
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TestCase")]
    public partial class TestCase
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TestCase()
        {
            Tests = new HashSet<Test>();
        }

        public int Id { get; set; }

        [Column("TestCase")]
        public string TestCase1 { get; set; }

        public string Kind { get; set; }

        public double? CrossOver { get; set; }

        public double? Mutation { get; set; }

        public int? Population { get; set; }

        public int? LimitLoop { get; set; }

        public int? NoChange { get; set; }

        public DateTime? Date { get; set; }

        public int? ElapseTime { get; set; }

        public int? NoGen { get; set; }

        public double? MaxGen { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Test> Tests { get; set; }
    }
}
