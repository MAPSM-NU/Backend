using Gym_App.Domain.Transfer_Classes;

namespace Gym_App.Infastructure.Transfer_Classes
{
    public class GetterResponses<T>
    {
        public int status { get; set; }
        public string msg { get; set; } = string.Empty;
        public PagedList<T>? Data { get; set; }
    }
}
