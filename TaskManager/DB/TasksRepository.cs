using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.DB
{
    class TasksRepository
    {
        public static void CheckConnection()
        {
            using (var context = new DBContext())
            {
                try
                {
                    context.Database.Connection.Open();
                    context.Database.Connection.Close();
                }
                catch
                {
                    throw;
                }
            }
        }
        public IEnumerable<Model.WorkTask> GetTasks()
        {
            try
            {
                using (var context = new DBContext())
                {
                    IEnumerable<Model.WorkTask> resutl = context.WorkTasks.ToList();
                    foreach (Model.WorkTask wTask in resutl)
                    {
                        wTask.LoadCollectionProperty();
                    }
                    return context.WorkTasks.ToList();
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        public void AddTask(Model.WorkTask task, Model.WorkTask parentTask = null)
        {
            try
            {
                using (var context = new DBContext())
                {
                    if (parentTask == null)
                    {
                        context.WorkTasks.Add(task);
                        context.SaveChanges();
                    }
                    else
                    {
                        context.WorkTasks.Attach(parentTask);
                        task.UpLinkTask = parentTask;
                        context.WorkTasks.Add(task);
                        context.SaveChanges();
                    }
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        public void DelTask(int idOfTask)
        {
            try
            {
                using (var context = new DBContext())
                {
                    Model.WorkTask deletedTask = context.WorkTasks.Where(c => c.Id == idOfTask).FirstOrDefault();
                    if (deletedTask != null)
                    {
                        context.Entry(deletedTask).Collection(c => c.SubWorkTasks).Load();
                        if (deletedTask.SubWorkTasks.Count == 0)
                        {
                            context.Entry(deletedTask).State = EntityState.Deleted;
                            context.SaveChanges();
                        }
                        else
                        {
                            foreach (int i in GetAllRefDownTaskIDIncludeCurrent(idOfTask))
                            {
                                deletedTask = context.WorkTasks.Where(c => c.Id == i).FirstOrDefault();
                                context.Entry(deletedTask).State = EntityState.Deleted;
                            }
                            context.SaveChanges();
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        IEnumerable<int> GetAllRefDownTaskIDIncludeCurrent(int currentTaskID)
        {
            try
            {
                List<int> resultId = new List<int>();
                resultId.Add(currentTaskID);
                using (var context = new DBContext())
                {
                    Model.WorkTask currentTask = context.WorkTasks.Where(c => c.Id == currentTaskID).FirstOrDefault();
                    if (currentTask != null)
                    {
                        context.Entry(currentTask).Collection(c => c.SubWorkTasks).Load();
                        if (currentTask.SubWorkTasks.Count == 0)
                        {
                            return resultId;
                        }
                        else
                        {
                            foreach (Model.WorkTask subTask in currentTask.SubWorkTasks)
                            {
                                resultId.AddRange(GetAllRefDownTaskIDIncludeCurrent(subTask.Id));
                            }
                            return resultId;
                        }
                    }
                }
                return resultId;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// Setting property of Task, not for Status property
        /// </summary>
        /// <param name="editedTask"></param>
        /// <param name="nameOfParam"></param>
        /// <param name="value"></param>
        public void SetTaskParam(Model.WorkTask editedTask, string nameOfParam, object value)
        {
            if(nameOfParam != "Status")
            try
            {
                using (var context = new DBContext())
                {
                    if (context.WorkTasks.Any(o => o.Id == editedTask.Id))
                    {
                        context.WorkTasks.Attach(editedTask);
                        PropertyInfo pi = typeof(Model.WorkTask).GetProperty(nameOfParam);
                        if (pi != null)
                        {
                            if (pi.PropertyType != value.GetType())
                            {
                                    throw new Exceptions.CustomException(Properties.Resources.messageErrorPropertyValueType);
                            }
                            else
                            {
                                try
                                {
                                    pi.SetValue(editedTask, value);
                                    context.Entry(editedTask).State = EntityState.Modified;
                                    context.SaveChanges();
                                }
                                catch (Exception ex)
                                {
                                    throw ex;
                                }
                            }
                        }
                        else
                        {
                            throw new Exceptions.CustomException(Properties.Resources.messageErrorPropertyNotExist);

                        }
                    }
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        public void UpdateStatusTask(Model.WorkTask editedTask, int newStatus)
        {
            try
            {
                using (var context = new DBContext())
                {
                    if (context.WorkTasks.Any(o => o.Id == editedTask.Id))
                    {
                        try
                        {
                            int numberofrow = context.Database.ExecuteSqlCommand($"UPDATE {WorkTaskMap.nameOfTaskTable} SET Status='{newStatus}' WHERE Id={editedTask.Id}");
                        }
                        catch(Exception ex)
                        {
                            throw ex;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
    }
}
