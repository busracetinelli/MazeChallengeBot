using MazeChallengeBot.Entities;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace MazeChallengeBot.Adapters
{
	public class OpenAPIManager : IOpenAPIService
	{

		private readonly string _apiUrl;
		private readonly string _token;
		private readonly HttpClient _client;
		private string register = "/api/player/register";
		private string getMazes = "/api/mazes/all";
		private string enterMaze = "/api/mazes/enter";
		private string forgetMaze = "/api/player/forget";
		private string mazeMove = "/api/maze/move";
		private string exitMaze = "/api/maze/exit";
		private string collectScore = "/api/maze/collectScore";

		public OpenAPIManager(IConfiguration configuration, HttpClient client)
		{
			_apiUrl = configuration["OpenAPI:Url"] ?? "";
			_token = configuration["OpenAPI:Token"] ?? "";
			client.BaseAddress = new Uri(_apiUrl);
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
			_client = client;
		}

		public string GetApiUrl()
		{
			return _apiUrl;
		}

		public string GetToken()
		{
			return _token;
		}

		public string GetRegisterEndpoint()
		{
			return register;
		}

		public string GetMazes()
		{
			return getMazes;
		}

		public string GetEnterMaze()
		{
			return enterMaze;
		}

		public string GetForgetMaze()
		{
			return forgetMaze;
		}

		public string GetMazeMove()
		{
			return mazeMove;
		}

		public string GetExitMaze()
		{
			return exitMaze;
		}

		public async Task<OpenAPIResponse<string>> ForgetMazeAsync()
		{
			var response = await _client.DeleteAsync(forgetMaze);
			return new OpenAPIResponse<string>
			{
				IsSuccess = response.IsSuccessStatusCode,
				Code = (int)response.StatusCode,
				Message = response.StatusCode.ToString(),
				Data = await response.Content.ReadAsStringAsync()
			};
		}

		/// validation
		public async Task<OpenAPIResponse<string>> RegisterAsync(string input)
		{

			string endUrl = $"{register}?name={input}"; // Register with the given name.
			HttpResponseMessage response = await _client.PostAsync(endUrl, null); 
			return new OpenAPIResponse<string>
			{
				IsSuccess = response.IsSuccessStatusCode,
				Code = (int)response.StatusCode,
				Message = response.StatusCode.ToString(),
				Data = await response.Content.ReadAsStringAsync()
			};
		}

		public async Task<OpenAPIResponse<List<Mazes>>> GetMazesAsync()
		{
			var response = await _client.GetAsync($"{_apiUrl}{getMazes}");
			var data = JsonConvert.DeserializeObject<List<Mazes>>(await response.Content.ReadAsStringAsync());

			return new OpenAPIResponse<List<Mazes>>
			{
				IsSuccess = response.IsSuccessStatusCode,
				Code = (int)response.StatusCode,
				Message = response.StatusCode.ToString(),
				Data = data
			};
		}

		public async Task<OpenAPIResponse<EnterMazeResponse>> EnterMazeAsync(string selectedMazeName)
		{
			// HttpResponseMessage enterMazeResponse = await client.PostAsync(_openAPIService.GetApiUrl() + _openAPIService.GetEnterMaze() + "?mazeName=" + selectedMazeName, null); // Enter the selected maze.

			var response = await _client.PostAsync($"{_apiUrl}{enterMaze}?mazeName={selectedMazeName}", null);
			var data = JsonConvert.DeserializeObject<EnterMazeResponse>(await response.Content.ReadAsStringAsync());

			return new OpenAPIResponse<EnterMazeResponse>
			{
				IsSuccess = response.IsSuccessStatusCode,
				Code = (int)response.StatusCode,
				Message = response.StatusCode.ToString(),
				Data = data
			};
		}

		public async Task<OpenAPIResponse<EnterMazeResponse>> MazeMoveAsync(string direction)
		{
			var response = await _client.PostAsync($"{_apiUrl}{mazeMove}?direction={direction}", null);
			var data = JsonConvert.DeserializeObject<EnterMazeResponse>(await response.Content.ReadAsStringAsync());

			return new OpenAPIResponse<EnterMazeResponse>
			{
				IsSuccess = response.IsSuccessStatusCode,
				Code = (int)response.StatusCode,
				Message = response.StatusCode.ToString(),
				Data = data
			};
		}

		public async Task<OpenAPIResponse<string>> ExitMazeAsync()
		{
			var response = await _client.PostAsync($"{_apiUrl}{exitMaze}", null);
			var data = JsonConvert.DeserializeObject<EnterMazeResponse>(await response.Content.ReadAsStringAsync());

			return new OpenAPIResponse<string>
			{
				IsSuccess = response.IsSuccessStatusCode,
				Code = (int)response.StatusCode,
				Message = response.StatusCode.ToString(),
				Data = await response.Content.ReadAsStringAsync()
			};
		}

		public async Task<OpenAPIResponse<EnterMazeResponse>> CollectScoreAsync()
		{
			var response = await _client.PostAsync($"{_apiUrl}{collectScore}", null);
			var data = JsonConvert.DeserializeObject<EnterMazeResponse>(await response.Content.ReadAsStringAsync());

			return new OpenAPIResponse<EnterMazeResponse>
			{
				IsSuccess = response.IsSuccessStatusCode,
				Code = (int)response.StatusCode,
				Message = response.StatusCode.ToString(),
				Data = data
			};
		}
	}

}
