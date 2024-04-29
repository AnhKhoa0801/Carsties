using AutoMapper;
using Contracts;
using MassTransit.Caching;

namespace BiddingService;

public class MappingProfiles : Profile
{
	public MappingProfiles()
	{
		CreateMap<Bid, BidDto>();
		CreateMap<Bid, BidPlaced>();
	}
}
