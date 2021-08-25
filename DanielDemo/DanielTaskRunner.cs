using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ParallelProcessPractice.Core;

namespace DanielDemo
{
    public class TaskPool{
        private readonly int _step;
        private readonly int _limitThreadCount;
        private ConcurrentQueue<MyTask> _taskQueue;
        public List<Thread> _threadPool;
        private ManualResetEvent _queueSignal;
        private volatile bool _finished;

        public TaskPool(int step,int limitThreadCount)
        {
            
            _taskQueue = new ConcurrentQueue<MyTask>(Enumerable.Range(1,1024).Select(x=> default(MyTask)));
            MyTask t;
            while (_taskQueue.TryDequeue(out t))
            {
                
            }
            _threadPool = new List<Thread>();
            _queueSignal = new ManualResetEvent(false);
            this._step = step;
            this._limitThreadCount = limitThreadCount;
            InitPool();
        }

        public void AddTask(MyTask task){
            _taskQueue.Enqueue(task);
            _queueSignal.Set();
            _queueSignal.Reset();
        }

        private void InitPool(){
            for (int i = 0; i < _limitThreadCount; i++)
            {
                Thread worker = new Thread(Process);
   
                worker.Start();
                _threadPool.Add(worker);
            }
        }

        private void Process()
        {
            while (true)
            {
                while (_taskQueue.Count > 0)
                {
                    MyTask task = null;
                    if(_taskQueue.TryDequeue(out task)){
                        task.DoStepN(_step);
                        if(NextPool != null) {
                            NextPool.AddTask(task);
                        }
                    }
                }
                
                if (_finished){
                    break;
                }

                _queueSignal.WaitOne();
            }
        }

        public void WaitFinished()
        {
            _finished = true;
            this._queueSignal.Set();

            do
            {
              var thread = _threadPool[0];
              thread.Join();
              _threadPool.Remove(thread);
            } while (_threadPool.Count > 0);
            
            if (NextPool != null)
            {
                NextPool.WaitFinished();
            }
        }


        public TaskPool NextPool { get; internal set; }
    }

    public class DanielTaskRunner : TaskRunnerBase
    {
        TaskPool _poolChain;

        public DanielTaskRunner()
        {
            _poolChain =  new TaskPool(1,5) {
                NextPool = new TaskPool(2,3) {
                    NextPool =  new TaskPool(3,3){ 
                        NextPool = null
                    }
                }
            };
        }

        public override void Run(IEnumerable<MyTask> tasks)
        {
            foreach (var task in tasks)
            {
                _poolChain.AddTask(task);
            }

            _poolChain.WaitFinished();
        }
    }
}
