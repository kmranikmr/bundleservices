﻿using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class ProjectAutomation
    {
        public int ProjectAutomationId { get; set; }
        public int ProjectId { get; set; }
        public int ReaderId { get; set; }
        public int ProjectSchemaId { get; set; }
        public string FolderPath { get; set; }
        public int CreatedBy { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public virtual Project Project { get; set; }
        public virtual ProjectSchema ProjectSchema { get; set; }
        public virtual Reader Reader { get; set; }
    }
}
