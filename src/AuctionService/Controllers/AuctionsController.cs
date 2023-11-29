using AuctionService.Data.Connections;
using AuctionService.Data.Entities;
using AuctionService.DTOs;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[Route("api/auctions")]
[ApiController]
public class AuctionsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    public AuctionsController(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;        
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAll()
    {
        var auctions = await _context.Auctions
                            .Include(x => x.Item)
                            .OrderBy(x => x.Item!.Make)
                            .ToListAsync();
        return _mapper.Map<List<AuctionDto>>(auctions);
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
        return CreatedAtAction(nameof(GetById), new {auction.Id}, _mapper.Map<AuctionDto>(auction));
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