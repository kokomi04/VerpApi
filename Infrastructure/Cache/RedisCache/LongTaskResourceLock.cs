using RedLockNet;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Verp.Cache.RedisCache
{
    public static class LongTaskResourceLockFactory
    {
        private static LongTaskResourceLock lockObj;
        public static async Task<LongTaskResourceLock> Accquire(string processTaskName, int userId, int? totalSteps = null)
        {
            try
            {
                var lockAccquire = await DistributedLockFactory.GetLockAsync("LONG_TASK_RESOURCE_LOCK");

                if (lockObj != null)
                {
                    lockObj.Dispose();
                }

                lockObj = new LongTaskResourceLock(lockAccquire, processTaskName, userId, totalSteps);
                return lockObj;
            }
            catch (DistributedLockExeption)
            {
                var data = GetCurrentProcess();
                throw new LongTaskResourceLockException(data);
            }

        }
        public static ILongTaskResourceInfo GetCurrentProcess()
        {
            try
            {
                return new LongTaskResourceInfo()
                {
                    ProcessingTaskName = lockObj.ProcessingTaskName,
                    UserId = lockObj.UserId,
                    CreatedDatetimeUtc = lockObj.CreatedDatetimeUtc,
                    TotalRows = lockObj.TotalRows,
                    ProcessedRows = lockObj.ProcessedRows,
                    ProccessingPercent = lockObj.ProccessingPercent,
                    TotalSteps = lockObj.TotalSteps,
                    CurrentStep = lockObj.CurrentStep,
                    CurrentStepName = lockObj.CurrentStepName,
                };
            }
            catch (ObjectDisposedException)
            {
                return null;
            }
        }
    }

    public interface ILongTaskResourceInfo
    {
        string ProcessingTaskName { get; }
        int UserId { get; }
        DateTime CreatedDatetimeUtc { get; }
        int? TotalRows { get; }
        int? ProcessedRows { get; }
        int? ProccessingPercent { get; }
        int? TotalSteps { get; }
        int? CurrentStep { get; }
        string CurrentStepName { get; }
    }

    internal class LongTaskResourceInfo : ILongTaskResourceInfo
    {
        public string ProcessingTaskName { get; internal set; }
        public int UserId { get; internal set; }
        public DateTime CreatedDatetimeUtc { get; internal set; }
        public int? TotalRows { get; internal set; }
        public int? ProcessedRows { get; internal set; }
        public int? ProccessingPercent { get; internal set; }

        public int? TotalSteps { get; internal set; }
        public int? CurrentStep { get; internal set; }
        public string CurrentStepName { get; internal set; }
    }

    public class LongTaskResourceLock : IDisposable, ILongTaskResourceInfo
    {
        public string ProcessingTaskName { get; private set; }
        public int UserId { get; private set; }
        public DateTime CreatedDatetimeUtc { get; private set; }
        public int? TotalRows { get; private set; }
        public int? ProcessedRows { get; private set; }

        public int? ProccessingPercent { get { return TotalRows <= 0 ? 0 : ProcessedRows * 100 / TotalRows; } }

        public int? TotalSteps { get; private set; }
        public int? CurrentStep { get; private set; }
        public string CurrentStepName { get; private set; }


        private IRedLock distributedLock;
        internal LongTaskResourceLock(IRedLock distributedLock, string processTaskName, int userId, int? totalSteps)
        {
            this.distributedLock = distributedLock;
            this.ProcessingTaskName = processTaskName;
            this.UserId = userId;
            this.CreatedDatetimeUtc = DateTime.UtcNow;
            this.TotalSteps = totalSteps;
        }

        public void SetCurrentStep(string currentStepName, int? totalRows = null)
        {
            this.TotalRows = totalRows;
            this.ProcessedRows = null;

            if (this.CurrentStep == null)
            {
                CurrentStep = 1;
            }
            else
            {
                this.CurrentStep++;
            }

            this.CurrentStepName = currentStepName;
        }

        public void SetTotalRows(int totalRows)
        {
            this.TotalRows = totalRows;
            this.ProcessedRows = 0;
        }

        public void SetProcessedRows(int sumProcessedRows)
        {
            this.ProcessedRows = sumProcessedRows;
        }

        public void IncProcessedRows()
        {
            if (this.ProcessedRows == null) ProcessedRows = 0;
            ProcessedRows++;
        }

        public void Dispose()
        {
            distributedLock.Dispose();
        }


    }

}
