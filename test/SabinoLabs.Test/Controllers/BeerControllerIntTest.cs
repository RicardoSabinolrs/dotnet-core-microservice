using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using SabinoLabs.Configuration.AutoMapper;
using SabinoLabs.Domain.Dto;
using SabinoLabs.Domain.Entities;
using SabinoLabs.Domain.Repositories.Interfaces;
using SabinoLabs.Test.Setup;
using Xunit;

namespace SabinoLabs.Test.Controllers
{
    public class BeersControllerIntTest
    {
        private const string DefaultName = "AAAAAAAAAA";
        private const string UpdatedName = "BBBBBBBBBB";

        private const string DefaultIbu = "AAAAAAAAAA";
        private const string UpdatedIbu = "BBBBBBBBBB";

        private const string DefaultStyle = "AAAAAAAAAA";
        private const string UpdatedStyle = "BBBBBBBBBB";

        private const string DefaultDescription = "AAAAAAAAAA";
        private const string UpdatedDescription = "BBBBBBBBBB";

        private const string DefaultAlcoholTenor = "AAAAAAAAAA";
        private const string UpdatedAlcoholTenor = "BBBBBBBBBB";
        private readonly IBeerRepository _beerRepository;
        private readonly HttpClient _client;

        private readonly AppWebApplicationFactory<TestStartup> _factory;

        private readonly IMapper _mapper;

        private Beer _beer;

        public BeersControllerIntTest()
        {
            _factory = new AppWebApplicationFactory<TestStartup>().WithMockUser();
            _client = _factory.CreateClient();

            _beerRepository = _factory.GetRequiredService<IBeerRepository>();

            MapperConfiguration config = new MapperConfiguration(cfg => { cfg.AddProfile(new AutoMapperProfile()); });

            _mapper = config.CreateMapper();

            InitTest();
        }

        private Beer CreateEntity() =>
            new()
            {
                Name = DefaultName,
                Ibu = DefaultIbu,
                Style = DefaultStyle,
                Description = DefaultDescription,
                AlcoholTenor = DefaultAlcoholTenor
            };

        private void InitTest() => _beer = CreateEntity();

        [Fact]
        public async Task CreateBeer()
        {
            int databaseSizeBeforeCreate = await _beerRepository.CountAsync();

            // Create the Beer
            BeerDto _beerDto = _mapper.Map<BeerDto>(_beer);
            HttpResponseMessage response = await _client.PostAsync("/api/beers", TestUtil.ToJsonContent(_beerDto));
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            // Validate the Beer in the database
            IEnumerable<Beer> beerList = await _beerRepository.GetAllAsync();
            beerList.Count().Should().Be(databaseSizeBeforeCreate + 1);
            Beer testBeer = beerList.Last();
            testBeer.Name.Should().Be(DefaultName);
            testBeer.Ibu.Should().Be(DefaultIbu);
            testBeer.Style.Should().Be(DefaultStyle);
            testBeer.Description.Should().Be(DefaultDescription);
            testBeer.AlcoholTenor.Should().Be(DefaultAlcoholTenor);
        }

        [Fact]
        public async Task CreateBeerWithExistingId()
        {
            int databaseSizeBeforeCreate = await _beerRepository.CountAsync();
            databaseSizeBeforeCreate.Should().Be(0);
            // Create the Beer with an existing ID
            _beer.Id = 1L;

            // An entity with an existing ID cannot be created, so this API call must fail
            BeerDto _beerDto = _mapper.Map<BeerDto>(_beer);
            HttpResponseMessage response = await _client.PostAsync("/api/beers", TestUtil.ToJsonContent(_beerDto));

            // Validate the Beer in the database
            IEnumerable<Beer> beerList = await _beerRepository.GetAllAsync();
            beerList.Count().Should().Be(databaseSizeBeforeCreate);
        }

        [Fact]
        public async Task GetAllBeers()
        {
            // Initialize the database
            await _beerRepository.CreateOrUpdateAsync(_beer);
            await _beerRepository.SaveChangesAsync();

            // Get all the beerList
            HttpResponseMessage response = await _client.GetAsync("/api/beers?sort=id,desc");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            JToken json = JToken.Parse(await response.Content.ReadAsStringAsync());
            json.SelectTokens("$.[*].id").Should().Contain(_beer.Id);
            json.SelectTokens("$.[*].name").Should().Contain(DefaultName);
            json.SelectTokens("$.[*].ibu").Should().Contain(DefaultIbu);
            json.SelectTokens("$.[*].style").Should().Contain(DefaultStyle);
            json.SelectTokens("$.[*].description").Should().Contain(DefaultDescription);
            json.SelectTokens("$.[*].alcoholTenor").Should().Contain(DefaultAlcoholTenor);
        }

