using AutoMapper;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;

namespace BiddingService;

[ApiController]
[Route("api/[controller]")]
public class BidsController : ControllerBase
{
	private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;

    public BidsController(IMapper mapper, IPublishEndpoint publishEndpoint)
	{
		_mapper = mapper;
        _publishEndpoint = publishEndpoint;
    }
	[Authorize]
	[HttpPost]
	public async Task<ActionResult<BidDto>> PlaceBid(string auctionId, int amount)
	{
		var auction = await DB.Find<Auction>().OneAsync(auctionId);

		if (auction is null)
		{
			return NotFound();
		}

		if (auction.Seller == User.Identity.Name)
		{
			return BadRequest("You can't bid on your own auction");
		}

		var bid = new Bid
		{
			AuctionId = auctionId,
			Bidder = User.Identity.Name,
			Amount = amount
		};

		if (auction.AuctionEnd < DateTime.UtcNow)
		{
			bid.BidStatus = BidStatus.Finished;
		}
		else
		{
			var highBid = await DB.Find<Bid>()
						.Match(b => b.AuctionId == auctionId)
						.Sort(b => b.Descending(x => x.Amount))
						.ExecuteFirstAsync();

			if (highBid is not null && highBid.Amount >= amount || highBid is null)
			{
				bid.BidStatus = amount > auction.ReservePrice ? BidStatus.Accepted : BidStatus.AcceptedBelowReserve;
			}

			if (highBid is not null && highBid.Amount < amount)
			{
				bid.BidStatus = BidStatus.TooLow;
			}
		}

		await DB.SaveAsync(bid);

		await _publishEndpoint.Publish<BidPlaced>(_mapper.Map<BidPlaced>(bid));

		return Ok(_mapper.Map<BidDto>(bid));

	}

	[HttpGet("{auctionId}")]
	public async Task<ActionResult<List<BidDto>>> GetBidsForAuction(string auctionId)
	{
		var bids = await DB.Find<Bid>()
			.Match(b => b.AuctionId == auctionId)
			.Sort(b => b.Descending(x => x.BidTime))
			.ExecuteAsync();

		return Ok(bids.Select(_mapper.Map<BidDto>).ToList());
	}
}
