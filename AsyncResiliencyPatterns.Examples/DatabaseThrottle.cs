using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncResiliencyPatterns.Examples
{
    /// <summary>
    /// Example 1: Prevent more than 100 concurrent requests executing against SQL Server at any one time.
    /// </summary>
    public class DatabaseThrottle
    {
        private Throttle throttle = new Throttle(100);

        public async Task ExecuteQuery()
        {
            Func<Task> func = async () => { await this.ExecuteQueryInner(); };

            await this.throttle.ExecuteAsync(func);
        }

        private async Task ExecuteQueryInner()
        {
            using (SqlConnection connection = new SqlConnection())
            {
                using (SqlCommand command = new SqlCommand("SELECT * FROM BigTable", connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
