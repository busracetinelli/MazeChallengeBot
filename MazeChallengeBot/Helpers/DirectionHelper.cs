namespace MazeChallengeBot.Helpers
{
	public class DirectionHelper
	{
		public string GetDirectionCode(string nextDirection)
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
