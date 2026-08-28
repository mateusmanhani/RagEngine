namespace RagEngine.Application.Exceptions
{
    public class AnswerGenerationException : Exception
    {
        public AnswerGenerationException(string message)
            : base(message)
        {
        }

        public AnswerGenerationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
