﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PADI_DSTM
{
    interface iData
    {
        bool Fail();
        bool Freeze();
        bool Recover();
    }
}