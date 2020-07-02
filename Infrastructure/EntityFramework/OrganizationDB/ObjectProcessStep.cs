using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class ObjectProcessStep
    {
        public ObjectProcessStep()
        {
            ObjectProcessStepDependDependObjectProcessStep = new HashSet<ObjectProcessStepDepend>();
            ObjectProcessStepDependObjectProcessStep = new HashSet<ObjectProcessStepDepend>();
            ObjectProcessStepUser = new HashSet<ObjectProcessStepUser>();
        }

        public int ObjectProcessStepId { get; set; }
        public int ObjectProcessTypeId { get; set; }
        public int SortOrder { get; set; }
        public string ObjectProcessStepName { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual ICollection<ObjectProcessStepDepend> ObjectProcessStepDependDependObjectProcessStep { get; set; }
        public virtual ICollection<ObjectProcessStepDepend> ObjectProcessStepDependObjectProcessStep { get; set; }
        public virtual ICollection<ObjectProcessStepUser> ObjectProcessStepUser { get; set; }
    }
}
