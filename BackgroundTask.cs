using System;
using System.Threading;
using System.Threading.Tasks;

namespace Eigengrau.BackgroundTask;

/// <summary>
/// Used to start asynchronous background tasks
/// </summary>
public class BackgroundTask
{
    private readonly TimeSpan _interval;
    private readonly Action? _action;
    private readonly Func<Task>? _task;
    private readonly bool _count;
    private int _counts;
    private Task? _timerTask;
    private readonly CancellationTokenSource _cts = new();

    /// <summary>
    /// Used to start asynchronous background tasks
    /// </summary>
    /// <param name="action">Action ran after each tick</param>
    /// <param name="interval">Interval between ticks</param>
    /// <param name="counts">Number of times to run the action. Default (0) is unlimited.</param>
    public BackgroundTask(Action action, TimeSpan interval, int counts = 0)
    {
        _interval = interval;
        _action = action;
        _count = counts > 0;
        _counts = counts;
    }

    /// <summary>
    /// Used to start asynchronous background tasks
    /// </summary>
    /// <param name="action">Action ran after each tick</param>
    /// <param name="milliseconds">Milliseconds between ticks</param>
    /// <param name="counts">Number of times to run the action. Default (0) is unlimited.</param>
    public BackgroundTask(Action action, int milliseconds, int counts = 0)
    {
        _interval = TimeSpan.FromMilliseconds(milliseconds);
        _action = action;
        _count = counts > 0;
        _counts = counts;
    }
    
    /// <summary>
    /// Used to start asynchronous background tasks.
    /// Note: if task takes longer than interval, tick time will be the duration of the task
    /// </summary>
    /// <param name="task">Task ran after each tick</param>
    /// <param name="interval">Interval between ticks</param>
    /// <param name="counts">Number of times to run the action. Default (0) is unlimited.</param>
    public BackgroundTask(Func<Task> task, TimeSpan interval, int counts = 0)
    {
        _interval = interval;
        _task = task;
        _count = counts > 0;
        _counts = counts;
    }

    /// <summary>
    /// Used to start asynchronous background tasks
    /// Note: if task takes longer than interval, tick time will be the duration of the task
    /// </summary>
    /// <param name="task">Task ran after each tick</param>
    /// <param name="milliseconds">Milliseconds between ticks</param>
    /// <param name="counts">Number of times to run the action. Default (0) is unlimited.</param>
    public BackgroundTask(Func<Task> task, int milliseconds, int counts = 0)
    {
        _interval = TimeSpan.FromMilliseconds(milliseconds);
        _task = task;
        _count = counts > 0;
        _counts = counts;
    }

    /// <summary>
    /// Start the background task
    /// </summary>
    public void Start()
    {
        async Task DoStart()
        {
            try
            {
                var timer = new PeriodicTimer(_interval);
                
                while (await timer.WaitForNextTickAsync(_cts.Token))
                {
                    if (_count)
                    {
                        _counts--;
                        if (_counts == 0)
                            _cts.Cancel();
                    }
                    
                    if (_task is not null)
                        await _task();
                    
                    _action?.Invoke();
                }
            }
            catch (TaskCanceledException _) {}
            catch (OperationCanceledException _){}
        }

        _timerTask = DoStart();
    }

    /// <summary>
    /// Stop the background task
    /// </summary>
    public async Task StopAsync()
    {
        _cts.Cancel();
        if (_timerTask is not null)
        {
            await _timerTask;
            _timerTask.Dispose();
        }
        _cts.Dispose();
    }
}

