using AuctionService.Controllers;
using AuctionService.DTOs;
using AuctionService.Entities;
using AuctionService.RequestHelpers;
using AutoFixture;
using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AuctionService.UnitTests;

public class AuctionControllerTests
{
	private readonly Mock<IAuctionRepository> _auctionRepo;
	private readonly Mock<IPublishEndpoint> _publishEndpoint;
	private readonly Fixture _fixture;
	private readonly AuctionsController _controller;
	private readonly IMapper _mapper;
	public AuctionControllerTests()
	{
		_fixture = new Fixture();
		_auctionRepo = new Mock<IAuctionRepository>();
		_publishEndpoint = new Mock<IPublishEndpoint>();

		var mockMapper = new MapperConfiguration(mc =>
		{
			mc.AddMaps(typeof(MappingProfiles).Assembly);
		}).CreateMapper().ConfigurationProvider;

		_mapper = new Mapper(mockMapper);
		_controller = new AuctionsController(_auctionRepo.Object, _mapper, _publishEndpoint.Object)
		{
			ControllerContext = new ControllerContext
			{
				HttpContext = new DefaultHttpContext { User = Helpers.GetClaimsPrincipal() }
			}
		};
	}

	[Fact]
	public async Task GetAuctions_WithNoParams_Return10Auction()
	{
		// Arrange
		var auctions = _fixture.CreateMany<AuctionDto>(10).ToList();
		_auctionRepo.Setup(x => x.GetAuctionsAsync(null)).ReturnsAsync(auctions);

		// Act
		var result = await _controller.GetAllAuctions(null);

		// Assert
		Assert.Equal(10, result.Value.Count());
		Assert.IsType<ActionResult<List<AuctionDto>>>(result);
	}

	[Fact]
	public async Task GetAuctionById_WithValidGuid_ReturnAuction()
	{
		// Arrange
		var auction = _fixture.Create<AuctionDto>();
		_auctionRepo.Setup(x => x.GetAuctionByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);

		// Act
		var result = await _controller.GetAuctionById(auction.Id);

		// Assert
		Assert.Equal(auction.Make, result.Value.Make);
		Assert.IsType<ActionResult<AuctionDto>>(result);
	}

	[Fact]
	public async Task GetAuctionById_WithInValidGuid_ReturnNotFound()
	{
		// Arrange
		_auctionRepo.Setup(x => x.GetAuctionByIdAsync(It.IsAny<Guid>())).ReturnsAsync(value: null);

		// Act
		var result = await _controller.GetAuctionById(Guid.NewGuid());

		// Assert
		Assert.IsType<NotFoundResult>(result.Result);
	}

	[Fact]
	public async Task CreateAuction_WithValidCreateAuctionDto_ReturnCreatedAtAction()
	{
		// Arrange
		var auction = _fixture.Create<CreateAuctionDto>();

		_auctionRepo.Setup(x => x.AddAuction(It.IsAny<Auction>()));
		_auctionRepo.Setup(x => x.SaveChangesAsync()).ReturnsAsync(true);

		// Act
		var result = await _controller.CreateAuction(auction);
		var createdResult = result.Result as CreatedAtActionResult;

		// Assert
		Assert.NotNull(createdResult);
		Assert.Equal("GetAuctionById", createdResult.ActionName);
		Assert.IsType<AuctionDto>(createdResult.Value);
	}

	[Fact]
	public async Task CreateAuction_FailedSave_Returns400BadRequest()
	{
		// arrange
		var auctiondto = _fixture.Create<CreateAuctionDto>();
		_auctionRepo.Setup(x => x.AddAuction(It.IsAny<Auction>()));
		_auctionRepo.Setup(x => x.SaveChangesAsync()).ReturnsAsync(false);

		// act
		var result = await _controller.CreateAuction(auctiondto);

		// assert
		Assert.IsType<BadRequestObjectResult>(result.Result);
	}

	[Fact]
	public async Task UpdateAuction_WithUpdateAuctionDto_ReturnsOkResponse()
	{
		// Arrange
		var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
		auction.Item = _fixture.Build<Item>().Without(x => x.Auction).Create();
		auction.Seller = "test";

		var updateDto = _fixture.Create<UpdateAuctionDto>();

		_auctionRepo.Setup(x => x.GetAuctionEntityByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);
		_auctionRepo.Setup(x => x.SaveChangesAsync()).ReturnsAsync(true);

		// Act
		var result = await _controller.UpdateAuction(auction.Id, updateDto);

		// Assert
		Assert.IsType<OkResult>(result);
	}

	[Fact]
	public async Task UpdateAuction_WithInvalidUser_Returns403Forbid()
	{
		// Arrange
		var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
		auction.Seller = "not-test";

		var updateDto = _fixture.Create<UpdateAuctionDto>();

		_auctionRepo.Setup(x => x.GetAuctionEntityByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);

		// Act
		var result = await _controller.UpdateAuction(auction.Id, updateDto);

		// Assert
		Assert.IsType<ForbidResult>(result);
	}

	[Fact]
	public async Task UpdateAuction_WithInvalidGuid_ReturnsNotFound()
	{
		var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();

		var updateDto = _fixture.Create<UpdateAuctionDto>();

		_auctionRepo.Setup(x => x.GetAuctionEntityByIdAsync(It.IsAny<Guid>())).ReturnsAsync(value: null);

		// Act
		var result = await _controller.UpdateAuction(auction.Id, updateDto);

		// Assert
		Assert.IsType<NotFoundResult>(result);
	}

	[Fact]
	public async Task DeleteAuction_WithValidUser_ReturnsOkResponse()
	{
		var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
		auction.Item = _fixture.Build<Item>().Without(x => x.Auction).Create();
		auction.Seller = "test";


		_auctionRepo.Setup(x => x.GetAuctionEntityByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);
		_auctionRepo.Setup(x => x.SaveChangesAsync()).ReturnsAsync(true);

		// Act
		var result = await _controller.DeleteAuction(auction.Id);

		// Assert
		Assert.IsType<OkResult>(result);
	}

	[Fact]
	public async Task DeleteAuction_WithInvalidGuid_Returns404Response()
	{
		var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
		_auctionRepo.Setup(x => x.GetAuctionEntityByIdAsync(It.IsAny<Guid>())).ReturnsAsync(value: null);

		// Act
		var result = await _controller.DeleteAuction(auction.Id);

		// Assert
		Assert.IsType<NotFoundResult>(result);
	}

	[Fact]
	public async Task DeleteAuction_WithInvalidUser_Returns403Response()
	{
		var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
		auction.Seller = "not-test";

		_auctionRepo.Setup(x => x.GetAuctionEntityByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);

		// Act
		var result = await _controller.DeleteAuction(auction.Id);

		// Assert
		Assert.IsType<ForbidResult>(result);
	}
}
