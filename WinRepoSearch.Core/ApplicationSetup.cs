using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace WinRepoSearch.Core
{
    public static class ApplicationSetup
    {
        public static async Task<(int exitCode, string stdout, string stderror)> InitPostgresqlAsync(string dataDirectory = "./data")
        {
            var wrapper = new GPS.Postgresql.pg_ctl.PgCtlWrapper(null);

            CancellationTokenSource source = new();

            try
            {
                var status = await wrapper.StatusAsync(dataDirectory, false, source.Token);

                bool start = false;
                bool abort = true;

                switch (status.exitCode)
                {
                    case 1:
                    case 4: // data directory missing
                        Directory.CreateDirectory(dataDirectory.Trim('"'));
                        status = await wrapper.InitAsync(dataDirectory, false, source.Token);
                        status = await wrapper.StartAsync(dataDirectory, false, source.Token);
                        break;

                    case 3:
                        status = await wrapper.StartAsync(dataDirectory, false, source.Token);
                        break;

                    case 0:
                        break;
                }

                return status;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                try
                {
                    source.Cancel();
                    source.Dispose();
                }
                catch
                {

                }
            }
        }
    }
}
