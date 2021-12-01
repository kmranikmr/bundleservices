using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class ModelMetadata
    {
        public int MetadataId { get; set; }
        public int ProjectId { get; set; }
        public int ModelId { get; set; }
        public string ColumnName { get; set; }
        public string DataType { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public virtual SchemaModel Model { get; set; }
        public virtual Project Project { get; set; }
    }
}
