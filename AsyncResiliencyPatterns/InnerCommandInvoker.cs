using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncResiliencyPatterns
{
    internal class InnerCommandInvoker : ResiliencyCommandInvoker
    {
        public async Task<T> ExecuteAsync<T>(Func<Task<T>> command)
        {
            return await command();
        }

        public async Task ExecuteAsync(Func<Task> command)
        {
            Func<Task<bool>> func = async () =>
            {
                await command();
                return true;
            };

            await func();
        }
    }
}
