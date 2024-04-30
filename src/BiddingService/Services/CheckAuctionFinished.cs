

using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace BiddingService;

public class CheckAuctionFinished : BackgroundService
{
	private ILogger<CheckAuctionFinished> _logger;
	private IServiceProvider _serviceProvider;

	public CheckAuctionFinished(ILogger<CheckAuctionFinished> logger, IServiceProvider serviceProvider)
	{
		_logger = logger;
		_serviceProvider = serviceProvider;
	}
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		_logger.LogInformation("======> CheckAuctionFinished is starting.");
		_logger.LogInformation("======> CheckAuctionFinished is starting.");

		stoppingToken.Register(() =>
			_logger.LogInformation("======> CheckAuctionFinished is stopping."));

		while (!stoppingToken.IsCancellationRequested)
		{
			_logger.LogInformation("======> CheckAuctionFinished is checking {time}.", DateTime.UtcNow);
			await CheckAuctions(stoppingToken);

			await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
		}
	}

	private async Task CheckAuctions(CancellationToken stoppingToken)
	{
		var finishedAuctions = await DB.Find<Auction>()
			.Match(a => a.AuctionEnd <= DateTime.UtcNow)
			.Match(a => !a.Finished)
			.ExecuteAsync(stoppingToken);

		if (finishedAuctions.Count == 0) return;

		_logger.LogInformation("===> Found {count} auctions to finish.", finishedAuctions.Count);

		using var scope = _serviceProvider.CreateScope();
		var endpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

		foreach (var item in finishedAuctions)
		{
			item.Finished = true;
			await item.SaveAsync(null, stoppingToken);

			var winningBid = await DB.Find<Bid>()
				.Match(b => b.AuctionId == item.ID)
				.Match(b => b.BidStatus == BidStatus.Accepted)
				.Sort(x => x.Descending(a => a.Amount))
				.ExecuteFirstAsync(stoppingToken);

			await endpoint.Publish(new AuctionsFinished
			{
				AuctionId = item.ID,
				ItemSold = winningBid is not null,
				Winner = winningBid?.Bidder,
				Seller = item.Seller,
				Amount = winningBid?.Amount
			}, stoppingToken);

		}

	}
}
