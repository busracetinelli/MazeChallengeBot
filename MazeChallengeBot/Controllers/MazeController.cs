using MazeChallengeBot.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using MazeChallengeBot.Adapters;
using Newtonsoft.Json;
using MazeChallengeBot.Helpers;

namespace MazeChallengeBot.Controllers
{

	[ApiController]
	[Route("[controller]")]
	public class MazeController : ControllerBase
	{
		IOpenAPIService _openAPIService;
		private readonly DirectionHelper _directionHelper; // Create a DirectionHelper object to use its methods.
		public MazeController(IOpenAPIService openAPIService)
		{
			_openAPIService = openAPIService; // Assign the OpenAPIService object to the _openAPIService variable.
			_directionHelper = new DirectionHelper(); // Create a DirectionHelper object to use its methods.
		}

		private int totalScore = 0; // Create an integer variable to store the total score.
		private HashSet<string> visitedMazes = new HashSet<string>(); // Create a collection to store visited mazes.

		[HttpPost]
		public async Task<IActionResult> MazeBot(string input)
		{
			using (HttpClient client = new HttpClient())
			{
				try
				{
					var responseForget = await _openAPIService.ForgetMazeAsync();// Log out of the previous session.
					if (responseForget.IsSuccess) // If the logout is successful, continue.
					{
						//string responseForgetBody = await responseForget.Content.ReadAsStringAsync(); 
						Console.WriteLine("Forget Success" + (responseForget.Code)); // Log out of the previous session.

						//string endUrl = $"{_openAPIService.GetRegisterEndpoint()}?name={input}"; // Register with the given name.
						//HttpResponseMessage response = await client.PostAsync(endUrl, null); 

						var register = await _openAPIService.RegisterAsync(input); // Register with the given name.

						if (register.IsSuccess) // If the registration is successful, continue.
						{
							string responseBody = register.Data ?? ""; // If the registration is successful, continue.
							Console.WriteLine("Register Success" + (register.Code)); // If the registration is successful, continue.

							var mazesRes = await _openAPIService.GetMazesAsync(); // Get all mazes.
							if (mazesRes.IsSuccess) // If the mazes are successfully received, continue.
							{
								while (true) // Continue until all mazes are completed.
								{
									List<Mazes> mazes = mazesRes.Data ?? new List<Mazes>(); // Deserialize the received mazes.

									if (mazes.Count == 0) // If all mazes are completed, end the loop.
									{
										break; // If all mazes are completed, end the loop.
									}

									string selectedMazeName = ""; // Select a random maze that has not been visited before.

									do
									{
										Random rnd = new Random();  // Creating a Random object to choose a random index.
										int index = rnd.Next(0, mazes.Count);   // Selecting a random index from the mazes collection.
										selectedMazeName = mazes[index].name;  // Assigning the name of the selected maze to selectedMazeName.
									}
									while (visitedMazes.Contains(selectedMazeName)); // If the selected maze has been visited before, continue to select a maze.

									visitedMazes.Add(selectedMazeName); // Add the selected maze to the visitedMazes collection.

									Console.WriteLine("Selected Maze:" + selectedMazeName); // Print the name of the selected maze.

									var maze = await _openAPIService.EnterMazeAsync(selectedMazeName); // Enter the selected maze.
									if (maze.IsSuccess && maze.Data != null) // If the maze is successfully entered, continue.
									{
										Console.WriteLine("Enter Maze:" + maze.Code); // If the maze is successfully entered, continue.
										var mazeResult = await EnterMazeAsync(maze.Data); // Enter the maze.
										if (mazeResult != null) // If the maze is successfully entered, continue.
										{
											return mazeResult; // If the maze is successfully entered, return the result.
										}
									}
									else
									{
										Console.WriteLine("EnterMaze Error: " + maze.Code); // If the maze is not successfully entered, print the error.
										return StatusCode(maze.Code); // If the maze is not successfully entered, return the error.
									}
								}
							}
							else
							{
								Console.WriteLine("GetMazes Error: " + mazesRes.Code); // If the mazes are not successfully received, print the error.
								return StatusCode(mazesRes.Code); // If the mazes are not successfully received, return the error.
							}
						}
						else
						{
							Console.WriteLine("Register Error: " + register.Code); // If the registration is not successful, print the error.
							return StatusCode(register.Code); // If the registration is not successful, return the error.
						}
					}
					else
					{
						Console.WriteLine("Delete Error: " + responseForget.Code); // If the logout is not successful, print the error.
						return StatusCode(responseForget.Code); // If the logout is not successful, return the error.
					}
				}
				// If an error occurs, print the error.
				catch (HttpRequestException e)
				{
					Console.WriteLine("Error: " + e.Message);
					return BadRequest(e.Message);
				}

				// All operations have been completed, and no IActionResult was returned.
				return Ok("Total Score: " + totalScore); // Return the total score.
			}
		}

