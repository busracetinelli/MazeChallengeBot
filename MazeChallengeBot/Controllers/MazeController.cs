using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MazeChallengeBot.Adapters;
using MazeChallengeBot.Entities;
using MazeChallengeBot.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace MazeChallengeBot.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class MazeController : ControllerBase
	{
		private readonly IOpenAPIService _openAPIService;
		private readonly DirectionHelper _directionHelper;
		private int totalScore = 0;
		private int totalScoreInHand = 0;
		private int totalScoreInBag = 0;
		private HashSet<string> visitedMazes = new HashSet<string>();

		public MazeController(IOpenAPIService openAPIService)
		{
			_openAPIService = openAPIService;
			_directionHelper = new DirectionHelper();
		}

		[HttpPost]
		public async Task<IActionResult> MazeBot(string input)
		{
			try
			{
				var responseForget = await _openAPIService.ForgetMazeAsync();
				if (!responseForget.IsSuccess)
					return StatusCode(responseForget.Code);

				var register = await _openAPIService.RegisterAsync(input);
				if (!register.IsSuccess)
					return StatusCode(register.Code);

				while (true)
				{
					var mazesRes = await _openAPIService.GetMazesAsync();
					if (!mazesRes.IsSuccess)
						return StatusCode(mazesRes.Code);

					var mazes = mazesRes.Data ?? new List<Mazes>();
					if (mazes.Count == 0)
						break;

					string selectedMazeName = SelectRandomUnvisitedMaze(mazes);
					visitedMazes.Add(selectedMazeName);

					Console.WriteLine("Selected Maze: " + selectedMazeName);
					var mazeResult = await EnterMazeAndCollectScoreAsync(selectedMazeName);
					if (mazeResult != null)
						return mazeResult;
				}
			}
			catch (HttpRequestException e)
			{
				Console.WriteLine("Error: " + e.Message);
				return BadRequest(e.Message);
			}

			await _openAPIService.CollectScoreAsync();
			return Ok("Total Score: " + totalScore);
		}

		private string SelectRandomUnvisitedMaze(List<Mazes> mazes)
		{
			string selectedMazeName = "";
			do
			{
				Random rnd = new Random();
				int index = rnd.Next(0, mazes.Count);
				selectedMazeName = mazes[index].name;
			}
			while (visitedMazes.Contains(selectedMazeName));
			return selectedMazeName;
		}

		private async Task<StatusCodeResult?> EnterMazeAndCollectScoreAsync(string mazeName)
		{
			var mazeResponse = await _openAPIService.EnterMazeAsync(mazeName);
			if (!mazeResponse.IsSuccess || mazeResponse.Data == null)
				return StatusCode(mazeResponse.Code);

			var maze = mazeResponse.Data;
			string firstDirection = maze.possibleMoveActions[0].direction;
			bool exitFound = false;
			int moveCount = 0;

			while (!exitFound)
			{
				var move = await _openAPIService.MazeMoveAsync(firstDirection);
				if (!move.IsSuccess || move.Data == null)
					return StatusCode(move.Code);

				var nextResponse = move.Data;

				if (nextResponse.currentScoreInHand > 0)
				{
					if (nextResponse.canCollectScoreHere)
					{
						var collected = await _openAPIService.CollectScoreAsync();
						if (collected.IsSuccess)
						{
							int scoreInHand = collected.Data?.currentScoreInBag ?? 0;
							totalScoreInBag += scoreInHand;
							Console.WriteLine("Collected Score in Bag: " + scoreInHand);
						}
					}
				}


				//if (nextResponse.canCollectScoreHere)
				//{
				//	var collected = await _openAPIService.CollectScoreAsync();
				//	if (collected.IsSuccess)
				//	{
				//		totalScoreInHand += nextResponse.currentScoreInHand;
				//		Console.WriteLine("Collected Score in Hand: " + nextResponse.currentScoreInHand);
				//	}
				//}

				if (nextResponse.possibleMoveActions.Count > 0)
				{
					List<string> possibleDirections = new List<string>();
					moveCount++;
					bool exitMazeHere = nextResponse.canExitMazeHere;
					string nextDirection = SelectNextDirection(nextResponse, possibleDirections, exitMazeHere);
					exitFound = exitMazeHere;

					if (!exitFound)
						firstDirection = nextDirection;
				}
				else
				{
					Console.WriteLine("No possible move actions found.");
					break;
				}
			}

			totalScoreInBag += totalScoreInHand;
			totalScore += totalScoreInHand;

			Console.WriteLine("Exit found in " + moveCount + " moves.");
			Console.WriteLine("Total Score: " + totalScoreInHand);
			Console.WriteLine("Total Score in Bag: " + totalScoreInBag);


			var exitMaze = await _openAPIService.ExitMazeAsync();

			if (exitMaze.IsSuccess && exitMaze.Data != null)
			{
				Console.WriteLine("ExitMaze Success" + exitMaze.Code);
				totalScoreInHand = 0;
			}
			else
			{
				Console.WriteLine("ExitMaze Error: " + exitMaze.Code);
				return StatusCode(exitMaze.Code);
			}

			return null;
		}

		private string SelectNextDirection(EnterMazeResponse response, List<string> possibleDirections, bool exitMazeHere)
		{
			foreach (var moveAction in response.possibleMoveActions)
			{
				if (exitMazeHere)
					return moveAction.direction;
				possibleDirections.Add(moveAction.direction);
			}

			Random rnd = new Random();
			int randomDirectionIndex = rnd.Next(possibleDirections.Count);
			return possibleDirections[randomDirectionIndex];
		}
	}
}