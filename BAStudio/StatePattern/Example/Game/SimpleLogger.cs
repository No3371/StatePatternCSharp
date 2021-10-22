namespace BAStudio.StatePattern.Example.Game
{
    public class SimpleLogger : ILogger
    {
        public void Log (string content) => System.Console.WriteLine(content);
        public void Log(string format, object arg0)
        {
            System.Console.WriteLine(string.Format(format, arg0));
        }

        public void Log(string format, object arg0, object arg1)
        {
            System.Console.WriteLine(string.Format(format, arg0, arg1));
        }

        public void Log(string format, object arg0, object arg1, object arg2)
        {
            System.Console.WriteLine(string.Format(format, arg0, arg1, arg2));
        }

        public void Log(string format, object arg0, object arg1, object arg2, object arg3)
        {
            System.Console.WriteLine(string.Format(format, arg0, arg1, arg2, arg3));
        }

        public void Log(string format, object arg0, object arg1, object arg2, object arg3, object arg4)
        {
            System.Console.WriteLine(string.Format(format, arg0, arg1, arg2, arg3, arg4));
        }
    }
}