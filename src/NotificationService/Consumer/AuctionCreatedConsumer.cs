using Contracts;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace NotificationService;

public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
{
	private readonly IHubContext<NotificationHubs> _hubContext;

	public AuctionCreatedConsumer(IHubContext<NotificationHubs> hubContext)
	{
		_hubContext = hubContext;
	}
	public async Task Consume(ConsumeContext<AuctionCreated> context)
	{
		Console.WriteLine("===> notify-svc received message AuctionCreated");

		await _hubContext.Clients.All.SendAsync("AuctionCreated", context.Message);
	}
}
