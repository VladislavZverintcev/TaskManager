using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data.Entity;
using System.Collections.ObjectModel;

namespace TaskManager.DB
{
    class DBContext : DbContext
    {
        public DBContext() : base("name=DBContext")
        {
            try
            {
                Database.Initialize(false);
            }
            catch (SqlException)
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
