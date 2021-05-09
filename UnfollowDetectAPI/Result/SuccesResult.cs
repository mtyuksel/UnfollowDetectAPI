using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UnfollowDetectAPI.Result
{
    public class SuccesResult : Result
    {
        public SuccesResult(string message) : base(true, message)
        {

        }

        public SuccesResult() : base(true)
        {

        }
    }
}
