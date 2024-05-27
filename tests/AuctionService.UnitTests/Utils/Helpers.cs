using System.Security.Claims;

namespace AuctionService.UnitTests;

public static class Helpers
{
	public static ClaimsPrincipal GetClaimsPrincipal(string name = "test")
	{
		var claims = new List<Claim>
		{
			new Claim(ClaimTypes.Name,"test"),
		};
		var identity = new ClaimsIdentity(claims, name);
		return new ClaimsPrincipal(identity);
	}
}
