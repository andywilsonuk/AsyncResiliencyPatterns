using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AsyncResiliencyPatterns
{
    /// <summary>
    /// Occurs when concurrent executions is above the maximum.
    /// </summary>
    [Serializable]
    public class ThrottleLimitException : Exception
    {
        public ThrottleLimitException()
            : base("Too many concurrent executions.")
        {
        }

        public ThrottleLimitException(string message)
            : base(message)
        {
        }

        public ThrottleLimitException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected ThrottleLimitException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