        [Fact]
        public async Task GetBeer()
        {
            // Initialize the database
            await _beerRepository.CreateOrUpdateAsync(_beer);
            await _beerRepository.SaveChangesAsync();

            // Get the beer
            HttpResponseMessage response = await _client.GetAsync($"/api/beers/{_beer.Id}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            JToken json = JToken.Parse(await response.Content.ReadAsStringAsync());
            json.SelectTokens("$.id").Should().Contain(_beer.Id);
            json.SelectTokens("$.name").Should().Contain(DefaultName);
            json.SelectTokens("$.ibu").Should().Contain(DefaultIbu);
            json.SelectTokens("$.style").Should().Contain(DefaultStyle);
            json.SelectTokens("$.description").Should().Contain(DefaultDescription);
            json.SelectTokens("$.alcoholTenor").Should().Contain(DefaultAlcoholTenor);
        }

        [Fact]
        public async Task GetNonExistingBeer()
        {
            HttpResponseMessage response = await _client.GetAsync("/api/beers/" + long.MaxValue);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UpdateBeer()
        {
            // Initialize the database
            await _beerRepository.CreateOrUpdateAsync(_beer);
            await _beerRepository.SaveChangesAsync();
            int databaseSizeBeforeUpdate = await _beerRepository.CountAsync();

            // Update the beer
            Beer updatedBeer = await _beerRepository.QueryHelper().GetOneAsync(it => it.Id == _beer.Id);
            // Disconnect from session so that the updates on updatedBeer are not directly saved in db
            //TODO detach
            updatedBeer.Name = UpdatedName;
            updatedBeer.Ibu = UpdatedIbu;
            updatedBeer.Style = UpdatedStyle;
            updatedBeer.Description = UpdatedDescription;
            updatedBeer.AlcoholTenor = UpdatedAlcoholTenor;

            BeerDto updatedBeerDto = _mapper.Map<BeerDto>(_beer);
            HttpResponseMessage response =
                await _client.PutAsync($"/api/beers/{_beer.Id}", TestUtil.ToJsonContent(updatedBeerDto));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Validate the Beer in the database
            IEnumerable<Beer> beerList = await _beerRepository.GetAllAsync();
            beerList.Count().Should().Be(databaseSizeBeforeUpdate);
            Beer testBeer = beerList.Last();
            testBeer.Name.Should().Be(UpdatedName);
            testBeer.Ibu.Should().Be(UpdatedIbu);
            testBeer.Style.Should().Be(UpdatedStyle);
            testBeer.Description.Should().Be(UpdatedDescription);
            testBeer.AlcoholTenor.Should().Be(UpdatedAlcoholTenor);
        }

        [Fact]
        public async Task UpdateNonExistingBeer()
        {
            int databaseSizeBeforeUpdate = await _beerRepository.CountAsync();

            // If the entity doesn't have an ID, it will throw BadRequestAlertException
            BeerDto _beerDto = _mapper.Map<BeerDto>(_beer);
            HttpResponseMessage response = await _client.PutAsync("/api/beers/1", TestUtil.ToJsonContent(_beerDto));
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            // Validate the Beer in the database
            IEnumerable<Beer> beerList = await _beerRepository.GetAllAsync();
            beerList.Count().Should().Be(databaseSizeBeforeUpdate);
        }

        [Fact]
        public async Task DeleteBeer()
        {
            // Initialize the database
            await _beerRepository.CreateOrUpdateAsync(_beer);
            await _beerRepository.SaveChangesAsync();
            int databaseSizeBeforeDelete = await _beerRepository.CountAsync();

            HttpResponseMessage response = await _client.DeleteAsync($"/api/beers/{_beer.Id}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Validate the database is empty
            IEnumerable<Beer> beerList = await _beerRepository.GetAllAsync();
            beerList.Count().Should().Be(databaseSizeBeforeDelete - 1);
        }

        [Fact]
        public void EqualsVerifier()
        {
            TestUtil.EqualsVerifier(typeof(Beer));
            Beer beer1 = new Beer {Id = 1L};
            Beer beer2 = new Beer {Id = beer1.Id};
            beer1.Should().Be(beer2);
            beer2.Id = 2L;
            beer1.Should().NotBe(beer2);
            beer1.Id = 0;
            beer1.Should().NotBe(beer2);
        }
    }
}
