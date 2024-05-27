﻿
using System.Net;
using System.Net.Http.Json;
using AuctionService.Data;
using AuctionService.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.IntergrationTests;

public class AuctionControllerTests : IClassFixture<CustomWebAppFactory>, IAsyncLifetime
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

	public Task DisposeAsync()
	{
		using var scope = _factory.Services.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
		DbHelper.ReinitDbForTests(db);
		return Task.CompletedTask;
	}

	public Task InitializeAsync() => Task.CompletedTask;
}

