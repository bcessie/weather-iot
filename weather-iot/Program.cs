using System;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace weather_iot
{
    class Program
    {
        public static Scheduler scheduler = new Scheduler();
        static void Main(string[] args)
        {
            RunProgram();

            Console.ReadLine();

            scheduler.Dispose();
        }

        private static async void RunProgram()
        {
            await scheduler.Start();
        }
    }
}
