using MazeChallengeBot.Adapters;
using Microsoft.AspNetCore.Mvc;

namespace MazeChallengeBot.Controllers
{
	public class BaseController : Controller
	{
		private static ApiQueries _apiQuery;
		public static ApiQueries ApiQuery
		{
			get
			{
				if (_apiQuery == null)
				{
					_apiQuery = new ApiQueries();
				}
				return _apiQuery;
			}
		}
		private string apiUrl = "https://maze.hightechict.nl";
		private string token = "HTI Thanks You [nVM]";
		
		public string GetApiUrl()
		{
			return apiUrl;
		}
		public string GetToken()
		{
			return token;
		}

		private string getDirectioncode(string nextDirection)
		{
			switch (nextDirection)
			{
				case "Up": return "↑";
				case "Down": return "↓";
				case "Right": return "→";
				case "Left": return "←";
				default: return ".";
			}
		}

	}
}
