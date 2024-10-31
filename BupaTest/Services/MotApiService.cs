using BupaTest.Exceptions;
using BupaTest.Models;
using Serilog;
using Serilog.Context;
using System.Net;
using System.Reflection;
using System.Text.Json;

namespace BupaTest.Services
{
    // Service responsible for fetching MOT data from the government API.
    public class MotApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;

        public MotApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, IHttpClientFactory clientFactory)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _clientFactory = clientFactory;
        }

        // Retrieves MOT data for the given vehicle registration number.
        public async Task<LatestMotData> GetMotData(string registrationNumber)
        {
            string requestId = Guid.NewGuid().ToString();
            string appVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";

            using (LogContext.PushProperty("RequestId", requestId))
            using (LogContext.PushProperty("RegistrationNumber", registrationNumber))
            using (LogContext.PushProperty("AppVersion", appVersion))
            {
                string apiKey = _configuration["MotApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    Log.Error("MOT API key is not configured.");
                    throw new MotApiException("API key is missing. Please check the configuration.", 401);
                }

                var apiUrl = $"https://beta.check-mot.service.gov.uk/trade/vehicles/mot-tests?registration={registrationNumber}";

                try
                {
                    var client = _clientFactory.CreateClient();
                    client.DefaultRequestHeaders.Add("x-api-key", apiKey);

                    Log.Information("Sending request to MOT API: {ApiUrl}", apiUrl);
                    var response = await client.GetAsync(apiUrl);

                    Log.Information("Received response from MOT API: {StatusCode}", response.StatusCode);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                        if (errorResponse != null)
                        {
                            Log.Error("API Error: {ErrorCode} - {ErrorMessage}", errorResponse.Code, errorResponse.Message);
                            throw new MotApiException(errorResponse.Message, (int)response.StatusCode, ((int)response.StatusCode).ToString());
                        }
                        else
                        {
                            switch (response.StatusCode)
                            {
                                case HttpStatusCode.Unauthorized:
                                    Log.Error("Unauthorized access to MOT API. Check your API key.");
                                    throw new MotApiException("Unauthorized access to MOT API.", (int)response.StatusCode);
                                case HttpStatusCode.InternalServerError:
                                    Log.Error("Internal Server Error from MOT API.");
                                    throw new MotApiException("MOT API is experiencing issues. Please try again later.", (int)response.StatusCode);
                                default:
                                    Log.Error("API Error occurred while fetching MOT data for registration {RegistrationNumber}. Status code: {StatusCode}", registrationNumber, response.StatusCode);
                                    throw new MotApiException("Failed to retrieve MOT data. Please try again later.", (int)response.StatusCode);
                            }
                        }
                    }

                    var vehicleDataList = await response.Content.ReadFromJsonAsync<List<VehicleData>>();
                    if (vehicleDataList != null && vehicleDataList.Count > 0)
                    {
                        var vehicleData = vehicleDataList[0];
                        var latestMotTest = vehicleData.MotTests?.OrderByDescending(t => t.CompletedDate).FirstOrDefault();
                        if (latestMotTest != null)
                        {
                            return new LatestMotData
                            {
                                Make = vehicleData.Make,
                                Model = vehicleData.Model,
                                Colour = vehicleData.PrimaryColour,
                                MotExpiryDate = latestMotTest.ExpiryDate,
                                Mileage = int.Parse(latestMotTest.OdometerValue).ToString()
                            };
                        }
                        else
                        {
                            Log.Warning("No MOT tests found for registration {Registration}", registrationNumber);
                            throw new MotApiException("No MOT tests found for this vehicle.", 404);
                        }
                    }
                    else
                    {
                        Log.Warning("No vehicle data found for registration {Registration}", registrationNumber);
                        throw new MotApiException("Vehicle not found.", 404);
                    }
                }
                catch (HttpRequestException ex)
                {
                    Log.Error(ex, "Network error during MOT API request.");
                    throw new MotApiException("Failed to connect to the MOT API. Please check your network connection.", 500);
                }
                catch (JsonException ex)
                {
                    Log.Error(ex, "Error deserializing JSON response from MOT API.");
                    throw new MotApiException("Invalid data received from the MOT API.", 500);
                }
                catch (MotApiException ex)
                {
                    Log.Error(ex, "MOT API error: {ErrorCode} - {ErrorMessage}", ex.ErrorCode, ex.Message);
                    throw;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "An unexpected error occurred while fetching MOT data.");
                    throw new MotApiException("An unexpected error occurred. Please try again later.", 500);
                }
            }
        }
    }
}
