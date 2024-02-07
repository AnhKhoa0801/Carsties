using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService;

public class AuctionUpdatedConsumer : IConsumer<AuctionUpdated>
{
	private readonly IMapper _mapper;

	public AuctionUpdatedConsumer(IMapper mapper)
	{
		_mapper = mapper;
	}

	public async Task Consume(ConsumeContext<AuctionUpdated> context)
	{
		Console.WriteLine("--> Consuming auction updated: " + context.Message.Id);

		var item = _mapper.Map<Item>(context.Message);

		var result = await DB.Update<Item>()
	   			.Match(a => a.ID == item.ID)
				.ModifyOnly(x => new
				{
					x.Make,
					x.Model,
					x.Year,
					x.Color,
					x.Mileage
				}, item)
	   			.ExecuteAsync();

		if (!result.IsAcknowledged)
			throw new MessageException(typeof(AuctionUpdated), "Failed to update item mongoDB");

		// await DB.Update<Item>()
		// 		.MatchID(item.ID)
		// 		.Modify(a => a.Make, item.Make)
		// 		.Modify(a => a.Model, item.Model)
		// 		.Modify(a => a.Year, item.Year)
		// 		.Modify(a => a.Color, item.Color)
		// 		.Modify(a => a.Mileage, item.Mileage)
		// 		.ExecuteAsync();
	}
}
