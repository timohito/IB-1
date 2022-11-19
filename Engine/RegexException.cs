using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IB1.Engine
{
    public class RegexException : Exception
    {
        public RegexException(string message)  : base(message)
        {

        }
    }
}
