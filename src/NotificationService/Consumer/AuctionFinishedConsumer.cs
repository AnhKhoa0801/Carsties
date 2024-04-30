using Contracts;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace NotificationService;

public class AuctionFinishedConsumer : IConsumer<AuctionsFinished>
{
	private readonly IHubContext<NotificationHubs> _hubContext;

	public AuctionFinishedConsumer(IHubContext<NotificationHubs> hubContext)
	{
		_hubContext = hubContext;
	}
	public async Task Consume(ConsumeContext<AuctionsFinished> context)
	{
		Console.WriteLine("===> notify-svc received message AuctionsFinished");

		await _hubContext.Clients.All.SendAsync("AuctionsFinished", context.Message);
	}
}
