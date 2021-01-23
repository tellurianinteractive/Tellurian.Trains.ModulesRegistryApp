using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModulesRegistry.Data
{
    public partial class User
    {
        public virtual Person Person { get; set; }
    }
}
