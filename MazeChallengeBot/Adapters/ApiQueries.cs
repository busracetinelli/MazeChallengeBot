namespace MazeChallengeBot.Adapters
{
	public class ApiQueries
	{
		private string apiUrl = "https://maze.hightechict.nl";
		private string registerEndpoint = "/api/player/register";
		private string getMazes = "/api/mazes/all";
		private string enterMaze = "/api/mazes/enter";
		public string forgetMaze = "/api/player/forget";
		private string mazeMove = "/api/maze/move";
		private string exitMaze = "/api/maze/exit";
		private string token = "HTI Thanks You [nVM]";

		public string GetApiUrl()
		{
			return apiUrl;
		}

		public string GetRegisterEndpoint()
		{
			return registerEndpoint;
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
		public string GetToken()
		{
			return token;
		}
	}
}
