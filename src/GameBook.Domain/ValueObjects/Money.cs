namespace GameBook.Domain.ValueObjects;

public readonly record struct Money(decimal Amount, string Currency = "GEL")
{
    public static Money Gel(decimal amount) => new(amount, "GEL");

    public override string ToString() => $"{Amount} {Currency}";
}
