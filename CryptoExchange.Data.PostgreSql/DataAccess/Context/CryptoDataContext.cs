using CryptoExchange.Data.PostgreSql.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace CryptoExchange.Data.PostgreSql.DataAccess.Context;

public partial class CryptoDataContext : DbContext
{
	private const string _SCHEMANAME_MIGRATIONSHISTORY = "order_books";

	public CryptoDataContext()
	{
	}

	public CryptoDataContext( DbContextOptions<CryptoDataContext> options )
		: base( options )
	{
	}

	public virtual DbSet<ExchangeEntity> ExchangeEntities { get; set; }

	public virtual DbSet<ExchangeOrderEntity> ExchangeOrderEntities { get; set; }

	protected override void OnConfiguring( DbContextOptionsBuilder optionsBuilder )
	{
		base.OnConfiguring( optionsBuilder );

		optionsBuilder.UseNpgsql( o => o.MigrationsHistoryTable( "__EFMigrationsHistory", _SCHEMANAME_MIGRATIONSHISTORY ) );
	}

	protected override void OnModelCreating( ModelBuilder modelBuilder )
	{
		modelBuilder.HasDefaultSchema( "order_books" );

		modelBuilder.Entity<ExchangeEntity>( entity =>
		{
			entity.HasKey( e => e.Id ).HasName( "exchange_pkey" );

			entity.ToTable( "exchange", "order_books" );

			entity.Property( e => e.Id )
				.ValueGeneratedNever()
				.HasColumnName( "id" );
			entity.Property( e => e.AvailableCrypto ).HasColumnName( "available_crypto" );
			entity.Property( e => e.AvailableEuro ).HasColumnName( "available_euro" );
			entity.Property( e => e.Name )
				.HasColumnType( "character varying(50)" )
				.HasMaxLength( 50 )
				.HasColumnName( "name" );
		} );

		modelBuilder.Entity<ExchangeOrderEntity>( entity =>
		{
			entity.HasKey( e => e.Id ).HasName( "exchange_order_pkey" );

			entity.ToTable( "exchange_order", "order_books" );

			entity.HasIndex( e => e.ExchangeId, "fki_orders_exchanges" );

			entity.Property( e => e.Id )
				.ValueGeneratedNever()
				.HasColumnName( "id" );
			entity.Property( e => e.Amount ).HasColumnName( "amount" );
			entity.Property( e => e.ExchangeId ).HasColumnName( "exchange_id" );
			entity.Property( e => e.Kind )
				.HasColumnType( "character varying(20)" )
				.HasMaxLength( 20 )
				.HasColumnName( "kind" );
			entity.Property( "Time" )
				.HasColumnType( "timestamp with time zone" )
				.HasColumnName( "time" );
			entity.Property( e => e.Price ).HasColumnName( "price" );
			entity.Property( e => e.Type )
				.HasColumnType( "character varying(20)" )
				.HasMaxLength( 20 )
				.HasColumnName( "type" );

			entity.HasOne( d => d.Exchange ).WithMany( p => p.ExchangeOrders )
				.HasForeignKey( d => d.ExchangeId )
				.OnDelete( DeleteBehavior.ClientSetNull )
				.HasConstraintName( "orders_exchanges" );
		} );

		OnModelCreatingPartial( modelBuilder );
	}

	partial void OnModelCreatingPartial( ModelBuilder modelBuilder );
}
