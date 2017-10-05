using DataLightViewer.Mediator;
using System;
using System.Threading.Tasks;

namespace DataLightViewer.Services
{
    public sealed class ExecutingTask
    {
        public string Id { get; private set; }
        public string Message { get; private set; }
        public bool IsActive { get; set; }

        public ExecutingTask(string message, bool isActive = true)
        {
            Id = new Guid().ToString();
            Message = message;
            IsActive = true;
        }

        public static readonly ExecutingTask Ready = new ExecutingTask("Ready");
    }

    public static class TaskExecutionMonitor
    {
        public static void AttachMonitoring(ExecutingTask task)
        {
            Messenger.Instance.NotifyColleagues(MessageType.ExecutionStatus, task);
        }

        public static void DetachMonitoring(ExecutingTask task)
        {
            task.IsActive = false;
            Messenger.Instance.NotifyColleagues(MessageType.ExecutionStatus, task);
        }

        public static void MonitorOneTime(ExecutingTask task)
        {
            Messenger.Instance.NotifyColleagues(MessageType.ExecutionStatus, task);
            task.IsActive = false;
            Task.Delay(TimeSpan.FromSeconds(1));
            Messenger.Instance.NotifyColleagues(MessageType.ExecutionStatus, task);
        }
    }
}
