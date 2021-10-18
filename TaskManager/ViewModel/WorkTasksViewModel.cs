using System;
using System.Linq;
using System.Windows;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using System.Collections.ObjectModel;
using System.Data.SqlClient;

namespace TaskManager.ViewModel
{
    public class WorkTasksViewModel : DependencyObject
    {
        ListCollectionView view;

        #region TaskCollection
        public ICollectionView TaskItems
        {
            get { return (System.ComponentModel.ICollectionView)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("TaskItems", typeof(System.ComponentModel.ICollectionView), typeof(WorkTasksViewModel), new PropertyMetadata(null));
        #endregion TaskCollection

        #region Constructors
        public WorkTasksViewModel()
        {
            //SQL connection Test
            try
            {
                DB.TasksRepository.CheckConnection();
            }
            catch (System.Data.DataException)
            {
                System.Windows.MessageBox.Show(Properties.Resources.messageErrorDataException,
                    TaskManager.Properties.Resources.titleError, MessageBoxButton.OK, MessageBoxImage.Error);
                App.Current.Shutdown();
            }
            catch (SqlException)
            {
                System.Windows.MessageBox.Show(Properties.Resources.messageErrorSqlConnection,
                    TaskManager.Properties.Resources.titleError, MessageBoxButton.OK, MessageBoxImage.Error);
                App.Current.Shutdown();
            }
            //
            try
            {
                Model.TasksHolder th = new Model.TasksHolder(true);
                TaskItems = CollectionViewSource.GetDefaultView(Model.TasksHolder.TaskList);
                view = (ListCollectionView)TaskItems;
                view.CustomSort = new CustomSorter();
            }
            catch(Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, Properties.Resources.titleError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            TaskTableIsVisible = Visibility.Hidden;
        }
        #endregion Constructors

        #region SelectedTask
        Model.WorkTask selectedWorkTask;
        public Model.WorkTask SelectedWorkTask
        {
            get { return selectedWorkTask; }
            set { if (selectedWorkTask != null) { SelectedWorkTask.PropertyChanged -= OnFinishTime_PropertyChanged; } 
                selectedWorkTask = value; SetVisabilityTableTask(); UpdateTableProperties(); }
        }
        #endregion SelectedTask

        #region TaskTableVisible
        public Visibility TaskTableIsVisible
        {
            get { return (Visibility)GetValue(TaskTableIsVisibleProperty); }
            set { SetValue(TaskTableIsVisibleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TaskTableIsVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TaskTableIsVisibleProperty =
            DependencyProperty.Register("TaskTableIsVisible", typeof(Visibility), typeof(WorkTasksViewModel), new PropertyMetadata(null));

        void SetVisabilityTableTask()
        {
            if (SelectedWorkTask != null)
            {
                TaskTableIsVisible = Visibility.Visible;
            }
            else { TaskTableIsVisible = Visibility.Hidden; }
        }

        #endregion TaskTableVisible

        #region TreeView

        #region EmptyClickAndDeSelect
        RelayCommand _emptyClickComm;
        public ICommand EmptyClickComm
        {
            get
            {
                if (_emptyClickComm == null)
                {
                    _emptyClickComm = new RelayCommand(() => DeSelect(), true);
                }
                return _emptyClickComm;
            }
        }
        void DeSelect()
        {
            if (SelectedWorkTask != null)
            {
                SelectedWorkTask.IsSelected = false;
            }
        }
        #endregion EmptyClickAndDeSelect

        #region AddTask
        RelayCommand _addTaskComm;
        public ICommand AddTaskComm
        {
            get
            {
                if (_addTaskComm == null)
                {
                    _addTaskComm = new RelayCommand(() => AddTask(), true);
                }
                return _addTaskComm;
            }
        }
        void AddTask()
        {
            try
            {
                if (SelectedWorkTask != null)
                {
                    Model.WorkTask newTask = new Model.WorkTask(Model.TasksHolder.GetNewName(), new TimeSpan(), new ObservableCollection<string>());
                    Model.TasksHolder.AddTask(newTask, SelectedWorkTask);
                }
                else
                {
                    Model.WorkTask newTask = new Model.WorkTask(Model.TasksHolder.GetNewName(), new TimeSpan(), new ObservableCollection<string>());
                    Model.TasksHolder.AddTask(newTask);
                }
            }
            catch(Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, Properties.Resources.titleError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion AddTask

        #region DelTask
        public ICommand DelTaskComm
        {
            get
            {
                return new Commands.VMCommands((obj) =>
                    {
                        DelTask();
                    }, (obj) => CanDel());
            }
        }
        void DelTask()
        {
            if (SelectedWorkTask != null)
            {
                try
                {
                    if (SelectedWorkTask.UpLinkTask != null)
                    {
                        int currentTaskID = SelectedWorkTask.Id;
                        Model.WorkTask parentTask = Model.TasksHolder.GetTaskByName(SelectedWorkTask.UpLinkTask.Name);
                        parentTask.DeleteSubTask(SelectedWorkTask.Name);
                        parentTask.IsSelected = true;
                        var rep = new DB.TasksRepository();
                        rep.DelTask(currentTaskID);
                    }
                    else
                    {
                        var rep = new DB.TasksRepository();
                        rep.DelTask(SelectedWorkTask.Id);
                        Model.TasksHolder.TaskList.Remove(SelectedWorkTask);
                        DeSelect();
                    }
                }
                catch(Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message, Properties.Resources.titleError, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        bool CanDel()
        {
            if (SelectedWorkTask != null)
            {
                return true;
            }
            else { return false; }
        }
        #endregion DelTask

        #endregion TreeView

        #region TableProperties

        #region Properties
        public bool ErrTriggerTName
        {
            get { return (bool)GetValue(ErrTriggerTNameProperty); }
            set { SetValue(ErrTriggerTNameProperty, value); }
        }
        // Using a DependencyProperty as the backing store for errTriggerTName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ErrTriggerTNameProperty =
            DependencyProperty.Register("ErrTriggerTName", typeof(bool), typeof(WorkTasksViewModel), new PropertyMetadata(true));
        public bool ErrTriggerTHours
        {
            get { return (bool)GetValue(ErrTriggerTHoursProperty); }
            set { SetValue(ErrTriggerTHoursProperty, value); }
        }
        // Using a DependencyProperty as the backing store for errTriggerTName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ErrTriggerTHoursProperty =
            DependencyProperty.Register("ErrTriggerTHours", typeof(bool), typeof(WorkTasksViewModel), new PropertyMetadata(true));


        public string TaskName
        {
            get { return (string)GetValue(TaskNameProperty); }
            set { SetValue(TaskNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TaskName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TaskNameProperty =
            DependencyProperty.Register("TaskName", typeof(string), typeof(WorkTasksViewModel), new PropertyMetadata(null));

        public string TaskDescription
        {
            get { return (string)GetValue(TaskDescriptionProperty); }
            set { SetValue(TaskDescriptionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TaskDescription.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TaskDescriptionProperty =
            DependencyProperty.Register("TaskDescription", typeof(string), typeof(WorkTasksViewModel), new PropertyMetadata(null));

        public DateTime? TaskRegTime
        {
            get { return (DateTime?)GetValue(TaskRegTimeProperty); }
            set { SetValue(TaskRegTimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TaskRegTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TaskRegTimeProperty =
            DependencyProperty.Register("TaskRegTime", typeof(DateTime?), typeof(WorkTasksViewModel), new PropertyMetadata(null));

        public string TableHours
        {
            get { return (string)GetValue(TableHoursProperty); }
            set { SetValue(TableHoursProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TableHours.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TableHoursProperty =
            DependencyProperty.Register("TableHours", typeof(string), typeof(WorkTasksViewModel), new PropertyMetadata(null));

        public DateTime? TaskFinishTime
        {
            get { return (DateTime?)GetValue(TaskFinishTimeProperty); }
            set { SetValue(TaskFinishTimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TaskFinishTimeProperty =
            DependencyProperty.Register("TaskFinishTime", typeof(DateTime?), typeof(WorkTasksViewModel), new PropertyMetadata(null));

        public ObservableCollection<string> TableExecutorsList
        {
            get { return (ObservableCollection<string>)GetValue(TableExecutorsListProperty); }
            set { SetValue(TableExecutorsListProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TableExecutorsList.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TableExecutorsListProperty =
            DependencyProperty.Register("TableExecutorsList", typeof(ObservableCollection<string>), typeof(WorkTasksViewModel), new PropertyMetadata(null));

        public string TableNewPerson
        {
            get { return (string)GetValue(TableNewPersonProperty); }
            set { SetValue(TableNewPersonProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TableNewPerson.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TableNewPersonProperty =
            DependencyProperty.Register("TableNewPerson", typeof(string), typeof(WorkTasksViewModel), new PropertyMetadata(null));
        #endregion Properties

        #region AddPerson

        public ICommand AddPersonComm
        {
            get
            {
                return new Commands.VMCommands((obj) =>
                {
                    AddPerson();
                }, (obj) => CanAddPerson());
            }
        }
        void AddPerson()
        {
            if (SelectedWorkTask != null)
            {
                if (TableExecutorsList != null)
                {
                    if (TableNewPerson != null && TableNewPerson != "")
                        TableExecutorsList.Add(TableNewPerson);
                    TableNewPerson = "";
                }
            }
        }
        bool CanAddPerson()
        {
            if (SelectedWorkTask == null || TableExecutorsList == null)
            {
                return false;
            }
            if (TableNewPerson != null && TableNewPerson != "" && !TableExecutorsList.Contains(TableNewPerson) && (TableNewPerson.Length - TableNewPerson.Replace(" ", "").Length) != TableNewPerson.Length)
            {
                return true;
            }
            return false;
        }

        #endregion AddPerson

        #region DelPerson

        ICommand _delPersonComm;
        public Commands.VMCommands DelPersonComm => (Commands.VMCommands)(_delPersonComm ?? (_delPersonComm = new Commands.VMCommands(parameter =>
        {
            if (parameter is string)
                DelPerson((string)parameter);
        }))); 
        void DelPerson(string namePerson)
        {
            if (SelectedWorkTask != null && namePerson != "")
            {
                TableExecutorsList.Remove(namePerson);
            }
        }
        #endregion DelPerson

        void UpdateTableProperties()
        {
            if(SelectedWorkTask == null)
            {
                TaskName = null;
                TaskDescription = null;
                TaskRegTime = null;
                TableHours = null;
                TaskFinishTime = null;
                TableExecutorsList = null;
                TableNewPerson = null;
            }
            else
            {
                SelectedWorkTask.PropertyChanged += OnFinishTime_PropertyChanged;
                TaskName = SelectedWorkTask.Name;
                TaskDescription = SelectedWorkTask.Description;
                TaskRegTime = SelectedWorkTask.RegistredTime;
                if(SelectedWorkTask.FinishTime != new DateTime())
                {
                    TaskFinishTime = SelectedWorkTask.FinishTime;
                }
                else { TaskFinishTime = null; }
                if(SelectedWorkTask.ToCompleteTime.TotalHours != 0)
                {
                    TableHours = SelectedWorkTask.ToCompleteTime.TotalHours.ToString();
                }
                else { TableHours = ""; }
                if(SelectedWorkTask.Executors != null)
                {
                    TableExecutorsList = new ObservableCollection<string>(SelectedWorkTask.Executors);
                }
            }
        }

        #region SaveTableTask


        public ICommand SaveTableTaskComm
        {
            get
            {
                return new Commands.VMCommands((obj) =>
                {
                    SaveTableTask();
                }, (obj) => CanSaveTableTaskChanges());
            }
        }
        void SaveTableTask()
        {
            if (SelectedWorkTask != null)
            {
                try
                {
                    if (SelectedWorkTask.Name != TaskName)
                    {
                        SelectedWorkTask.Name = TaskName;
                        var rep = new DB.TasksRepository(); rep.SetTaskParam(SelectedWorkTask, "Name", TaskName);
                    }
                    if (SelectedWorkTask.Description != TaskDescription)
                    {
                        SelectedWorkTask.Description = TaskDescription;
                        var rep = new DB.TasksRepository(); rep.SetTaskParam(SelectedWorkTask, "Description", TaskDescription);
                    }
                    if (SelectedWorkTask.ToCompleteTime.Hours.ToString() != TableHours)
                    {
                        SelectedWorkTask.ToCompleteTime = TimeSpan.FromHours(Convert.ToDouble(TableHours));
                        var rep = new DB.TasksRepository(); rep.SetTaskParam(SelectedWorkTask, "ToCompleteTime", TimeSpan.FromHours(Convert.ToDouble(TableHours)));
                    }
                    if ((TableExecutorsList.Where(a => !SelectedWorkTask.Executors.Contains(a)).ToList()).Count > 0 || TableExecutorsList.Count != SelectedWorkTask.Executors.Count)
                    {
                        SelectedWorkTask.Executors = new ObservableCollection<string>(TableExecutorsList);
                        var rep = new DB.TasksRepository(); rep.SetTaskParam(SelectedWorkTask, "Executors", new ObservableCollection<string>(TableExecutorsList));
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message, Properties.Resources.titleError, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        bool CanSaveTableTaskChanges()
        {
            if (SelectedWorkTask == null)
            {
                return false;
            }

            //Checks TName and THours
            bool checkresult = true;
            if (TaskName != null && TaskName != SelectedWorkTask.Name)
            {
                if ((TaskName.Length - TaskName.Replace(" ", "").Length) == TaskName.Length 
                    || Model.TasksHolder.TaskNameExist(TaskName))
                {
                    ErrTriggerTName = false;
                    checkresult = false;
                }
                else { ErrTriggerTName = true; }
            }
            else { ErrTriggerTName = true; }

            if(TableHours != null && TableHours != SelectedWorkTask.ToCompleteTime.TotalHours.ToString())
            {
                if (TableHours == "0" || TableHours == "" || TableHours.Contains(" "))
                {
                    ErrTriggerTHours = false;
                    checkresult = false;
                }
                else { ErrTriggerTHours = true; }
            }
            else { ErrTriggerTHours = true; }
            if(!checkresult) { return false; }

            //CheckForChanges and set key to save
            if (TaskName != SelectedWorkTask.Name)
            { return true; }
            if (TaskDescription != SelectedWorkTask.Description)
            { return true; }

            if (TableHours != SelectedWorkTask.ToCompleteTime.TotalHours.ToString())
            { return true; }

            if(TableExecutorsList != null)
            {
                if ((TableExecutorsList.Where(a => !SelectedWorkTask.Executors.Contains(a)).ToList()).Count > 0 
                    || TableExecutorsList.Count != SelectedWorkTask.Executors.Count)
                { return true; }
            }
            

            

            return false;
        }

        #endregion SaveTableTask

        #region OnFinishTimeChanged

        private void OnFinishTime_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "FinishTime")
            {
                if(SelectedWorkTask != null)
                {
                    if (SelectedWorkTask.FinishTime != new DateTime())
                    {
                        TaskFinishTime = SelectedWorkTask.FinishTime;
                    }
                    else { TaskFinishTime = null; }
                }
            }
        }

        #endregion OnFinishTimeChanged

        #endregion TableProperties
    }
}
