using System;
using System.Windows.Threading;

namespace AppInstaller.Models;

public class TimerModel
{
    private readonly DispatcherTimer _timer;
    private DateTime _startTime;

    public int ElapsedSeconds { get; private set; }

    public TimerModel()
    {
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += TimerTick;
    }

    private void TimerTick(object? sender, EventArgs e)
    {
        ElapsedSeconds = (int)(DateTime.Now - _startTime).TotalSeconds;
    }

    public void Start()
    {
        _startTime = DateTime.Now;
        _timer.Start();
    }

    public void Stop()
    {
        _timer.Stop();
    }

    public void Reset()
    {
        _timer.Stop();
        ElapsedSeconds = 0;
    }
}