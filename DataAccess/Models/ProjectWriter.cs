using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class ProjectWriter
    {
        public int ProjectId { get; set; }
        public int WriterId { get; set; }

        public virtual Project Project { get; set; }
        public virtual Writer Writer { get; set; }
    }
}
