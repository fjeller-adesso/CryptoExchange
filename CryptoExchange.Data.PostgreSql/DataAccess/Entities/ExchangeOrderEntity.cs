namespace CryptoExchange.Data.PostgreSql.DataAccess.Entities;

public partial class ExchangeOrderEntity
{
    public Guid Id { get; set; }

    public Guid ExchangeId { get; set; }

    public string Type { get; set; } = null!;

    public string Kind { get; set; } = null!;

    public DateTime Time { get; set; }

    public decimal Amount { get; set; }

    public decimal Price { get; set; }

    public virtual ExchangeEntity Exchange { get; set; } = null!;
}
