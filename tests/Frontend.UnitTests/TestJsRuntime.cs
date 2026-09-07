using Microsoft.JSInterop;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Tasks;
using System;

public class TestJsRuntime : IJSRuntime
{
    private readonly ConcurrentDictionary<string, string> _store = new();

    public void SetItem(string key, string value)
    {
        _store[key] = value;
    }

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
    {
        if (identifier == "localStorage.getItem")
        {
            var key = args?[0]?.ToString() ?? "";
            _store.TryGetValue(key, out var v);
            object result = v;
            return new ValueTask<TValue>((TValue)result!);
        }

        return new ValueTask<TValue>(default(TValue)!);
    }

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, System.Threading.CancellationToken cancellationToken, object?[]? args)
    {
        return InvokeAsync<TValue>(identifier, args);
    }

    public ValueTask<object> InvokeAsync(string identifier, object?[]? args)
    {
        return new ValueTask<object>(InvokeAsync<object>(identifier, args).Result);
    }

    public ValueTask<object> InvokeAsync(string identifier, System.Threading.CancellationToken cancellationToken, object?[]? args)
    {
        return InvokeAsync(identifier, args);
    }

    // Provide void variants used by SetItem/RemoveItem/Clear
    public ValueTask<TValue> InvokeVoidAsync<TValue>(string identifier, object?[]? args)
    {
        if (identifier == "localStorage.setItem")
        {
            var key = args?[0]?.ToString() ?? "";
            var val = args?.Length > 1 ? args[1]?.ToString() ?? "" : "";
            _store[key] = val!;
        }

        if (identifier == "localStorage.removeItem")
        {
            var key = args?[0]?.ToString() ?? "";
            _store.TryRemove(key, out _);
        }

        if (identifier == "localStorage.clear")
        {
            _store.Clear();
        }

        return new ValueTask<TValue>(default(TValue)!);
    }

    public ValueTask<TValue> InvokeVoidAsync<TValue>(string identifier, System.Threading.CancellationToken cancellationToken, object?[]? args)
    {
        return InvokeVoidAsync<TValue>(identifier, args);
    }
}