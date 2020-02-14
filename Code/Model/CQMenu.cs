using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.Model
{
    public enum CQMenu : int
    {
        App = 0x61B0,
        Flow = 0x61B1,
        Log = 0x61B4,
        AppFolder = 0x61B2,
        About = 0x6275,
        Reset = 0x6219,
        OnlineStatic_QMe = 0x620C,
        OnlineStatic_Online = 0x620D,
        OnlineStatic_PlayGame = 0x620E,
        OnlineStatic_Hidden = 0x6211,
        OnlineStatic_Offline = 0x6213,
        FastRestart = 0x6216,
        FlowType1 = 0xBF69,
        FlowType2 = 0xBF6A,
    }

}
