namespace BAStudio.StatePattern.Example.Game
{
    public interface ILogger
    {
        void Log (string content);
        void Log (string format, object arg0);
        void Log (string format, object arg0, object arg1);
        void Log (string format, object arg0, object arg1, object arg2);
        void Log (string format, object arg0, object arg1, object arg, object arg3);
        void Log (string format, object arg0, object arg1, object arg, object arg3, object arg4);
    }
}