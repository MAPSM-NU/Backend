using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_App.Domain.DTOs
{
    public class ScheduleDTO
    {
        public Guid UserID { get; set; }
        public Guid ScheduleID { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }

    }
}
