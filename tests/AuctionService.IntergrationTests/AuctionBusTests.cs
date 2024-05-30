
using System.Net;
using System.Net.Http.Json;
using AuctionService.Data;
using AuctionService.DTOs;
using Contracts;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.IntergrationTests;

[Collection("SharedFixture")]
public class AuctionBusTests : IAsyncLifetime
{
	private readonly CustomWebAppFactory _factory;
	private readonly HttpClient _httpClient;
	private ITestHarness _harness;

	public AuctionBusTests(CustomWebAppFactory factory)
	{
		_factory = factory;
		_httpClient = factory.CreateClient();
		_harness = factory.Services.GetTestHarness();

	}

	[Fact]
	public async Task CreateAuction_WithValidAuction_ShouldPyblishAuction()
	{
		// arrange
		var auction = GetAuctionForCreate();
		_httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));

		// act
		var response = await _httpClient.PostAsJsonAsync("/api/auctions", auction);

		// assert
		response.EnsureSuccessStatusCode();
		Assert.True(await _harness.Published.Any<AuctionCreated>());
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
}
