namespace FinanceBot.Exceptions;

public class InvalidArgumentsException : ParserException
{
    public InvalidArgumentsException(string message) : base(message) { }
}