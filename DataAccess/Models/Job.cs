﻿using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class Job
    {
        public int JobId { get; set; }
        public int ProjectFileId { get; set; }
        public int UserId { get; set; }
        public int JobStatusId { get; set; }
        public int ProjectId { get; set; }
        public string JobDescription { get; set; }
        public int? SchemaId { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? StartedOn { get; set; }
        public DateTime? CompletedOn { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public virtual JobStatus JobStatus { get; set; }
        public virtual Project Project { get; set; }
        public virtual ProjectFile ProjectFile { get; set; }
        public virtual ProjectSchema Schema { get; set; }
    }
}
