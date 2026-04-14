using System.ComponentModel.DataAnnotations.Schema;

namespace Mvc_CRUD.Models;

    [NotMapped]
    public class RateLimitViolationInfo
    {
        public DateTime BlockedUntilUtc { get; set; }
        public int ViolationSeconds { get; set; }
    }

