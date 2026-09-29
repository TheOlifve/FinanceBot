namespace FinanceBot.Exceptions;

public abstract class ParserException: Exception
{
    public ParserException(string message): base(message) { }
}