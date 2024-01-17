using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService;

public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
{
    public AuctionCreatedConsumer(IMapper mapper)
    {
        _mapper = mapper;
    }

    private IMapper _mapper { get; }

    public async Task Consume(ConsumeContext<AuctionCreated> context)
    {
        Console.WriteLine(" ==> Consuming Auction created: " + context.Message.Id);

        var item = _mapper.Map<Item>(context.Message);

        await item.SaveAsync();
    }
}
