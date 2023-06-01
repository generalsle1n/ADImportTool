using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADImportTool.Model
{
    public class PreStartSync
    {
        public string Path { get; set; }
        public string Arguments { get; set; }
        public bool RequiredForSync { get; set; }
        public bool Enabled { get; set; }
    }
}
