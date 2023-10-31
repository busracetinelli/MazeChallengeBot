namespace MazeChallengeBot.Entities
{
	public class OpenAPIResponse<T>
	{
		public bool IsSuccess { get; set; }
		public int Code { get; set; }
		public required string Message { get; set; }
		public T? Data { get; set; }
	}
}
