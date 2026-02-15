using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class InputManager : IDisposable
{
    private readonly Dictionary<ConsoleKey, Action> _listeners = new();
    private Task? _task;
    private CancellationTokenSource _cts = new();

    public InputManager RegisterKey(ConsoleKey key, Action action)
    {
        if(!_listeners.TryAdd(key, action))
        {
            _listeners[key] += action;
        }
        return this;
    }


    public void ListenForNextInput()
    {
        var key = Console.ReadKey(true).Key;
        if (_listeners.ContainsKey(key))
        {
            _listeners[key]();
        }
    }

    public Task StartListeningForInput()
    {
        _task = Task.Run(()=>
        {
            while (!_cts.IsCancellationRequested)
            {
                var key = Console.ReadKey(true).Key;
                if (_listeners.ContainsKey(key))
                {
                    _listeners[key]();
                }
            }
        }, _cts.Token);
        return _task;
    }

    public void Dispose() 
        => _cts.Cancel();
}