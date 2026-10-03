using ESMC.ELM.Domain.Entities;

namespace ESMC.ELM.Web.Models
{
    public class TasksBoardViewModel
    {
        public List<TaskItem> ToDo { get; set; } = new();

        public List<TaskItem> InProgress { get; set; } = new();

        public List<TaskItem> Blocked { get; set; } = new();

        public List<TaskItem> Done { get; set; } = new();

        public Dictionary<Guid, string> UserNames { get; set; } = new();

        public int TotalTasks =>
            ToDo.Count +
            InProgress.Count +
            Blocked.Count +
            Done.Count;

        public string GetUserName(Guid userId)
        {
            return UserNames.TryGetValue(userId, out var name)
                ? name
                : "Unknown User";
        }
    }
}