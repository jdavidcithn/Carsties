using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.API.Models;
using SearchService.API.RequestHelpers;

namespace SearchService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<Item>>> SearchItems([FromQuery] SearchParams searchParams)
        {
            var query = DB.PagedSearch<Item, Item>();

            if (!string.IsNullOrEmpty(searchParams.SearchTerm))
            {
                query.Match(Search.Full, searchParams.SearchTerm).SortByTextScore();
            }

            query = searchParams.OrderBy switch
            {
                "make" => query.Sort(x => x.Ascending(a => a.Make)),
                "new" => query.Sort(x => x.Descending(d => d.CreatedAt)),
                _ => query.Sort(x => x.Ascending(a => a.AuctionEnd))
            };

            query = searchParams.FilterBy switch
            {
                "finished" => query.Match(x => x.AuctionEnd < DateTime.Now),
                "endingSoon" => query.Match(x => x.AuctionEnd < DateTime.UtcNow.AddHours(6) && x.AuctionEnd > DateTime.UtcNow),
                _ => query.Match(x => x.AuctionEnd > DateTime.UtcNow)
            };
            
            if (!string.IsNullOrEmpty(searchParams.Seller))
            {
                query.Match(x => x.Seller == searchParams.Seller);
            }            
            
            if (!string.IsNullOrEmpty(searchParams.Winner))
            {
                query.Match(x => x.Seller == searchParams.Winner);
            }

            query.PageNumber(searchParams.PageNumber);
            query.PageSize(searchParams.PageSize);

            var result = await query.ExecuteAsync();

            return Ok(new { results = result.Results, pageCount = result.PageCount, totalCount = result.TotalCount });
        }
    }
}
