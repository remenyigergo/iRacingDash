using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRacingDash.Enums
{
    public enum SessionTypeEnum
    {
        [Description("Offline Testing")]
        OfflineTesting = 1,
        [Description("Practice")]
        Practice = 2,
        [Description("Qualify")]
        Qualify = 3,
        [Description("Warmup")]
        WarmUp = 4,
        [Description("Race")]
        Race = 5,
    }
}
