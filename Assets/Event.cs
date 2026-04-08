
using System;
public class Event<T>
{
    private event Action<T> listeners;

    public void Subscribe(Action<T> listener) => listeners += listener;
    public void Unsubscribe(Action<T> listener) => listeners -= listener;
    public void Invoke(T data) => listeners?.Invoke(data);
}

public class Event
{
    private event Action listeners;

    public void Subscribe(Action listener) => listeners += listener;
    public void Unsubscribe(Action listener) => listeners -= listener;
    public void Invoke() => listeners?.Invoke();
}