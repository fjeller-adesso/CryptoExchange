using CryptoExchange.Contracts.DataObjects;
using CryptoExchange.Contracts.Repositories;
using CryptoExchange.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.CryptoExchange.Core.Services;

public class BitcoinOrderServiceTests
{
    private readonly Mock<ICryptoExchangeRepository> _mockRepository;
    private readonly Mock<ILogger<BitcoinOrderService>> _mockLogger;
    private readonly BitcoinOrderService _service;

    public BitcoinOrderServiceTests()
    {
        _mockRepository = new Mock<ICryptoExchangeRepository>();
        _mockLogger = new Mock<ILogger<BitcoinOrderService>>();
        _service = new BitcoinOrderService(_mockRepository.Object, _mockLogger.Object);
    }

    #region BuyAsync Tests

    [Fact]
    public async Task BuyAsync_WithValidAmount_ReturnsSuccessfulResult()
    {
        // Arrange
        decimal bitcoinToBuy = 1.5m;
        var workingOrders = new List<WorkingBuyOrder>
        {
            CreateWorkingBuyOrder(Guid.NewGuid(), "Exchange1", 2.0m, 50000m, 2.0m, 5.0m),
            CreateWorkingBuyOrder(Guid.NewGuid(), "Exchange2", 1.0m, 51000m, 1.0m, 3.0m)
        };

        _mockRepository.Setup(r => r.GetOrderedWorkingBuyOrdersAsync())
            .ReturnsAsync(workingOrders);
        _mockRepository.Setup(r => r.UpdateAvailableCryptoAsync(It.IsAny<Dictionary<Guid, decimal>>()))
            .Returns(Task.CompletedTask);
        _mockRepository.Setup(r => r.UpdateFulfilledWorkOrdersAsync(It.IsAny<IEnumerable<CoinExchangeOrder>>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.BuyAsync(bitcoinToBuy);

        // Assert
        Assert.True(result.IsSuccessful);
        Assert.Equal(1.5m, result.TotalBitcoinPurchased);
        Assert.Equal(75000m, result.TotalCost); // 1.5 * 50000
        Assert.Single(result.ExecutedOrders);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public async Task BuyAsync_WithZeroAmount_ReturnsError()
    {
        // Act
        var result = await _service.BuyAsync(0m);

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Equal("Bitcoin amount to buy must be greater than 0", result.ErrorMessage);
        Assert.Empty(result.ExecutedOrders);
        Assert.Equal(0m, result.TotalBitcoinPurchased);
        Assert.Equal(0m, result.TotalCost);
    }

    [Fact]
    public async Task BuyAsync_WithNegativeAmount_ReturnsError()
    {
        // Act
        var result = await _service.BuyAsync(-1m);

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Equal("Bitcoin amount to buy must be greater than 0", result.ErrorMessage);
        Assert.Empty(result.ExecutedOrders);
    }

    [Fact]
    public async Task BuyAsync_WithInsufficientOrders_ReturnsPartialSuccess()
    {
        // Arrange
        decimal bitcoinToBuy = 5.0m;
        var workingOrders = new List<WorkingBuyOrder>
        {
            CreateWorkingBuyOrder(Guid.NewGuid(), "Exchange1", 1.0m, 50000m, 1.0m, 2.0m),
            CreateWorkingBuyOrder(Guid.NewGuid(), "Exchange2", 2.0m, 51000m, 2.0m, 3.0m)
        };

        _mockRepository.Setup(r => r.GetOrderedWorkingBuyOrdersAsync())
            .ReturnsAsync(workingOrders);
        _mockRepository.Setup(r => r.UpdateAvailableCryptoAsync(It.IsAny<Dictionary<Guid, decimal>>()))
            .Returns(Task.CompletedTask);
        _mockRepository.Setup(r => r.UpdateFulfilledWorkOrdersAsync(It.IsAny<IEnumerable<CoinExchangeOrder>>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.BuyAsync(bitcoinToBuy);

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Equal(3.0m, result.TotalBitcoinPurchased);
        Assert.Contains("3", result.ErrorMessage);
        Assert.Contains("Bitcoin could be purchased", result.ErrorMessage);
        Assert.Contains("Remaining needed: 2", result.ErrorMessage);
        Assert.Equal(2, result.ExecutedOrders.Count);
    }

    [Fact]
    public async Task BuyAsync_WithLimitedExchangeCrypto_RespectsExchangeBalance()
    {
        // Arrange
        decimal bitcoinToBuy = 2.0m;
        var workingOrders = new List<WorkingBuyOrder>
        {
            // Order has 5.0 available but exchange only has 1.5 crypto
            CreateWorkingBuyOrder(Guid.NewGuid(), "Exchange1", 5.0m, 50000m, 5.0m, 1.5m)
        };

        _mockRepository.Setup(r => r.GetOrderedWorkingBuyOrdersAsync())
            .ReturnsAsync(workingOrders);
        _mockRepository.Setup(r => r.UpdateAvailableCryptoAsync(It.IsAny<Dictionary<Guid, decimal>>()))
            .Returns(Task.CompletedTask);
        _mockRepository.Setup(r => r.UpdateFulfilledWorkOrdersAsync(It.IsAny<IEnumerable<CoinExchangeOrder>>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.BuyAsync(bitcoinToBuy);

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Equal(1.5m, result.TotalBitcoinPurchased);
        Assert.Equal(75000m, result.TotalCost); // 1.5 * 50000
    }

    [Fact]
    public async Task BuyAsync_WithNoOrders_ReturnsError()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetOrderedWorkingBuyOrdersAsync())
            .ReturnsAsync(new List<WorkingBuyOrder>());

        // Act
        var result = await _service.BuyAsync(1.0m);

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Equal(0m, result.TotalBitcoinPurchased);
        Assert.Contains("0", result.ErrorMessage);
        Assert.Contains("Bitcoin could be purchased", result.ErrorMessage);
        Assert.Empty(result.ExecutedOrders);
    }

    [Fact]
    public async Task BuyAsync_WithMultipleExecutionsFromSameOrder_UpdatesCorrectly()
    {
        // Arrange
        decimal bitcoinToBuy = 3.0m;
        var exchangeId = Guid.NewGuid();
        var workingOrders = new List<WorkingBuyOrder>
        {
            CreateWorkingBuyOrder(exchangeId, "Exchange1", 5.0m, 50000m, 5.0m, 10.0m)
        };

        _mockRepository.Setup(r => r.GetOrderedWorkingBuyOrdersAsync())
            .ReturnsAsync(workingOrders);
        _mockRepository.Setup(r => r.UpdateAvailableCryptoAsync(It.IsAny<Dictionary<Guid, decimal>>()))
            .Returns(Task.CompletedTask);
        _mockRepository.Setup(r => r.UpdateFulfilledWorkOrdersAsync(It.IsAny<IEnumerable<CoinExchangeOrder>>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.BuyAsync(bitcoinToBuy);

        // Assert
        Assert.True(result.IsSuccessful);
        Assert.Equal(3.0m, result.TotalBitcoinPurchased);
        Assert.Equal(150000m, result.TotalCost); // 3.0 * 50000

        // Verify repository calls
        _mockRepository.Verify(r => r.UpdateAvailableCryptoAsync(
            It.Is<Dictionary<Guid, decimal>>(d => d[exchangeId] == 3.0m)), Times.Once);
    }

    #endregion

    #region SellAsync Tests

    [Fact]
    public async Task SellAsync_WithValidAmount_ReturnsSuccessfulResult()
    {
        // Arrange
        decimal bitcoinToSell = 1.5m;
        var workingSellOrders = new List<WorkingSellOrder>
        {
            CreateWorkingSellOrder(Guid.NewGuid(), "Exchange1", 2.0m, 50000m, 2.0m, 150000m),
            CreateWorkingSellOrder(Guid.NewGuid(), "Exchange2", 1.0m, 49000m, 1.0m, 100000m)
        };

        _mockRepository.Setup(r => r.GetOrderedWorkingSellOrdersAsync())
            .ReturnsAsync(workingSellOrders);
        _mockRepository.Setup(r => r.UpdateAvailableFundsAsync(It.IsAny<Dictionary<Guid, (decimal, decimal)>>()))
            .Returns(Task.CompletedTask);
        _mockRepository.Setup(r => r.UpdateFulfilledWorkOrdersAsync(It.IsAny<IEnumerable<CoinExchangeOrder>>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.SellAsync(bitcoinToSell);

        // Assert
        Assert.True(result.IsSuccessful);
        Assert.Equal(1.5m, result.TotalBitcoinSold);
        Assert.Equal(75000m, result.TotalReceived); // 1.5 * 50000
        Assert.Single(result.ExecutedOrders);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public async Task SellAsync_WithZeroAmount_ReturnsError()
    {
        // Act
        var result = await _service.SellAsync(0m);

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Equal("Bitcoin amount to sell must be greater than 0", result.ErrorMessage);
        Assert.Empty(result.ExecutedOrders);
        Assert.Equal(0m, result.TotalBitcoinSold);
        Assert.Equal(0m, result.TotalReceived);
    }

    [Fact]
    public async Task SellAsync_WithNegativeAmount_ReturnsError()
    {
        // Act
        var result = await _service.SellAsync(-1m);

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Equal("Bitcoin amount to sell must be greater than 0", result.ErrorMessage);
        Assert.Empty(result.ExecutedOrders);
    }

    [Fact]
    public async Task SellAsync_WithInsufficientFunds_RespectsExchangeFunds()
    {
        // Arrange
        decimal bitcoinToSell = 2.0m;
        var workingSellOrders = new List<WorkingSellOrder>
        {
            // Exchange only has 75000 funds, can only buy 1.5 Bitcoin at 50000 price
            CreateWorkingSellOrder(Guid.NewGuid(), "Exchange1", 5.0m, 50000m, 5.0m, 75000m)
        };

        _mockRepository.Setup(r => r.GetOrderedWorkingSellOrdersAsync())
            .ReturnsAsync(workingSellOrders);
        _mockRepository.Setup(r => r.UpdateAvailableFundsAsync(It.IsAny<Dictionary<Guid, (decimal, decimal)>>()))
            .Returns(Task.CompletedTask);
        _mockRepository.Setup(r => r.UpdateFulfilledWorkOrdersAsync(It.IsAny<IEnumerable<CoinExchangeOrder>>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.SellAsync(bitcoinToSell);

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Equal(1.5m, result.TotalBitcoinSold);
        Assert.Equal(75000m, result.TotalReceived);
        Assert.Contains("1", result.ErrorMessage);
        Assert.Contains("Bitcoin could be sold", result.ErrorMessage);
        Assert.Contains("Remaining unsold:", result.ErrorMessage);
    }

    [Fact]
    public async Task SellAsync_WithNoOrders_ReturnsError()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetOrderedWorkingSellOrdersAsync())
            .ReturnsAsync(new List<WorkingSellOrder>());

        // Act
        var result = await _service.SellAsync(1.0m);

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Equal(0m, result.TotalBitcoinSold);
        Assert.Contains("0", result.ErrorMessage);
        Assert.Contains("Bitcoin could be sold", result.ErrorMessage);
        Assert.Empty(result.ExecutedOrders);
    }

    [Fact]
    public async Task SellAsync_WithMultipleOrdersAndExchanges_ExecutesOptimally()
    {
        // Arrange
        decimal bitcoinToSell = 3.0m;
        var exchangeId1 = Guid.NewGuid();
        var exchangeId2 = Guid.NewGuid();
        var workingSellOrders = new List<WorkingSellOrder>
        {
            CreateWorkingSellOrder(exchangeId1, "Exchange1", 2.0m, 52000m, 2.0m, 200000m), // Higher price - should execute first
            CreateWorkingSellOrder(exchangeId2, "Exchange2", 2.0m, 50000m, 2.0m, 200000m)  // Lower price - should execute second
        };

        _mockRepository.Setup(r => r.GetOrderedWorkingSellOrdersAsync())
            .ReturnsAsync(workingSellOrders);
        _mockRepository.Setup(r => r.UpdateAvailableFundsAsync(It.IsAny<Dictionary<Guid, (decimal, decimal)>>()))
            .Returns(Task.CompletedTask);
        _mockRepository.Setup(r => r.UpdateFulfilledWorkOrdersAsync(It.IsAny<IEnumerable<CoinExchangeOrder>>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.SellAsync(bitcoinToSell);

        // Assert
        Assert.True(result.IsSuccessful);
        Assert.Equal(3.0m, result.TotalBitcoinSold);
        Assert.Equal(154000m, result.TotalReceived); // 2.0 * 52000 + 1.0 * 50000
        Assert.Equal(2, result.ExecutedOrders.Count);

        // Verify higher price executed first
        Assert.Equal(52000m, result.ExecutedOrders[0].Price);
        Assert.Equal(50000m, result.ExecutedOrders[1].Price);
    }

    [Fact]
    public async Task SellAsync_UpdatesExchangeFundsCorrectly()
    {
        // Arrange
        decimal bitcoinToSell = 2.0m;
        var exchangeId = Guid.NewGuid();
        var workingSellOrders = new List<WorkingSellOrder>
        {
            CreateWorkingSellOrder(exchangeId, "Exchange1", 5.0m, 50000m, 5.0m, 300000m)
        };

        _mockRepository.Setup(r => r.GetOrderedWorkingSellOrdersAsync())
            .ReturnsAsync(workingSellOrders);
        _mockRepository.Setup(r => r.UpdateAvailableFundsAsync(It.IsAny<Dictionary<Guid, (decimal, decimal)>>()))
            .Returns(Task.CompletedTask);
        _mockRepository.Setup(r => r.UpdateFulfilledWorkOrdersAsync(It.IsAny<IEnumerable<CoinExchangeOrder>>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.SellAsync(bitcoinToSell);

        // Assert
        Assert.True(result.IsSuccessful);

        // Verify repository calls with correct values
        _mockRepository.Verify(r => r.UpdateAvailableFundsAsync(
            It.Is<Dictionary<Guid, (decimal, decimal)>>(d => 
                d[exchangeId].Item1 == 2.0m && // CryptoGained
                d[exchangeId].Item2 == 100000m)), // EuroSpent (2.0 * 50000)
            Times.Once);
    }

    #endregion

    #region Edge Cases and Error Scenarios

    [Fact]
    public async Task BuyAsync_WithOrdersHavingZeroRemainingAmount_SkipsOrders()
    {
        // Arrange
        decimal bitcoinToBuy = 1.0m;
        var workingOrders = new List<WorkingBuyOrder>
        {
            CreateWorkingBuyOrder(Guid.NewGuid(), "Exchange1", 0m, 50000m, 0m, 5.0m), // Zero remaining
            CreateWorkingBuyOrder(Guid.NewGuid(), "Exchange2", 2.0m, 51000m, 2.0m, 3.0m)
        };

        _mockRepository.Setup(r => r.GetOrderedWorkingBuyOrdersAsync())
            .ReturnsAsync(workingOrders);
        _mockRepository.Setup(r => r.UpdateAvailableCryptoAsync(It.IsAny<Dictionary<Guid, decimal>>()))
            .Returns(Task.CompletedTask);
        _mockRepository.Setup(r => r.UpdateFulfilledWorkOrdersAsync(It.IsAny<IEnumerable<CoinExchangeOrder>>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.BuyAsync(bitcoinToBuy);

        // Assert
        Assert.True(result.IsSuccessful);
        Assert.Equal(1.0m, result.TotalBitcoinPurchased);
        Assert.Equal(51000m, result.TotalCost); // Uses second order
        Assert.Single(result.ExecutedOrders);
        Assert.Equal("Exchange2", result.ExecutedOrders[0].ExchangeName);
    }

    [Fact]
    public async Task SellAsync_WithOrdersHavingZeroExchangeFunds_SkipsOrders()
    {
        // Arrange
        decimal bitcoinToSell = 1.0m;
        var workingSellOrders = new List<WorkingSellOrder>
        {
            CreateWorkingSellOrder(Guid.NewGuid(), "Exchange1", 2.0m, 52000m, 2.0m, 0m), // Zero funds
            CreateWorkingSellOrder(Guid.NewGuid(), "Exchange2", 2.0m, 50000m, 2.0m, 100000m)
        };

        _mockRepository.Setup(r => r.GetOrderedWorkingSellOrdersAsync())
            .ReturnsAsync(workingSellOrders);
        _mockRepository.Setup(r => r.UpdateAvailableFundsAsync(It.IsAny<Dictionary<Guid, (decimal, decimal)>>()))
            .Returns(Task.CompletedTask);
        _mockRepository.Setup(r => r.UpdateFulfilledWorkOrdersAsync(It.IsAny<IEnumerable<CoinExchangeOrder>>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.SellAsync(bitcoinToSell);

        // Assert
        Assert.True(result.IsSuccessful);
        Assert.Equal(1.0m, result.TotalBitcoinSold);
        Assert.Equal(50000m, result.TotalReceived); // Uses second order
        Assert.Single(result.ExecutedOrders);
        Assert.Equal("Exchange2", result.ExecutedOrders[0].ExchangeName);
    }

    [Fact]
    public async Task BuyAsync_LogsErrorForInvalidAmount()
    {
        // Act
        await _service.BuyAsync(-1m);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Bitcoin amount to buy must be greater than 0, but is -1")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SellAsync_LogsErrorForInvalidAmount()
    {
        // Act
        await _service.SellAsync(-1m);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Bitcoin amount to sell must be greater than 0, but is -1")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Helper Methods

    private static WorkingBuyOrder CreateWorkingBuyOrder(Guid exchangeId, string exchangeName, 
        decimal remainingAmount, decimal price, decimal orderAmount, decimal exchangeCrypto)
    {
        return new WorkingBuyOrder
        {
            OriginalOrder = new CoinExchangeOrder
            {
                Id = Guid.NewGuid(),
                TimeUtc = DateTime.UtcNow,
                Type = "buy",
                Kind = "limit",
                Amount = orderAmount,
                Price = price
            },
            ExchangeId = exchangeId,
            ExchangeName = exchangeName,
            RemainingAmount = remainingAmount,
            ExchangeCrypto = exchangeCrypto
        };
    }

    private static WorkingSellOrder CreateWorkingSellOrder(Guid exchangeId, string exchangeName, 
        decimal remainingAmount, decimal price, decimal orderAmount, decimal exchangeFunds)
    {
        return new WorkingSellOrder
        {
            OriginalOrder = new CoinExchangeOrder
            {
                Id = Guid.NewGuid(),
                TimeUtc = DateTime.UtcNow,
                Type = "sell",
                Kind = "limit",
                Amount = orderAmount,
                Price = price
            },
            ExchangeId = exchangeId,
            ExchangeName = exchangeName,
            RemainingAmount = remainingAmount,
            ExchangeFunds = exchangeFunds
        };
    }

    #endregion
}
