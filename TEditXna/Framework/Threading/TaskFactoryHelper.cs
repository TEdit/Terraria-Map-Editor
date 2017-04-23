using System;
using System.Threading.Tasks;

namespace TEdit.MvvmLight.Threading
{
    public static class TaskFactoryHelper
    {
        public static TaskFactory UiTaskFactory
        {
            get;
            private set;
        }

        public static TaskScheduler UiTaskScheduler
        {
            get;
            private set;
        }

        public static Task ExecuteUiTask(Action action)
        {
            return UiTaskFactory.StartNew(action);
        }

        public static void Initialize()
        {
            if (UiTaskFactory != null)
            {
                return;
            }

            UiTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            UiTaskFactory = new TaskFactory(UiTaskScheduler);
        }
    }
}