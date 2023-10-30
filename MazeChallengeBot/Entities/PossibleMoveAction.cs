namespace MazeChallengeBot.Entities
{
	public class PossibleMoveAction
	{
		public string direction { get; set; }
		public bool isStart { get; set; }
		public bool allowsExit { get; set; }
		public bool allowsScoreCollection { get; set; }
		public bool hasBeenVisited { get; set; }
		public int rewardOnDestination { get; set; }
		public object tagOnTile { get; set; }
	}
}
