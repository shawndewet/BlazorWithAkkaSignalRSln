namespace BlazorWithAkkaSignalR.Shared
{
    public class CounterIncremented
    {
        public CounterIncremented(string message)
        {
            Message = message;
        }

        public string Message { get; }
    }
}