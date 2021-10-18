using System.Collections;

namespace TaskManager.ViewModel
{
    public class CustomSorter : IComparer
    {
        public int Compare(object x, object y)
        {
            return ((Model.WorkTask)x).Name.CompareTo(((Model.WorkTask)y).Name);
        }
    }
}
