using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SmartGrainDry.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }

        [Route("api/sensor")]
        [ApiController]
        public class SensorController : ControllerBase
        {
            private static readonly object _lock = new(); // Ensures thread safety
            private static double _temperature = 0.0;
            private static double _humidity = 0.0;

            private readonly ILogger<SensorController> _logger;

            public SensorController(ILogger<SensorController> logger)
            {
                _logger = logger;
            }

            // API to receive ESP32 sensor data
            [HttpPost("data")]
            public IActionResult ReceiveSensorData([FromBody] SensorData data)
            {
                if (data == null)
                {
                    _logger.LogWarning("Invalid sensor data received.");
                    return BadRequest("Invalid data received");
                }

                lock (_lock)
                {
                    _temperature = data.Temperature;
                    _humidity = data.Humidity;
                }

                _logger.LogInformation($"Received sensor data: Temperature={_temperature}°C, Humidity={_humidity}%");

                return Ok(new { message = "Sensor data received!" });
            }

            // API to serve the latest sensor data to the frontend
            [HttpGet("data")]
            public IActionResult GetSensorData()
            {
                double temperature, humidity;

                lock (_lock)
                { 
                    temperature = _temperature;
                    humidity = _humidity;
                }

                return Ok(new { temperature, humidity });
            }
        }

        // Data Model for Sensor Data
        public class SensorData
        {
            public double Temperature { get; set; }
            public double Humidity { get; set; }
        }
    }
}