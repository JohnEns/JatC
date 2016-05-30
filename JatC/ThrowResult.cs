using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JatC
{
    public enum ThrowResult
    {
        Default = 0,
        FirstRoll = 1,
        LastRoll = 2,
        SmallStraight = 3,
        BigStraight = 4,
        Yahtzee = 5,
        Error = 6
    }
}
