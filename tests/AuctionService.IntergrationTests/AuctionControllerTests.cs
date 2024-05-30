
using System.Net;
using System.Net.Http.Json;
using AuctionService.Data;
using AuctionService.DTOs;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.IntergrationTests;

[Collection("SharedFixture")]
public class AuctionControllerTests : IAsyncLifetime
{

	private readonly CustomWebAppFactory _factory;
	private readonly HttpClient _httpClient;
	private const string GT_ID = "afbee524-5972-4075-8800-7d1f9d7b0a0c";

	public AuctionControllerTests(CustomWebAppFactory factory)
	{
		_factory = factory;
		_httpClient = factory.CreateClient();

	}

	[Fact]
	public async Task GetAuctions_ShouldReturn3Auctions()
	{
		// Arrange

		// act
		var response = await _httpClient.GetFromJsonAsync<List<AuctionDto>>("/api/auctions");

		// Assert
		Assert.Equal(3, response.Count);
	}

	[Fact]
	public async Task GetAuctionById_WithValidId_ReturnAuction()
	{
		// Arrange

		// act
		var response = await _httpClient.GetFromJsonAsync<AuctionDto>($"/api/auctions/{GT_ID}");

		// Assert
		Assert.Equal("GT", response.Model);
	}

	[Fact]
	public async Task GetAuctionById_InValidId_Return404()
	{
		// Arrange

		// act
		var response = await _httpClient.GetAsync($"/api/auctions/{Guid.NewGuid()}");

		// Assert
		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	[Fact]
	public async Task GetAuctionById_InValidId_Return400()
	{
		// Arrange

		// act
		var response = await _httpClient.GetAsync($"/api/auctions/notaguid");

		// Assert
		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
	}

	[Fact]
	public async Task CreateAuction_WithNoAuth_Return401()
	{
		// Arrange
		var auction = new CreateAuctionDto { Make = "test" };

		// act
		var response = await _httpClient.PostAsJsonAsync($"/api/auctions", auction);

		// Assert
		Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
	}

	[Fact]
	public async Task CreateAuction_WithAuth_Return201()
	{
		// Arrange
		var auction = GetAuctionForCreate();
		_httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));

		// act
		var response = await _httpClient.PostAsJsonAsync($"/api/auctions", auction);

		// Assert
		response.EnsureSuccessStatusCode();
		Assert.Equal(HttpStatusCode.Created, response.StatusCode);

		var createdAuction = await response.Content.ReadFromJsonAsync<AuctionDto>();
		Assert.Equal("bob", createdAuction.Seller);
	}

	[Fact]
	public async Task CreateAuction_WithInvalidCreateAuctionDto_ShouldReturn400()
	{
		// Arrange
		var auction = GetAuctionForCreate();
		auction.Make = null;
		_httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));

		// act
		var response = await _httpClient.PostAsJsonAsync($"/api/auctions", auction);

		// Assert
		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
	}

	[Fact]
	public async Task UpdateAuction_WithValidUpdateDtoAndUser_ShouldReturn200()
	{
		var (id, auction) = GetAuctionForUpdate();
		auction.Year = 2021;
		_httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));

		// act
		var response = await _httpClient.PutAsJsonAsync($"/api/auctions/{id}", auction);

		// Assert
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	}

	[Fact]
	public async Task UpdateAuction_WithValidUpdateDtoAndInvalidUser_ShouldReturn403()
	{
		var (id, auction) = GetAuctionForUpdate();
		auction.Year = 2021;
		_httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("notbob"));

		// act
		var response = await _httpClient.PutAsJsonAsync($"/api/auctions/{id}", auction);

		// Assert
		Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
	}

	public Task DisposeAsync()
	{
		using var scope = _factory.Services.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
		DbHelper.ReinitDbForTests(db);
		return Task.CompletedTask;
	}

	public Task InitializeAsync() => Task.CompletedTask;

	private CreateAuctionDto GetAuctionForCreate()
	{
		return new CreateAuctionDto
		{
			Make = "test",
			Model = "test",
			ImageUrl = "test",
			Color = "test",
			Mileage = 10,
			Year = 10,
			ReservePrice = 10,

		};
	}
	private (string, UpdateAuctionDto) GetAuctionForUpdate()
	{
		return ("afbee524-5972-4075-8800-7d1f9d7b0a0c", new UpdateAuctionDto
		{
			Make = "Ford",
			Model = "GT",
			Color = "White",
			Mileage = 50000,
			Year = 2020,
		});
	}
}

