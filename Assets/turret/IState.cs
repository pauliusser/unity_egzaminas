public interface IState<T> where T : class
{
    IState<T> DoState(T machine);
}