namespace Arcatos.Utils
{
    public class WeastException : Exception
    {
        public WeastException(string message)
            : base(message)
        {
        }

        public WeastException(string message, Exception inner)
            : base (message, inner)
        {
        }
    }
}
