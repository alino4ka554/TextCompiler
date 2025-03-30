using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextCompiler
{
    public class Error
    {
        public string Message;
        public string BeginOfError = null;
        public int Position = 0;
        public Error(string message, string beginOfError, int position)
        {
            this.Message = message;
            this.BeginOfError = beginOfError;
            this.Position = position;
        }
    }
}
