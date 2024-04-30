using Grpc.Net.Client;

namespace BiddingService;

public class GrpcAuctionClient
{
	private readonly ILogger<GrpcAuctionClient> _logger;
	private readonly IConfiguration _configuration;

	public GrpcAuctionClient(ILogger<GrpcAuctionClient> logger, IConfiguration configuration)
	{
		_logger = logger;
		_configuration = configuration;
	}

	public Auction GetAuction(string id)
	{
		_logger.LogInformation("===> Calling GetAuction request for {id}", id);

		var channel = GrpcChannel.ForAddress(_configuration["GrpcAuction"]);
		var client = new GrpcAuction.GrpcAuctionClient(channel);
		var request = new GetAuctionRequest { Id = id };

		try
		{
			var response = client.GetAuction(request);
			var auction = new Auction
			{
				ID = response.Auction.Id,
				AuctionEnd = DateTime.Parse(response.Auction.AuctionEnd),
				Seller = response.Auction.Seller,
				ReservePrice = response.Auction.ReservePrice,
			};

			return auction;

		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while calling GetAuction.");
			return null;
		}

	}
}
