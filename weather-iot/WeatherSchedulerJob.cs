using Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace weather_iot
{
    public class WeatherSchedulerJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            Weather weather = new Weather();
            weather.ProcessWeather();

            return Task.FromResult(0);
        }
    }
}
