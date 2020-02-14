using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UI.Model
{
    public class Message
    {
        public long Qq { get; set; }
        public long GroupId { get; set; }
        public string GroupName { get; set; }
        public string DisplayName { get; set; }
        public string Content { get; set; }
        public bool TargetSide { get; set; } = true;
    }
}
