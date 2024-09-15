using Infra.Redis.Interfaces;
using Infra.Redis.Interfaces.V2;
using Microsoft.AspNetCore.Mvc;
using WebApi.Boundaries.UseCases;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "Get")]
        public async Task<IActionResult> Get([FromServices] ICacheService cacheService
            , ICacheServiceV2 cacheServiceV2,
            [FromServices] IGetWeatherForecastUseCase getWeatherForecastUseCase, [FromQuery] string key)
        {
            // var output = await cacheService.GetOrSetAsync(
            //     key,
            //     async () => await getWeatherForecastUseCase.Get());

            var output = await cacheServiceV2.GetOrSetAsync(
                key,
                async () => await getWeatherForecastUseCase.Get());
            
            return Ok(output.Data);
        }
    }
}