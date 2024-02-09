using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService;

public class AuctionFinishedConsumer : IConsumer<AuctionsFinished>
{
	public async Task Consume(ConsumeContext<AuctionsFinished> context)
	{
		var auction = await DB.Find<Item>().OneAsync(context.Message.AuctionId);

		if (context.Message.ItemSold)
		{
			auction.Winner = context.Message.Winner;
			auction.SoldAmount = (int)context.Message.Amount;
		}

		auction.Status = "Finished";

		await auction.SaveAsync();
	}
}
