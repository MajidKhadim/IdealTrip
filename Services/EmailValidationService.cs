using IdealTrip.Models;
using Newtonsoft.Json;

namespace IdealTrip.Services
{
	public class EmailValidationService
	{
		private readonly HttpClient _httpClient;
		private readonly string _kickboxApiKey;

		public EmailValidationService(IConfiguration config)
		{
			_httpClient = new HttpClient();
			_kickboxApiKey = config["Kickbox:ApiKey"]; // Add this in appsettings.json
		}

		public async Task<bool> IsEmailRealAsync(string email)
		{
			var response = await _httpClient.GetAsync($"https://api.kickbox.com/v2/verify?email={email}&apikey={_kickboxApiKey}");

			if (!response.IsSuccessStatusCode)
				return false;

			var content = await response.Content.ReadAsStringAsync();
			var result = JsonConvert.DeserializeObject<KickboxResponse>(content);

			return result.result == "deliverable";
		}
	}

}
