using System;

namespace SabinoLabs.Domain.Dto
{
    public class AuditedEntityBaseDto
    {
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string LastModifiedBy { get; set; }
        public DateTime LastModifiedDate { get; set; }
    }
}