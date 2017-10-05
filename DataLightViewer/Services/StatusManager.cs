using DataLightViewer.Mediator;
using System.Collections.ObjectModel;

namespace DataLightViewer.Services
{
    public static class StatusManager
    {
        private static readonly ObservableCollection<string> _executingTasks = new ObservableCollection<string>();
        public static int TaskQuantity => _executingTasks.Count;

        public static void AttachTask(string actionMessage)
        {
            _executingTasks.Add(actionMessage);
        }

        public static void DetachTask(string actionMessage)
        {
            if (_executingTasks.Contains(actionMessage))
                _executingTasks.Remove(actionMessage);
        }
    }
}
