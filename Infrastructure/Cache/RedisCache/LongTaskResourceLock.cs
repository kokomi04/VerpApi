using RedLockNet;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Verp.Cache.RedisCache
{
    public static class LongTaskResourceLockFactory
    {
        private static LongTaskResourceLock lockObj;
        public static async Task<LongTaskResourceLock> Accquire(string processTaskName, LongTaskCreationInfo creationInfo, int? totalSteps = null)
        {
            try
            {
                var lockAccquire = await DistributedLockFactory.GetLockAsync("LONG_TASK_RESOURCE_LOCK");

                if (lockObj != null)
                {
                    lockObj.Dispose();
                }

                lockObj = new LongTaskResourceLock(lockAccquire, processTaskName, creationInfo, totalSteps);
                isWatchStatus = false;
                return lockObj;
            }
            catch (DistributedLockExeption)
            {
                var data = GetCurrentProcess();
                throw new LongTaskResourceLockException(data);
            }

        }

        private static LongTaskResourceInfo info;
        private static LongTaskResourceLock lockInfo;
        private static bool isWatchStatus = false;
        public static void UnWatchStatus()
        {
            isWatchStatus = false;
        }
        public static void WatchStatus()
        {
            isWatchStatus = true;
        }
        public static ILongTaskResourceInfo GetCurrentProcess()
        {
            try
            {

                if (lockObj == null || lockObj.IsDisposed)
                {
                    if (!isWatchStatus) return null;

                    if (info != null)
                    {
                        info.IsFinished = true;
                    }
                    return info;
                }

                if (lockInfo != lockObj)
                {
                    lockInfo = lockObj;
                    info = new LongTaskResourceInfo();
                }

                info.ProcessingTaskName = lockObj.ProcessingTaskName;
                info.CreationInfo = lockObj.CreationInfo;
                info.IsFinished = lockObj.IsFinished;
                info.Response = lockObj.Response;
                info.TotalRows = lockObj.TotalRows;
                info.ProcessedRows = lockObj.ProcessedRows;
                info.ProccessingPercent = lockObj.ProccessingPercent;
                info.TotalSteps = lockObj.TotalSteps;
                info.CurrentStep = lockObj.CurrentStep;
                info.CurrentStepName = lockObj.CurrentStepName;

                return info;
            }
            catch (ObjectDisposedException)
            {
                return null;
            }
        }

        public static void SetResponse(string traceIdentifier, int httpStatusCode, string responseBody)
        {
            if (info != null && info.CreationInfo?.TraceIdentifier == traceIdentifier)
            {
                info.Response = new LongTaskResponse(httpStatusCode, responseBody);
            }
        }
    }

    public class LongTaskCreationInfo
    {

        public LongTaskCreationInfo(string traceIdentifier, int userId, string userName, string userFullName, long createdDatetimeUtc)
        {
            TraceIdentifier = traceIdentifier;
            UserId = userId;
            UserName = userName;
            UserFullName = userFullName;
            CreatedDatetimeUtc = createdDatetimeUtc;
        }
        public string TraceIdentifier { get; }
        public int UserId { get; }
        public string UserName { get; }
        public string UserFullName { get; }
        public long CreatedDatetimeUtc { get; }
    }

    public class LongTaskResponse
    {
        public LongTaskResponse(int httpStatusCode, string responseBody)
        {
            HttpStatusCode = httpStatusCode;
            ResponseBody = responseBody;
        }

        public int HttpStatusCode { get; }
        public string ResponseBody { get; }

    }

    public interface ILongTaskResourceInfo
    {
        string ProcessingTaskName { get; }
        bool IsFinished { get; }
        LongTaskResponse Response { get; }

        LongTaskCreationInfo CreationInfo { get; }

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
        public bool IsFinished { get; internal set; }
        public LongTaskResponse Response { get; internal set; }
        public LongTaskCreationInfo CreationInfo { get; internal set; }
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
        public bool IsFinished { get; private set; }
        public LongTaskResponse Response { get; private set; }
        public LongTaskCreationInfo CreationInfo { get; private set; }
        public int? TotalRows { get; private set; }
        public int? ProcessedRows { get; private set; }

        public int? ProccessingPercent { get { return TotalRows <= 0 ? 0 : ProcessedRows * 100 / TotalRows; } }

        public int? TotalSteps { get; private set; }
        public int? CurrentStep { get; private set; }
        public string CurrentStepName { get; private set; }


        private readonly IRedLock distributedLock;

        public bool IsDisposed { get; private set; }
        internal LongTaskResourceLock(IRedLock distributedLock, string processTaskName, LongTaskCreationInfo creationInfo, int? totalSteps)
        {
            this.distributedLock = distributedLock;
            this.ProcessingTaskName = processTaskName;
            this.CreationInfo = creationInfo;
            this.TotalSteps = totalSteps;
            IsDisposed = false;
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
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            IsDisposed = true;
            IsFinished = true;
            distributedLock.Dispose();
        }
    }

}