		private async Task<StatusCodeResult?> EnterMazeAsync(EnterMazeResponse maze)
		{
			string firstDirection = maze.possibleMoveActions[0].direction; // Select the first direction to move.
			bool exitFound = false; // Create a boolean variable to check if the exit is found.
			string shortestPath = ""; // Create a string variable to store the shortest path.
			int moveCount = 0; // Create an integer variable to store the number of moves.

			while (!exitFound) // Continue until the exit is found.
			{
				var move = await _openAPIService.MazeMoveAsync(firstDirection); // Move in the selected direction.

				if (move.IsSuccess && move.Data != null) // If the move is successful, continue.
				{
					EnterMazeResponse nextResponse = move.Data;

					if (nextResponse.canCollectScoreHere) // If there is a score to collect, collect it.
					{
						var collected = await _openAPIService.CollectScoreAsync(); // Collect the score.
						if (collected.IsSuccess) // If the score is successfully collected, continue.
						{
							int totalScore = collected.Data?.currentScoreInBag ?? 0; // Add the score to the total score.
							Console.WriteLine("Collected Score: " + totalScore); // Print the collected score.

						}
					}
					if (nextResponse.possibleMoveActions.Count > 0) // If there are possible moves, continue.
					{
						List<string> possibleDirections = new List<string>(); // Create a list to store possible directions.
						moveCount++; // Increase the number of moves by 1.
						for (int i = 0; i < nextResponse.possibleMoveActions.Count; i++) // Loop through the possible moves.
						{
							var moveAction = nextResponse.possibleMoveActions[i]; // Get the current move action.
							if (nextResponse.canExitMazeHere) // If the exit is found, end the loop.
							{
								exitFound = true; // If the exit is found, end the loop.
								shortestPath += moveAction.direction; // Add the current direction to the shortest path.
								var exitMaze = await _openAPIService.ExitMazeAsync(); // Exit the maze.

								// If the exit is successful, continue.
								if (exitMaze.IsSuccess && exitMaze.Data != null)
								{
									Console.WriteLine("ExitMaze Success" + (exitMaze.Code));
								}
								else
								{
									Console.WriteLine("ExitMaze Error: " + exitMaze.Code); // If the exit is not successful, print the error.
								}
								if (exitFound)
								{
									return null; // If the exit is found, end the loop.
								}
							}
							else
							{
								possibleDirections.Add(moveAction.direction); // Add the current direction to the possible directions.
							}
						}

						if (!exitFound) // If the exit is not found, select a random direction from the possible directions.
						{
							// If the exit is not found, select a random direction from the possible directions.
							Random rndm = new Random();
							int randomDirectionIndex = rndm.Next(possibleDirections.Count);
							string nextDirection = possibleDirections[randomDirectionIndex];
							shortestPath += _directionHelper.GetDirectionCode(nextDirection);
							firstDirection = nextDirection;
						}
					}
					else
					{
						Console.WriteLine("No possible move actions found."); // If there are no possible moves, print the error.
						break; // If there are no possible moves, end the loop.
					}
				}
				else
				{
					Console.WriteLine("MazeMove Error: " + move.Code); // If the move is not successful, print the error.
					return StatusCode(move.Code); // If the move is not successful, return the error.
				}
			}
			Console.WriteLine("Exit found in " + moveCount + " moves."); // Print the number of moves.

			return null; // If the exit is found, end the loop.
		}
	}
}
