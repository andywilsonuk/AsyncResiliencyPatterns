using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncResiliencyPatterns.Tests
{
    class PassThroughInvoker : ResiliencyCommandInvoker
    {
        public bool Verify { get; private set; }

        public async Task ExecuteAsync(Func<Task> task)
        {
            Func<Task<bool>> func = async () =>
            {
                await task();
                return true;
            };

            await this.ExecuteAsync<bool>(func);
        }

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> task)
        {
            this.Verify = true;
            return await task();
        }
    }
}
