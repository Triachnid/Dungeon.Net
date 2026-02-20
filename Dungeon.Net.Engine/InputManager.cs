using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Dungeon.Net.Engine;

public delegate Task AsyncKeyAction(ConsoleKeyInfo consoleKeyInfo);
public delegate void KeyAction(ConsoleKeyInfo consoleKeyInfo);

public class InputManager : IDisposable
{
    public readonly CancellationTokenSource InputCanceller = new CancellationTokenSource();
    private readonly ConcurrentDictionary<ConsoleKey, AsyncKeyAction> _actions = new();

    public InputManager RegisterAction(KeyAction action, params IEnumerable<ConsoleKey> keys)
        => RegisterAction(keyInfo =>
        {
            action(keyInfo);
            return Task.CompletedTask;
        }, keys);

    public InputManager RegisterAction(AsyncKeyAction action, params IEnumerable<ConsoleKey> keys)
    {
        foreach(var key in keys)
        {
            if(!_actions.TryAdd(key, action))
            {
                _actions[key] += action;
            }
        }
        return this;
    }

    public Task StartListenter()
        => Task.Run(() =>
        {
            while (!InputCanceller.IsCancellationRequested)
            {
                var keyInfo = Console.ReadKey(true);
                if (!_actions.ContainsKey(keyInfo.Key))
                {
                    continue;
                }
                var actions = _actions[keyInfo.Key];
                if(actions != default)
                {
                    foreach(var action in actions.GetInvocationList().Cast<AsyncKeyAction>())
                    {
                        action(keyInfo);
                    }
                }
            }
        }, InputCanceller.Token);

    public static void ListenSingleInput(KeyAction action, params IEnumerable<ConsoleKey> keys)
    {
        var keyInfo = Console.ReadKey(true);
        if (keys.Contains(keyInfo.Key))
        {
            action(keyInfo);
        }
    }

    public void Dispose()
    {
        if (!InputCanceller.IsCancellationRequested)
        {
            InputCanceller.Cancel();
        }
    }
}