﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PADI_DSTM
{
    [Serializable]
    public class TxException : ApplicationException
    {
        public string message;

        public TxException(string m)
        {
            message = m;
        }
    }
}
