using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileVis.args
{
    public class ItemFindedArgs<T> : System.EventArgs where T : FileSystemInfo
    {
        public T FoundItem { get; set; }
        public FilteringSteps StepType { get; set; }
    }
}
