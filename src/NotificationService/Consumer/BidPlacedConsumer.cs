using Contracts;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace NotificationService;

public class BidPlacedConsumer : IConsumer<BidPlaced>
{
	private readonly IHubContext<NotificationHubs> _hubContext;

	public BidPlacedConsumer(IHubContext<NotificationHubs> hubContext)
	{
		_hubContext = hubContext;
	}
	public async Task Consume(ConsumeContext<BidPlaced> context)
	{
		Console.WriteLine("===> notify-svc received message BidPlaced");

		await _hubContext.Clients.All.SendAsync("BidPlaced", context.Message);
	}
}
