namespace FinanceBot.Exceptions;

public class InvalidAmountException : ParserException
{
    public InvalidAmountException(string message) : base(message) { }
}