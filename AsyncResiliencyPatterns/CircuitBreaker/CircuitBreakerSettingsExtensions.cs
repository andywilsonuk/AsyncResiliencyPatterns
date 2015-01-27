using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncResiliencyPatterns
{
    internal static class CircuitBreakerSettingsExtensions
    {
        internal static bool ShouldFailureBeRecorded(this CircuitBreakerSettings settings, Exception ex)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            if (ex == null) return false;
            if (settings.ExceptionTypes.Count == 0) return true;
            Type exceptionType = ex.GetType();
            return settings.ExceptionTypes.Any(p => p.IsAssignableFrom(exceptionType));
        }
    }
}
