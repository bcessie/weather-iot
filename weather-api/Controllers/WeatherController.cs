using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using weather_domain.Models;

namespace weather_api.Controllers
{
    /// <summary>
    /// API weather controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class WeatherController : ControllerBase
    {
        private readonly SocketManager _socketManager;

        public WeatherController(SocketManager socketManager)
        {
            _socketManager = socketManager;
        }

        [HttpPost]
        public async Task Post(WeatherModel model)
        {
            await _socketManager.SendMessageToAllAsync(JsonConvert.SerializeObject(model));
        }
    }
}
