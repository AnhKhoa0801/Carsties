using AuctionService.Data;
using Grpc.Core;

namespace AuctionService;

public class GrpcAuctionService : GrpcAuction.GrpcAuctionBase
{
	private readonly AuctionDbContext _dbContext;

	public GrpcAuctionService(AuctionDbContext context)
	{
		_dbContext = context;
	}

	public override async Task<GrpcAuctionResponse> GetAuction(GetAuctionRequest request, ServerCallContext context)
	{
		Console.WriteLine("===> Received GetAuction request");

		var auction = await _dbContext.Auctions.FindAsync(Guid.Parse(request.Id))
			?? throw new RpcException(new Status(StatusCode.NotFound, "Auction not found auction service"));

		var response = new GrpcAuctionResponse
		{
			Auction = new GrpcAuctionModel
			{
				Id = auction.Id.ToString(),
				Seller = auction.Seller,
				AuctionEnd = auction.AuctionEnd.ToString(),
				ReservePrice = auction.ReservePrice,
			}

		};

		return response;
	}
}
