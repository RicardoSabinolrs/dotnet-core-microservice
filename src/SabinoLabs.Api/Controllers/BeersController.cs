using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using JHipsterNet.Core.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SabinoLabs.Crosscutting.Exceptions;
using SabinoLabs.Domain;
using SabinoLabs.Domain.Dto;
using SabinoLabs.Domain.Entities;
using SabinoLabs.Domain.Services.Interfaces;
using SabinoLabs.Infrastructure.Web.Rest.Utilities;
using SabinoLabs.Web.Extensions;
using SabinoLabs.Web.Filters;
using SabinoLabs.Web.Rest.Utilities;

namespace SabinoLabs.Controllers
{
    [Authorize]
    [Route("api/beers")]
    [ApiController]
    public class BeersController : ControllerBase
    {
        private const string EntityName = "beer";
        private readonly IBeerService _beerService;
        private readonly ILogger<BeersController> _log;
        private readonly IMapper _mapper;

        public BeersController(ILogger<BeersController> log,
            IMapper mapper,
            IBeerService beerService)
        {
            _log = log;
            _mapper = mapper;
            _beerService = beerService;
        }

        [HttpPost]
        [ValidateModel]
        public async Task<ActionResult<BeerDto>> CreateBeer([FromBody] BeerDto beerDto)
        {
            _log.LogDebug($"REST request to save Beer : {beerDto}");
            if (beerDto.Id != 0)
            {
                throw new BadRequestAlertException("A new beer cannot already have an ID", EntityName, "idexists");
            }

            Beer beer = _mapper.Map<Beer>(beerDto);
            await _beerService.Save(beer);
            return CreatedAtAction(nameof(GetBeer), new {id = beer.Id}, beer)
                .WithHeaders(HeaderUtil.CreateEntityCreationAlert(EntityName, beer.Id.ToString()));
        }

        [HttpPut("{id}")]
        [ValidateModel]
        public async Task<IActionResult> UpdateBeer(long id, [FromBody] BeerDto beerDto)
        {
            _log.LogDebug($"REST request to update Beer : {beerDto}");
            if (beerDto.Id == 0)
            {
                throw new BadRequestAlertException("Invalid Id", EntityName, "idnull");
            }

            if (id != beerDto.Id)
            {
                throw new BadRequestAlertException("Invalid Id", EntityName, "idinvalid");
            }

            Beer beer = _mapper.Map<Beer>(beerDto);
            await _beerService.Save(beer);
            return Ok(beer)
                .WithHeaders(HeaderUtil.CreateEntityUpdateAlert(EntityName, beer.Id.ToString()));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BeerDto>>> GetAllBeers(IPageable pageable)
        {
            _log.LogDebug("REST request to get a page of Beers");
            IPage<Beer> result = await _beerService.FindAll(pageable);
            Page<BeerDto> page =
                new Page<BeerDto>(result.Content.Select(entity => _mapper.Map<BeerDto>(entity)).ToList(), pageable,
                    result.TotalElements);
            return Ok(((IPage<BeerDto>)page).Content).WithHeaders(page.GeneratePaginationHttpHeaders());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBeer([FromRoute] long id)
        {
            _log.LogDebug($"REST request to get Beer : {id}");
            Beer result = await _beerService.FindOne(id);
            BeerDto beerDto = _mapper.Map<BeerDto>(result);
            return ActionResultUtil.WrapOrNotFound(beerDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBeer([FromRoute] long id)
        {
            _log.LogDebug($"REST request to delete Beer : {id}");
            await _beerService.Delete(id);
            return Ok().WithHeaders(HeaderUtil.CreateEntityDeletionAlert(EntityName, id.ToString()));
        }
    }
}
