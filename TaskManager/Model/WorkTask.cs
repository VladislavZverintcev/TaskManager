using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace TaskManager.Model
{
    public class WorkTask : INotifyPropertyChanged
    {
        #region Fields
        string name = "";
        string description = "";
        int status;
        DateTime registredTime;
        TimeSpan toCompleteTime = new TimeSpan();
        ObservableCollection<DateTime> executionHistory = new ObservableCollection<DateTime>();
        DateTime finishTime;
        WorkTask upLinkTask = null;
        ObservableCollection<string> statusList = new ObservableCollection<string> { allStatus[0], allStatus[1], allStatus[2] };
        static string[] allStatus = new string[] { Properties.Resources.taskStatus0, Properties.Resources.taskStatus1, 
            Properties.Resources.taskStatus2, Properties.Resources.taskStatus3 };
        ObservableCollection<string> executors = new ObservableCollection<string>();
        ObservableCollection<WorkTask> subWorkTasks = new ObservableCollection<WorkTask>();
        private bool _IsSelected;
        private bool _IsExpanded;
        #endregion Fields

        #region Properties
        public int Id { get; set; }
        public string Name
        {
            get { return name; }
            set { name = value; UpLinkTask?.SortSubs(); 
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name")); }
        }
        public string Description
        {
            get { return description; }
            set { description = value; }
        }
        public ObservableCollection<string> Executors
        {
            get { return executors; }
            set { executors = value; 
                ExecutorsJsonS = JsonWorker.GetSerializedObj(Executors);
                var rep = new DB.TasksRepository(); rep.SetTaskParam(this, "ExecutorsJsonS", ExecutorsJsonS);
            }
        }
        public DateTime RegistredTime
        {
            get { return registredTime; }
            set { registredTime = value; }
        }
        public TimeSpan ToCompleteTime
        {
            get { return toCompleteTime; }
            set { toCompleteTime = value;
                SetStatus(status);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ToCompleteTime")); 
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ToCompleteTimeWithSubs"));
                RefreshUpLinks(); }
        }
        public WorkTask UpLinkTask { get; set; }
        public int Status
        {
            get { return status; }
            set
            {
                status = SetStatus(value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Status"));
                RefreshUpLinks();
            }
        }
        public TimeSpan ToCompleteTimeWithSubs
        {
            get { return GetToCompleteTimeWithSubs(); }
        }
        public TimeSpan ExecutionTime
        {
            get { return GetExecutionTime(); }
        }
        public TimeSpan ExecutionTimeWithSubs
        {
            get { return GetExecutionTimeWithSubs(); }
        }
        public DateTime FinishTime
        {
            get { return finishTime; }
            set { finishTime = value; 
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FinishTime")); }
        }
        public ObservableCollection<WorkTask> SubWorkTasks
        {
            get { return subWorkTasks; }
            set { subWorkTasks = value; }
        }
        public ObservableCollection<string> StatusList
        {
            get { return statusList; }
            set 
            {
                if (value != statusList)
                {
                    statusList = value;
                    SetStatusListJson();
                }
            }
        }
        [NotMapped]
        public bool IsSelected
        {
            get { return _IsSelected; }
            set { _IsSelected = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsSelected")); }
        }
        [NotMapped]
        public bool IsExpanded
        {
            get { return _IsExpanded; }
            set { _IsExpanded = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsExpanded")); 
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ToCompleteTimeWithSubs"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ExecutionTimeWithSubs"));
            }
        }

        #region JSON Properties
        public string ExecutorsJsonS { get; set; }
        public string ExecutionHistoryJsonS { get; set; }
        public string StatusListJsonS { get; set; }
        #endregion JSON Properties

        #endregion Properties

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion Events

        #region Constructors
        public WorkTask()
        {
            
        }
        public WorkTask(string name_, TimeSpan toCompleteTime_, ObservableCollection<string> executors_, string description_ = "")
        {
            Name = name_;
            Description = description_;
            Executors = executors_;
            ToCompleteTime = toCompleteTime_;
            RegistredTime = DateTime.Now;
            Status = 0;
        }
        public WorkTask(string name_, string description_, byte status_, DateTime registredTime_, TimeSpan toCompleteTime_, ObservableCollection<DateTime> executionHistory_, DateTime finishTime_, ObservableCollection<string> executors_)
        {
            Name = name_;
            Description = description_;
            status = status_;
            Executors = executors_;
            ToCompleteTime = toCompleteTime_;
            registredTime = registredTime_;
            executionHistory = executionHistory_;
            finishTime = finishTime_;
        }
        #endregion Constructors

        #region Methods

        #region Private
        int SetStatus(int value)
        {
            #region Check if Field TableHours is empty
            if (ToCompleteTime.TotalHours == 0)
            {
                if (status != 0 || value != 0)
                {
                    status = 0;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Status"));
                    value = 0;
                }
                if (StatusList.Contains(allStatus[3]))
                {
                    StatusList.Remove(allStatus[3]);
                    SetStatusListJson();
                }
                if (StatusList.Contains(allStatus[2]))
                {
                    StatusList.Remove(allStatus[2]);
                    SetStatusListJson();
                }
                if (StatusList.Contains(allStatus[1]))
                {
                    StatusList.Remove(allStatus[1]);
                    SetStatusListJson();
                }
            }
            else
            {
                if (!StatusList.Contains(allStatus[1]))
                {
                    StatusList.Add(allStatus[1]);
                    SetStatusListJson();
                }
                if (!StatusList.Contains(allStatus[2]))
                {
                    StatusList.Add(allStatus[2]);
                    SetStatusListJson();
                }
            }
            #endregion Check if Field TableHours is empty

            if (value == 0)
            {
                if (StatusList.Contains(allStatus[3]))
                {
                    StatusList.Remove(allStatus[3]);
                    SetStatusListJson();
                }
            }
            else
            {
                if (IsSubCompleted())
                {
                    if (!StatusList.Contains(allStatus[3]))
                    {
                        StatusList.Add(allStatus[3]);
                        SetStatusListJson();
                    }
                }
                else
                {
                    if (status == 3 || value == 3)
                    {
                        status = 1;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Status"));
                        value = 1;
                    }
                    if (StatusList.Contains(allStatus[3]))
                    {
                        StatusList.RemoveAt(3);
                        SetStatusListJson();
                    }
                }
            }
            ControlOfExecutionHistory(value);
            if(status != value && value == 3)
            {
                SetFinishTime(DateTime.Now);
            }
            if(value != 3)
            {
                SetFinishTime(new DateTime());
            }
            var rep = new DB.TasksRepository();
            rep.UpdateStatusTask(this, value);
            return value;
        }
        bool CanComplete()
        {
            if (status == 0)
            {
                return false;
            }
            return IsSubCompleted();
        }
        bool IsSubCompleted()
        {
            foreach (WorkTask wtask in subWorkTasks)
            {
                if(wtask.status != 3)
                {
                    return false;
                }
                if(wtask.IsSubCompleted() == false)
                {
                    return false;
                }
            }
            return true;
        }
        void ControlOfExecutionHistory(int newStatus)
        {
            //Fixing changes of status in executionHistory 

            if(status == 0 && newStatus == 1)
            {
                executionHistory.Add(DateTime.Now);
                SaveChangeToDB();
            }
            if (status == 1 && newStatus == 0)
            {
                executionHistory.Add(DateTime.Now);
                SaveChangeToDB();
            }
            if (status == 1 && newStatus == 2)
            {
                executionHistory.Add(DateTime.Now);
                SaveChangeToDB();
            }
            if (status == 2 && newStatus == 1)
            {
                executionHistory.Add(DateTime.Now);
                SaveChangeToDB();
            }
            if (status == 1 && newStatus == 3)
            {
                executionHistory.Add(DateTime.Now);
                SaveChangeToDB();
            }
            if (status == 3 && newStatus == 1)
            {
                executionHistory.Add(DateTime.Now);
                SaveChangeToDB();
            }

            void SaveChangeToDB()
            {
                ExecutionHistoryJsonS = JsonWorker.GetSerializedObj(executionHistory);
                var rep = new DB.TasksRepository(); rep.SetTaskParam(this, "ExecutionHistoryJsonS", ExecutionHistoryJsonS);
            }
        }
        TimeSpan GetExecutionTime()
        {
            TimeSpan result = new TimeSpan();
            
            if(executionHistory.Count != 0)
            {
                if (executionHistory.Count % 2 == 0)
                {
                    for (int i = 0; i < executionHistory.Count; i += 2)
                    {
                        result = result.Add(executionHistory[i + 1] - executionHistory[i]);
                    }
                    return result;
                }
                else
                {
                    if(executionHistory.Count == 1)
                    {
                        result = result.Add(DateTime.Now - executionHistory[0]);
                    }
                    else
                    {
                        for (int i = 0; i < executionHistory.Count - 1; i += 2)
                        {
                            result = result.Add(executionHistory[i + 1] - executionHistory[i]);
                        }
                        result = result.Add(DateTime.Now - executionHistory[executionHistory.Count - 1]);
                    }
                }
            }
            return result;
        }
        void RefreshUpLinks()
        {
            if (UpLinkTask != null)
            {
                UpLinkTask.SetStatus(UpLinkTask.Status);
                UpLinkTask.ToCompleteTime = UpLinkTask.toCompleteTime;
                UpLinkTask.RefreshUpLinks();
            }
        }
        TimeSpan GetSubToCompeteTime()
        {
            TimeSpan result = new TimeSpan();
            foreach(WorkTask subTask in SubWorkTasks)
            {
                if(subTask.IsExpanded)
                {
                    result = result.Add(subTask.toCompleteTime);
                    result = result.Add(subTask.GetSubToCompeteTime());
                }
                else
                {
                    result = result.Add(subTask.ToCompleteTimeWithSubs);
                }
            }
            return result;
        }
        TimeSpan GetSubExecutionTime()
        {
            TimeSpan result = new TimeSpan();
            foreach (WorkTask subTask in SubWorkTasks)
            {
                if (subTask.IsExpanded)
                {
                    result = result.Add(subTask.ExecutionTime);
                    result = result.Add(subTask.GetSubExecutionTime());
                }
                else
                {
                    result = result.Add(subTask.ExecutionTimeWithSubs);
                }
            }
            return result;
        }
        void SetStatusListJson()
        {
            if(StatusListJsonS != JsonWorker.GetSerializedObj(StatusList))
            {
                StatusListJsonS = JsonWorker.GetSerializedObj(StatusList);
                var rep = new DB.TasksRepository(); rep.SetTaskParam(this, "StatusListJsonS", StatusListJsonS);
            }
        }
        void SetFinishTime(DateTime newDataTime)
        {
            FinishTime = newDataTime;
            var rep = new DB.TasksRepository();
            rep.SetTaskParam(this, "FinishTime", FinishTime);
        }
        int Search(ObservableCollection<WorkTask> collection, int startIndex, WorkTask other)
        {
            for (int i = startIndex; i < collection.Count; i++)
            {
                if (other.Equals(collection[i]))
                    return i;
            }

            return -1;
        }
        #endregion Private

        #region Public
        public bool ItsLineTaskNameExist(string taskName)
        {
            if (Name == taskName)
            {
                return true;
            }
            if (SubWorkTasks != null || SubWorkTasks.Count != 0)
            {
                foreach (WorkTask subTask in SubWorkTasks)
                {
                    if (subTask.Name == taskName)
                    {
                        return true;
                    }
                    if (subTask.ItsLineTaskNameExist(taskName))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public void AddTask(WorkTask subTask)
        {
            subTask.UpLinkTask = this;
            SubWorkTasks.Add(subTask);
            SortSubs();
            Status = status;
        }
        public void ExpandUpLinks()
        {
            if(upLinkTask != null)
            {
                upLinkTask.IsExpanded = true;
                upLinkTask.ExpandUpLinks();
            }
        }
        public void DeleteSubTask(string taskName)
        {
            if (SubWorkTasks != null)
            {
                foreach (WorkTask wTask in SubWorkTasks)
                {
                    if (wTask.Name == taskName)
                    {
                        SubWorkTasks.Remove(wTask);
                        break;
                    }
                }
                Status = status;
            }
        }
        public TimeSpan GetToCompleteTimeWithSubs()
        {
            if (IsExpanded)
            { return toCompleteTime; }
            else
            {
                TimeSpan result = new TimeSpan();
                result = ToCompleteTime;
                result = result.Add(GetSubToCompeteTime());
                return result;
            }
        }
        public TimeSpan GetExecutionTimeWithSubs()
        {
            if (IsExpanded)
            { return ExecutionTime; }
            else
            {
                TimeSpan result = new TimeSpan();
                result = ExecutionTime;
                result = result.Add(GetSubExecutionTime());
                return result;
            }
        }
        public void RefreshExecutionTime()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ExecutionTime"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ExecutionTimeWithSubs"));
            foreach (WorkTask subTask in SubWorkTasks)
            {
                subTask.RefreshExecutionTime();
            }
        }
        public WorkTask GetTaskByName(string taskName)
        {
            if(Name == taskName)
            {
                return this;
            }
            foreach(WorkTask subTask in SubWorkTasks)
            {
                if(subTask.name == taskName)
                {
                    return subTask;
                }
                if (subTask.SubWorkTasks.Count != 0)
                {
                    WorkTask ftask = subTask.GetTaskByName(taskName);
                    if (ftask != null)
                    {
                        return ftask;
                    }
                }
            }
            return null;
        }
        public void LoadCollectionProperty()
        {
            #region Deserialize JSon strings
            if (ExecutorsJsonS != null && ExecutorsJsonS != "")
            {
                Executors = ((Newtonsoft.Json.Linq.JArray)(JsonWorker.GetDeserializedObj(ExecutorsJsonS))).ToObject<ObservableCollection<string>>();
            }
            if (ExecutionHistoryJsonS != null && ExecutionHistoryJsonS != "")
            {
                executionHistory = ((Newtonsoft.Json.Linq.JArray)(JsonWorker.GetDeserializedObj(ExecutionHistoryJsonS))).ToObject<ObservableCollection<DateTime>>();
            }
            if (StatusListJsonS != null && StatusListJsonS != "")
            {
                StatusList = ((Newtonsoft.Json.Linq.JArray)(JsonWorker.GetDeserializedObj(StatusListJsonS))).ToObject<ObservableCollection<string>>();
            }
            #endregion Deserialize JSon strings
        }
        public void SortSubs()
        {
            if(SubWorkTasks != null && SubWorkTasks.Count != 0)
            {
                List<WorkTask> sorted = SubWorkTasks.OrderBy(x => x.Name).ToList();
                int ptr = 0;
                while (ptr < sorted.Count - 1)
                {
                    if (!SubWorkTasks[ptr].Equals(sorted[ptr]))
                    {
                        int idx = Search(SubWorkTasks, ptr + 1, sorted[ptr]);
                        SubWorkTasks.Move(idx, ptr);
                    }
                    ptr++;
                }
            }
        }
        public void SortSubsCascad()
        {
            SortSubs();
            if(SubWorkTasks?.Count > 0)
            {
                foreach(var sTask in subWorkTasks)
                {
                    sTask.SortSubsCascad();
                }
            }
        }
        #endregion Public

        #endregion Methods
    }
}
