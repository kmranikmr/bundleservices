using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class WorkflowElement
    {
        public int WorkflowElementId { get; set; }
        public string ElementName { get; set; }
        public string ElementIconName { get; set; }
        public string ElementProperties { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }
}
