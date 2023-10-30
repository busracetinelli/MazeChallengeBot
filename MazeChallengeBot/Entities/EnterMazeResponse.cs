namespace MazeChallengeBot.Entities
{
	public class EnterMazeResponse
	{
		public List<PossibleMoveAction> possibleMoveActions { get; set; }
		public bool canCollectScoreHere { get; set; }
		public bool canExitMazeHere { get; set; }
		public int currentScoreInHand { get; set; }
		public int currentScoreInBag { get; set; }
		public object tagOnCurrentTile { get; set; }
	}
}
