using System;
using System.Linq.Expressions;

namespace Pdf.Storage.Hangfire
{
    public interface IHangfireQueue
    {
        string Enqueue<T>(Expression<Action<T>> methodCall);
        string EnqueueWithHighPriority<T>(Expression<Action<T>> methodCall);
        string Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay);
        bool RemoveJob(string id);
        void ScheduleRecurring<T>(string recurringJobId, Expression<Action<T>> methodCall, string cronExpression);
    }
}