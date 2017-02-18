namespace solver_TVHS_26_7
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Test")]
    public partial class Test
    {
        public int Id { get; set; }

        public int? TestCaseId { get; set; }

        public int? NoGen { get; set; }

        public float? Revenue { get; set; }

        public int? Children { get; set; }

        public float? RevenueIteration { get; set; }

        public virtual TestCase TestCase { get; set; }
    }
}
