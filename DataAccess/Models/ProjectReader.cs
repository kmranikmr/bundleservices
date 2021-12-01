using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class ProjectReader
    {
        public int ProjectId { get; set; }
        public int ReaderId { get; set; }

        public virtual Project Project { get; set; }
        public virtual Reader Reader { get; set; }
    }
}
