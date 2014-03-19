using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapitalStrategyServer
{
    [Table(Name = "Users")]
    public class User
    {
        public String username { get; set; }
        
    }
}
