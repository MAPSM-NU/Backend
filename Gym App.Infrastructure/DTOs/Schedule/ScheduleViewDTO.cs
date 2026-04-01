using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_App.Infastructure.DTOs.Schedule
{
    public class ScheduleViewDTO
    {
        public Guid UserID { get; set; }
        public Guid ScheduleID { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
