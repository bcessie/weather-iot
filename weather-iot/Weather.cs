using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NLog;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using weather_domain.Models;
using TransportType = Microsoft.Azure.Devices.Client.TransportType;

namespace weather_iot
{
    public class Weather
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private WeatherModel GetWeather()
        {
            WeatherModel weatherModel = null;

            using (WebClient wc = new WebClient())
            {
                try
                {
                    string baseUrl = ConfigurationManager.AppSettings["weather-BaseUrl"];
                    string cityId = ConfigurationManager.AppSettings["weather-CityId"];
                    string units = ConfigurationManager.AppSettings["weather-Units"];
                    string appId = ConfigurationManager.AppSettings["weather-AppId"];

                    string openWeatherConnectionString = $"{baseUrl}?id={cityId}&units={units}&appid={appId}";

                    logger.Info(openWeatherConnectionString);

                    string weatherJson = wc.DownloadString(openWeatherConnectionString);

                    logger.Info(weatherJson);

                    weatherModel = JsonConvert.DeserializeObject<WeatherModel>(weatherJson);
                }
                catch (Exception err)
                {
                    logger.Error(err.Message);
                }

                return weatherModel;
            }
        }

        private void SendWeatherTelementry(WeatherModel weatherModel)
        {
            if (weatherModel != null && weatherModel.cnt > 0)
            {
                string hostName = ConfigurationManager.AppSettings["azure-HostName"];
                string deviceId = ConfigurationManager.AppSettings["azure-DeviceId"];
                string accessKey = ConfigurationManager.AppSettings["azure-SharedAccessKey"];

                string deviceConnectionString = $"HostName={hostName};DeviceId={deviceId};SharedAccessKey={accessKey}";

                // Connect to the IoT hub using the MQTT protocol
                DeviceClient deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, TransportType.Mqtt);

                foreach (var weatherModelItem in weatherModel.list)
                {
                    try
                    {
                        string json = JsonConvert.SerializeObject(weatherModelItem);
                        var message = new Message(Encoding.ASCII.GetBytes(json));

                        // Add a custom application property to the message.
                        // An IoT hub can filter on these properties without access to the message body.
                        message.Properties.Add("City", weatherModelItem.name);

                        deviceClient.SendEventAsync(message);

                        logger.Info("Message sent for " + weatherModelItem.name);
                    }
                    catch (Exception err)
                    {
                        logger.Error(err.Message);
                    }
                }
            }
        }

        public void ProcessWeather()
        {
            WeatherModel weatherModel = GetWeather();

            SendWeatherTelementry(weatherModel);

            RestClient restClient = new RestClient(ConfigurationManager.AppSettings["weather-ClientApiUrl"]);
            var request = new RestRequest("Weather", Method.POST);
            request.AddJsonBody(weatherModel);

            IRestResponse response = restClient.Execute(request);
        }
    }
}
