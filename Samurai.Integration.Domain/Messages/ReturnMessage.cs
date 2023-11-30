using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Messages
{
    public class ReturnMessage
    {
        public Result Result { get; set; }
        public Exception Error { get; set; }
    }


    public class ReturnMessage<T> : ReturnMessage
    {
        public T Data { get; set; }
    }

    public enum Result
    {
        OK, 
        Error
    }
}
