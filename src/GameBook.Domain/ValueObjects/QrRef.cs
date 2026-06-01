namespace GameBook.Domain.ValueObjects;

public readonly record struct QrRef(string Value)
{
    public static QrRef Generate() => new(Guid.NewGuid().ToString("N"));
}
