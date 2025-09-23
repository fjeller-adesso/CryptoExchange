namespace CryptoExchange.Data.PostgreSql.DataAccess.Entities;

public partial class ExchangeEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal AvailableCrypto { get; set; }

    public decimal AvailableEuro { get; set; }

    public virtual ICollection<ExchangeOrderEntity> ExchangeOrders { get; set; } = new List<ExchangeOrderEntity>();
}
