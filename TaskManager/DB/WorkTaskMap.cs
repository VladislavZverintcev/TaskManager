using System;
using System.Data.Entity.ModelConfiguration;

namespace TaskManager.DB
{
    public class WorkTaskMap : EntityTypeConfiguration<Model.WorkTask>
    {
        public static string nameOfTaskTable = "WorkTaskTable";
        public WorkTaskMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(50);
            this.Property(t => t.Description);
            this.Property(t => t.Status);
            this.Property(t => t.ToCompleteTime);
            this.Property(t => t.FinishTime);
            this.Property(t => t.ExecutionHistoryJsonS);
            this.Property(t => t.StatusListJsonS);

            // Table & Column Mappings
            this.ToTable(nameOfTaskTable);
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.Name).HasColumnName("Name");
            this.Property(t => t.Description).HasColumnName("Description");
            this.Property(t => t.Status).HasColumnName("Status");
            this.Property(t => t.ToCompleteTime).HasColumnName("ToCompleteTime");
            this.Property(t => t.FinishTime).HasColumnName("FinishTime");
            this.Property(t => t.ExecutionHistoryJsonS).HasColumnName("ExecutionHistoryJsonS");
            this.Property(t => t.StatusListJsonS).HasColumnName("StatusListJsonS");
        }
    }
}
