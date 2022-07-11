namespace ModLoader.Events
{
    public class ConsoleInputReceivedEventArgs
    {
        public string Input { get; private set; }

        public ConsoleInputReceivedEventArgs(string input)
        {
            Input = input;
        }
    }
}
