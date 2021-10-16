using System;
using System.Data.SqlClient;
using System.Data.Entity;

namespace TaskManager.DB
{
    class DBContext : DbContext
    {
        public DBContext() : base("name=DBContext")
        {
            if(!Database.Exists())
            {
                try
                {
                    Database.Create();
                }
                catch (System.Data.DataException)
                {
                    throw;
                }
            }
            try
            {
                Database.Initialize(false);
            }
            catch (SqlException)
            {
                throw;
            }
            catch (System.Data.DataException)
            {
                throw;
            }

        }
        public DbSet<Model.WorkTask> WorkTasks { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new WorkTaskMap());
            modelBuilder.Properties<DateTime>().Configure(c => c.HasColumnType("datetime2"));
        }
    }
}
