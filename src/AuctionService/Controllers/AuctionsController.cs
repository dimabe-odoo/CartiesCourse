using AuctionService.Data.Connections;
using AuctionService.Data.Entities;
using AuctionService.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[Route("api/auctions")]
[ApiController]
public class AuctionsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionsController(ApplicationDbContext context, IMapper mapper, IPublishEndpoint publishEndpoint)
    {
        _context = context;        
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAll(string? date)
    {

        var query = _context.Auctions.OrderBy(x => x.Item.Make).AsQueryable();

        if (!string.IsNullOrEmpty(date))
            query = query
                .Where(x => x.UpdatedAt.Value.CompareTo(
                        DateTime.Parse(date).ToUniversalTime()) > 0);

        
        return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();
    }

    [HttpGet("{id}")]

    public async Task<ActionResult<AuctionDto>> GetById(Guid id)
    {
        var auction = await _context.Auctions
                    .Include(x => x.Item)
                    .FirstOrDefaultAsync(x => x.Id == id);

        return _mapper.Map<AuctionDto>(auction);
    }

    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto dto)
    {
        var auction = _mapper.Map<Auction>(dto);
        // :TODO get a current user connected
        auction.Seller = "test";

        _context.Auctions.Add(auction);
        await _context.SaveChangesAsync();

        var newAuction = _mapper.Map<AuctionDto>(auction);

        await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));

        return CreatedAtAction(nameof(GetById), new {auction.Id}, _mapper.Map<AuctionDto>(newAuction));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto dto)
    {
        var auction = await _context.Auctions
                    .Include(x => x.Item)
                    .FirstOrDefaultAsync(x => x.Id == id);

        if (auction is null) return NotFound();

        // TODO: check current user == auction.Seller

        auction.Item!.Make = dto.Make ?? auction.Item!.Make;
        auction.Item!.Model = dto.Model ?? auction.Item!.Model;
        auction.Item!.Color = dto.Color ?? auction.Item!.Color;
        auction.Item!.Mileage = dto.Mileage ?? auction.Item!.Mileage;
        auction.Item!.Year = dto.Year ?? auction.Item!.Year;

        var result = await _context.SaveChangesAsync() > 0;
        if (result) return Ok();

        return BadRequest("Error updating Auction");
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        var auction = await _context.Auctions.FindAsync(id);

        if (auction is null) return NotFound();

        _context.Auctions.Remove(auction);

        var result = await _context.SaveChangesAsync() > 0;

        if (result) return NoContent();

        return BadRequest("Error deleting Auction");
    }




}