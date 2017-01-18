using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ConsoleApplication22
{


    class Program
    {
        static Thread MainThread;

        static readonly Task<Boolean> CompletedTask = Task.FromResult(true);

        static void Log(string s)
        {
            if (Thread.CurrentThread.Equals(MainThread))
            {
                Console.WriteLine("M: " + s);
            }
            else
            {
                Console.WriteLine("B: " + s);
            }
        }

        static void Rest(int i)
        {
            Log("Started sleeping for " + i);
            Thread.Sleep(i);
            Log("Done sleeping for " + i);
        }

        static Task Task_That_Returns_Immediately_Without_Spawning_Any_Work()
        {
            Log("entering Task_That_Returns_Immediately_Without_Spawning_Any_Work");
            Rest(100);
            Log("exiting Task_That_Returns_Immediately_Without_Spawning_Any_Work");
            return CompletedTask;
        }

        static async Task Async_Task_That_Returns_Immediately_Without_Spawning_Any_Work()
        {
            Log("entering Async_Task_That_Returns_Immediately_Without_Spawning_Any_Work");
            Rest(100);
            Log("exiting Async_Task_That_Returns_Immediately_Without_Spawning_Any_Work");
        }

        static Task Task_That_Runs_On_Background_Thread()
        {
            return Task.Run(() =>
            {
                Log("Entering Task_That_Runs_On_Background_Thread");
                Rest(100);
                Log("Exiting Task_That_Runs_On_Background_Thread");
            });
        }

        static async Task Async_Task_That_Awaits_On_Background_Thread_Task()
        {
            Log("entering Async_Task_That_Awaits_On_Background_Thread_Task");
            await Task_That_Runs_On_Background_Thread();
            Log("exiting Async_Task_That_Awaits_On_Background_Thread_Task");
        }

        static void Main(string[] args)
        {
            MainThread = Thread.CurrentThread;

            Log("Calling Task_That_Returns_Immediately_Without_Spawning_Any_Work without waiting.  This will run sync on the foreground thread because it doesn't ever yeild the thread.");
            Task_That_Returns_Immediately_Without_Spawning_Any_Work();
            Rest(400);
            Log("Done with Task_That_Returns_Immediately_Without_Spawning_Any_Work");
            Console.ReadLine();

            Log("Calling Task_That_Returns_Immediately_Without_Spawning_Any_Work with waiting.  This will run sync on the foreground thread because it doesn't ever yield the thread and returns a completed Task object.");
            Task_That_Returns_Immediately_Without_Spawning_Any_Work().Wait();
            Rest(400);
            Log("Done with Task_That_Returns_Immediately_Without_Spawning_Any_Work");
            Console.ReadLine();

            Log("Calling Async_Task_That_Returns_Immediately_Without_Spawning_Any_Work without waiting.  This will run sync on the foreground thread because it doesn't ever yield the thread.");
            Async_Task_That_Returns_Immediately_Without_Spawning_Any_Work();
            Rest(400);
            Log("Done with Async_Task_That_Returns_Immediately_Without_Spawning_Any_Work");
            Console.ReadLine();

            Log("Calling Async_Task_That_Returns_Immediately_Without_Spawning_Any_Work with waiting.  This will run sync on the foreground thread because it doesn't ever yield the thread and returns a completed Task object.");
            Async_Task_That_Returns_Immediately_Without_Spawning_Any_Work().Wait();
            Rest(400);
            Log("Done with Async_Task_That_Returns_Immediately_Without_Spawning_Any_Work");
            Console.ReadLine();

            Log("Calling Task_That_Runs_On_Background_Thread without waiting.  This will run on a background thread because it returns a true Task.");
            Task_That_Runs_On_Background_Thread();
            Rest(400);
            Log("Done with Task_That_Runs_On_Background_Thread");
            Console.ReadLine();

            Log("Calling Task_That_Runs_On_Background_Thread with waiting.  This will run on a background thread. But the wait will cause it to block the foreground thread");
            Task_That_Runs_On_Background_Thread().Wait();
            Rest(400);
            Log("Done with Task_That_Runs_On_Background_Thread");
            Console.ReadLine();

            Log("Calling Async_Task_That_Awaits_On_Background_Thread_Task without waiting.  This will run on a background thread because it returns a true Task.");
            Async_Task_That_Awaits_On_Background_Thread_Task();
            Rest(400);
            Log("Done with Async_Task_That_Awaits_On_Background_Thread_Task");
            Console.ReadLine();

            Log("Calling Async_Task_That_Awaits_On_Background_Thread_Task with waiting.  This will run on a background thread. But the wait will cause it to block the foreground thread");
            Async_Task_That_Awaits_On_Background_Thread_Task().Wait();
            Rest(400);
            Log("Done with Async_Task_That_Awaits_On_Background_Thread_Task");

            while (true)
            {
                Console.ReadLine();
            }

        }
    }
}
