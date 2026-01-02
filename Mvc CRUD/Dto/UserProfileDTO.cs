using System.ComponentModel.DataAnnotations.Schema;

namespace Mvc_CRUD.Dto;

    [NotMapped]
    public class UserProfileDTO
    {
       public string? subject { get; set; }
       public string? SchoolName { get; set; }
       public string? collegePeriod { get; set; }
       public string? course { get; set; }
       public string? collegeName { get; set; }
       public string? location { get; set; }
       public string? relationship { get; set; }
       public string? bio { get; set; }
       public string? fromLocation { get; set; }
       public string? jobPeriod { get; set; }
       public string? industry { get; set; }
       public string? jobTitle { get; set; }
       public string?  schoolPeriod { get; set; }
       public string? web { get; set; }
    }

