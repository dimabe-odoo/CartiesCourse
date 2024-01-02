using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Services;


public class AuctionSvcHttpClient
{
    private readonly HttpClient _httpClient;

    private readonly IConfiguration _configuration;
    public AuctionSvcHttpClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<List<Item>> GetItemsForSeachDb()
    {
        var lastUpdated = await DB.Find<Item, string>()
        .Sort(x => x.Descending(x => x.UpdatedAt))
        .Project(x => x.UpdatedAt.ToString())
        .ExecuteFirstAsync();

        var endpointUrl = _configuration["AuctionServiceUrl"] + "/api/auctions";

        if (!string.IsNullOrEmpty(lastUpdated))
            endpointUrl += "?date=" + lastUpdated;

        return await _httpClient.GetFromJsonAsync<List<Item>>(endpointUrl);
    }

    
}
