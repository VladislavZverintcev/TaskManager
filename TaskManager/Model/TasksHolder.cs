using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace TaskManager.Model
{
    public class TasksHolder
    {
        #region Fields
        static ObservableCollection<WorkTask> taskList = new ObservableCollection<WorkTask>();
        private static System.Timers.Timer refreshExecTimer;
        #endregion Fields
        #region Properties
        public int Id { get; set; }
        public static ObservableCollection<WorkTask> TaskList
        {
            get { return taskList; }
            set { taskList = value; }
        }
        #endregion Properties
        #region Constructors
        public TasksHolder(bool fromBase)
        {
            if(fromBase == true)
            {
                var rep = new DB.TasksRepository();
                IEnumerable<Model.WorkTask> tasksEnum = rep.GetTasks();
                TaskList = new ObservableCollection<WorkTask>();
                foreach (WorkTask wTask in tasksEnum)
                {
                    if(wTask.UpLinkTask == null)
                    {
                        TaskList.Add(wTask);
                    }
                }
            }
            SetRefExecTimer();
        }
        #endregion Constructors
        #region Methods

        public static bool TaskNameExist(string taskName)
        {
            if (TaskList == null || TaskList.Count == 0)
            { return false; }
            else
            {
                foreach(WorkTask wTask in TaskList)
                {
                    if(wTask.ItsLineTaskNameExist(taskName))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public static string GetNewName()
        {
            string originalStr = Properties.Resources.taskNewName;
            int i = 1;
            string result = originalStr + $"{i}";
            while (TaskNameExist(result))
            {
                i++;
                result = originalStr + $"{i}";
            }
            return result;
        }

        private void RefreshExecutionTime()
        {
            if(TaskList != null)
            {
                foreach(WorkTask wTask in TaskList)
                {
                    wTask.RefreshExecutionTime();
                }
            }
        }
        private void SetRefExecTimer()
        {
            // Create a timer with a two second interval.
            refreshExecTimer = new System.Timers.Timer(300000);
            // Hook up the Elapsed event for the timer. 
            refreshExecTimer.Elapsed += OnTimedEvent;
            refreshExecTimer.Enabled = true;
        }

        #region TaskWorks

        public static void AddTask(WorkTask addedTask, WorkTask mainTask = null)
        {
            if(mainTask == null)
            {
                TaskList.Add(addedTask);
                var rep = new DB.TasksRepository();
                rep.AddTask(addedTask, null);
            }
            else
            {
                mainTask.AddTask(addedTask);
                mainTask.IsExpanded = true;
                addedTask.ExpandUpLinks();
                var rep = new DB.TasksRepository();
                rep.AddTask(addedTask, mainTask);
            }
            
        }

        public static WorkTask GetTaskByName(string taskName)
        {
            foreach(WorkTask findTask in TaskList)
            {
                WorkTask ftask = findTask.GetTaskByName(taskName);
                if (ftask != null)
                    return ftask;
            }
            return null;
        }

        #endregion TaskWorks

        #endregion Methods

        #region Events
        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            RefreshExecutionTime();
        }
        #endregion Events
    }
}
