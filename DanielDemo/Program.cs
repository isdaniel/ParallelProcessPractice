using System;
using ParallelProcessPractice.Core;
namespace DanielDemo
{
    class Program
    {
        static void Main(string[] args)
        {

            TaskRunnerBase run = new DanielTaskRunner();
            run.ExecuteTasks(1000);
        }
    }
}
