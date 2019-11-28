using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Threading.Tasks;

namespace weather_iot
{
    public class Scheduler : IDisposable
    {
        private IScheduler _scheduler;

        private bool _disposed = false;

        public async Task Start()
        {
            try
            {
                // Create the scheduler with the followign config
                NameValueCollection props = new NameValueCollection
                {
                    { "quartz.jobStore.type", "Quartz.Simpl.RAMJobStore, Quartz" },
                    { "quartz.threadPool.threadCount", "1" },
                    { "quartz.scheduler.instanceName", "WeatherScheduler" }
                };

                StdSchedulerFactory factory = new StdSchedulerFactory(props);
                _scheduler = await factory.GetScheduler();

                // start the scheduler - the scheduler won't run without this
                await _scheduler.Start();

                // define the job and tie it to our HelloJob class
                IJobDetail job = JobBuilder.Create<WeatherSchedulerJob>()
                    .WithIdentity("WeatherJob", "WeatherGroup")
                    .Build();

                // Trigger the job to run now, and then every 40 seconds
                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity("WeatherTrigger", "WeatherGroup")
                    .StartNow()
                    //.WithCronSchedule("0 0 5 1/1 * ? *") // @ 5 every morning
                    .WithCronSchedule("0 0/1 * 1/1 * ? *") // @ every 1 min
                
                    .Build();

                await _scheduler.ScheduleJob(job, trigger);
            }
            catch (SchedulerException se)
            {
                await Console.Error.WriteLineAsync(se.ToString());
            }
        }

        public async Task Stop()
        {
            if (_scheduler != null)
            {
                // and last shut down the scheduler when you are ready to close your program
                await _scheduler.Shutdown();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual async void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                await Stop();
            }

            // Free any other unmanaged objects here
            if (_scheduler != null)
            {
                _scheduler = null;
            }

            _disposed = true;
        }
    }
}
