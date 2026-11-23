using System.Collections.ObjectModel;
using System.Windows.Threading;
using Avalon.Models;
using Avalon.Terminal;

namespace Avalon.Timers;

/// <summary>
/// A command or script waiting to run.
/// </summary>
/// <param name="Command">The command or script.</param>
/// <param name="ExecuteAs">How to run it.</param>
/// <param name="RunAfter">When it is due (for scheduled tasks).</param>
public sealed record PendingTask(string Command, ExecuteType ExecuteAs, DateTime RunAfter);

/// <summary>
/// Commands and scripts scheduled to run after a delay, added with #task-add or terminal.AddScheduledTask.
/// </summary>
public sealed class ScheduledTasks
{
    private readonly Interpreter _interpreter;
    private readonly DispatcherTimer _timer;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduledTasks"/> class.
    /// </summary>
    public ScheduledTasks(Interpreter interpreter)
    {
        _interpreter = interpreter;
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
        _timer.Tick += Timer_OnTick;
    }

    /// <summary>
    /// Gets the tasks that have not run yet, soonest first.
    /// </summary>
    public ObservableCollection<PendingTask> Tasks { get; } = [];

    /// <summary>
    /// Schedules a command or script to run after <paramref name="delay"/>.
    /// </summary>
    public void Add(string command, ExecuteType executeAs, TimeSpan delay)
    {
        var task = new PendingTask(command, executeAs, DateTime.Now + delay);
        int index = 0;
        while (index < Tasks.Count && Tasks[index].RunAfter <= task.RunAfter)
        {
            index++;
        }

        Tasks.Insert(index, task);
        _timer.Start();
    }

    /// <summary>
    /// Removes every scheduled task.
    /// </summary>
    public void Clear()
    {
        Tasks.Clear();
        _timer.Stop();
    }

    private async void Timer_OnTick(object? sender, EventArgs e)
    {
        var now = DateTime.Now;
        while (Tasks.Count > 0 && Tasks[0].RunAfter <= now)
        {
            var task = Tasks[0];
            Tasks.RemoveAt(0);
            try
            {
                await _interpreter.ExecuteAsync(task.ExecuteAs, task.Command);
            }
            catch (Exception ex) when (ex is InvalidOperationException or System.IO.IOException or OperationCanceledException)
            {
                _interpreter.EchoError($"Scheduled task: {ex.Message}");
            }
        }

        if (Tasks.Count == 0)
        {
            _timer.Stop();
        }
    }
}

/// <summary>
/// Commands and scripts queued with terminal.AddBatchTask and run one after another by StartBatch.
/// </summary>
public sealed class BatchTasks(Interpreter interpreter)
{
    private readonly Queue<PendingTask> _queue = new();
    private CancellationTokenSource? _running;

    /// <summary>
    /// Gets the number of tasks waiting in the batch.
    /// </summary>
    public int Count => _queue.Count;

    /// <summary>
    /// Adds a task to the batch.
    /// </summary>
    public void Add(string command, ExecuteType executeAs) => _queue.Enqueue(new PendingTask(command, executeAs, DateTime.MinValue));

    /// <summary>
    /// Runs the batch, waiting <paramref name="between"/> between tasks.
    /// </summary>
    public async void Start(TimeSpan between)
    {
        _running?.Cancel();
        var cancellation = _running = new CancellationTokenSource();
        try
        {
            while (_queue.TryDequeue(out var task))
            {
                await interpreter.ExecuteAsync(task.ExecuteAs, task.Command, cancellation.Token);
                if (_queue.Count > 0)
                {
                    await Task.Delay(between, cancellation.Token);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Cleared or restarted.
        }
    }

    /// <summary>
    /// Stops the batch and removes the remaining tasks.
    /// </summary>
    public void Clear()
    {
        _running?.Cancel();
        _queue.Clear();
    }
}

/// <summary>
/// Runs the game's On Tick Event every <see cref="Game.TickDurationInSeconds"/> while connected.
/// </summary>
public sealed class TickTimer
{
    private readonly Interpreter _interpreter;
    private readonly DispatcherTimer _timer = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="TickTimer"/> class.
    /// </summary>
    public TickTimer(Interpreter interpreter)
    {
        _interpreter = interpreter;
        _timer.Tick += async (_, _) =>
        {
            Ticks++;
            try
            {
                await _interpreter.RunScriptTextAsync(_interpreter.Game.OnTickEvent);
            }
            catch (OperationCanceledException)
            {
            }
        };
    }

    /// <summary>
    /// Gets the number of ticks since the timer started.
    /// </summary>
    public int Ticks { get; private set; }

    /// <summary>
    /// Starts the timer if the game has a tick duration and an On Tick Event.
    /// </summary>
    public void Start()
    {
        var game = _interpreter.Game;
        if (game.TickDurationInSeconds <= 0 || string.IsNullOrWhiteSpace(game.OnTickEvent))
        {
            return;
        }

        Ticks = 0;
        _timer.Interval = TimeSpan.FromSeconds(game.TickDurationInSeconds);
        _timer.Start();
    }

    /// <summary>
    /// Stops the timer.
    /// </summary>
    public void Stop() => _timer.Stop();
}
