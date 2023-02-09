using ServerCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Server
{
    struct JobTimerElem : IComparable<JobTimerElem>
    {
        // 실행 타이밍
        public int execTick;
        public Action action;

        public int CompareTo(JobTimerElem other)
        {
            // 작은 것이 우선 순위
            return other.execTick - this.execTick; 
        }
    }

    // 작업의 중앙관리 시스템
    internal class JobTimer
    {
        PriorityQueue<JobTimerElem> _pq = new PriorityQueue<JobTimerElem>();
        object _lock = new object();

        public static JobTimer Instance { get; } = new JobTimer();

        public void Push(Action action, int tickAfter = 0)
        {
            JobTimerElem job;
            job.action = action;
            // 현재 시간 + 해당 작업의 tickAfter
            job.execTick = Environment.TickCount + tickAfter;

            lock (_lock)
            {
                _pq.Push(job);
            }
        }

        public void Flush()
        {
            while (true)
            {
                int now = Environment.TickCount;
                JobTimerElem job;

                lock (_lock)
                {
                    job = _pq.Peek();
                    
                    // 가장 이른 job이 아직 실행 타이밍이 아님
                    if (job.execTick > now)
                        break; // while

                    // 실행 타이밍이 됨
                    _pq.Pop();
                }

                job.action.Invoke();
            }
        }
    }
}
