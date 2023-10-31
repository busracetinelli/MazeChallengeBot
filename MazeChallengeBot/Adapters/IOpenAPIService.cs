using MazeChallengeBot.Entities;

namespace MazeChallengeBot.Adapters
{
	public interface IOpenAPIService
	{
		public Task<OpenAPIResponse<string>> ForgetMazeAsync();
		public Task<OpenAPIResponse<string>> RegisterAsync(string input);
		public Task<OpenAPIResponse<List<Mazes>>> GetMazesAsync();
		public Task<OpenAPIResponse<EnterMazeResponse>> EnterMazeAsync(string selectedMazeName);
		public Task<OpenAPIResponse<EnterMazeResponse>> MazeMoveAsync(string direction);
		public Task<OpenAPIResponse<string>> ExitMazeAsync();
		public Task<OpenAPIResponse<EnterMazeResponse>> CollectScoreAsync();
	}
}
