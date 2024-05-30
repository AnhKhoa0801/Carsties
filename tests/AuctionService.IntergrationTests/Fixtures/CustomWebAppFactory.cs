using System.Data;
using System.Data.Common;
using AuctionService.Data;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Testcontainers.PostgreSql;
using WebMotions.Fake.Authentication.JwtBearer;

namespace AuctionService.IntergrationTests;

public class CustomWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
	private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();

	public async Task InitializeAsync()
	{
		await _postgreSqlContainer.StartAsync();
	}

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.ConfigureTestServices(services =>
		{
			services.RemoveDbContext<AuctionDbContext>();

			var connection = _postgreSqlContainer.GetConnectionString();

			services.AddDbContext<AuctionDbContext>(options =>
			{
				options.UseNpgsql(_postgreSqlContainer.GetConnectionString());
				
			});

			services.AddMassTransitTestHarness();

			services.EnsureCreated<AuctionDbContext>();

			services.AddAuthentication(FakeJwtBearerDefaults.AuthenticationScheme)
				.AddFakeJwtBearer(opt =>{
					opt.BearerValueType = FakeJwtBearerBearerValueType.Jwt;
				});
		});
		
	}

	[Fact]
    public void ConnectionStateReturnsOpen()
    {
        // Given
        using DbConnection connection = new NpgsqlConnection(_postgreSqlContainer.GetConnectionString());

        // When
        connection.Open();

        // Then
        Assert.Equal(ConnectionState.Open, connection.State);
    }

	[Fact]
    public async Task ExecScriptReturnsSuccessful()
    {
        // Given
        const string scriptContent = "SELECT 1;";

        // When
        var execResult = await _postgreSqlContainer.ExecScriptAsync(scriptContent)
            .ConfigureAwait(true);

        // Then
        Assert.True(0L.Equals(execResult.ExitCode), execResult.Stderr);
        Assert.Empty(execResult.Stderr);
    }

	Task IAsyncLifetime.DisposeAsync() => _postgreSqlContainer.DisposeAsync().AsTask();
}