using System.Collections.Generic;

namespace AssemblyUpdater.Models
{
    public class ApplicationData
    {
       public List<AssemblyFileItem> AssemblyFiles { get; set; }
        public List<FrameworkProjectItem> ProjectFiles { get; set; }

    }
}
