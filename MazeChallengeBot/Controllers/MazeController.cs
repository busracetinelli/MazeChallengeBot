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
		ApiQueries _apiQuery;
		private readonly DirectionHelper _directionHelper; // Create a DirectionHelper object to use its methods.
		// Create a constructor to initialize the ApiQueries and DirectionHelper classes.
		public MazeController()
		{
			_apiQuery = new ApiQueries();
			_directionHelper = new DirectionHelper();
		}

		private int totalScore = 0; // Create an integer variable to store the total score.
		private HashSet<string> visitedMazes = new HashSet<string>(); // Create a collection to store visited mazes.

		[HttpPost]
		public async Task<IActionResult> MazeBot(string input)
		{
			using (HttpClient client = new HttpClient())
			{
				client.BaseAddress = new Uri(_apiQuery.GetApiUrl()); // Set the base address.
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiQuery.GetToken()); // Set the token.

				try
				{
					HttpResponseMessage responseForget = await client.DeleteAsync(_apiQuery.GetForgetMaze()); // Log out of the previous session.
					if (responseForget.IsSuccessStatusCode) // If the logout is successful, continue.
					{
						string responseForgetBody = await responseForget.Content.ReadAsStringAsync(); 
						Console.WriteLine("Forget Success" + (responseForget.StatusCode)); // Log out of the previous session.

						string endUrl = $"{_apiQuery.GetRegisterEndpoint()}?name={input}"; // Register with the given name.
						HttpResponseMessage response = await client.PostAsync(endUrl, null); 

						if (response.IsSuccessStatusCode) // If the registration is successful, continue.
						{
							string responseBody = await response.Content.ReadAsStringAsync(); 
							Console.WriteLine("Register Success" + (response.StatusCode)); // If the registration is successful, continue.

							while (true) // Continue until all mazes are completed.
							{
								HttpResponseMessage mazesResponse = await client.GetAsync(_apiQuery.GetApiUrl() + _apiQuery.GetMazes()); // Get all mazes.

								if (mazesResponse.IsSuccessStatusCode) // If the mazes are successfully received, continue.
								{
									string mazesResponseBody = await mazesResponse.Content.ReadAsStringAsync(); // If the mazes are successfully received, continue.
									List<Mazes> mazes = JsonConvert.DeserializeObject<List<Mazes>>(mazesResponseBody); // Deserialize the received mazes.

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
									Console.WriteLine("GetMazes Success" + (mazesResponse.StatusCode)); // Print the name of the selected maze.

									HttpResponseMessage enterMazeResponse = await client.PostAsync(_apiQuery.GetApiUrl() + _apiQuery.GetEnterMaze() + "?mazeName=" + selectedMazeName, null); // Enter the selected maze.

									if (enterMazeResponse.IsSuccessStatusCode) // If the maze is successfully entered, continue.
									{
										string enterMazeResponseBody = await enterMazeResponse.Content.ReadAsStringAsync(); // If the maze is successfully entered, continue.
										Console.WriteLine("EnterMaze Success" + (enterMazeResponse.StatusCode)); // If the maze is successfully entered, continue.
										EnterMazeResponse responseJson = JsonConvert.DeserializeObject<EnterMazeResponse>(enterMazeResponseBody); // Deserialize the received maze.
										string firstDirection = responseJson.possibleMoveActions[0].direction; // Select the first direction to move.
										Console.WriteLine("Direction: " + firstDirection); // Print the first direction to move.
										bool exitFound = false; // Create a boolean variable to check if the exit is found.
										string shortestPath = ""; // Create a string variable to store the shortest path.
										int moveCount = 0; // Create an integer variable to store the number of moves.

										while (!exitFound) // Continue until the exit is found.
										{ 
											HttpResponseMessage mazeMoveResponse = await client.PostAsync(_apiQuery.GetApiUrl() + _apiQuery.GetMazeMove() + "?direction=" + firstDirection, null); // Move in the selected direction.

											if (mazeMoveResponse.IsSuccessStatusCode) // If the move is successful, continue.
											{
												string mazeMoveResponseBody = await mazeMoveResponse.Content.ReadAsStringAsync(); // If the move is successful, continue.
												Console.WriteLine("MazeMove Success" + (mazeMoveResponse.StatusCode)); // If the move is successful, continue.
												EnterMazeResponse nextResponse = JsonConvert.DeserializeObject<EnterMazeResponse>(mazeMoveResponseBody); // Deserialize the received maze.

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
															HttpResponseMessage exitMazeResponse = await client.PostAsync(_apiQuery.GetApiUrl() + _apiQuery.GetExitMaze(), null); // Exit the maze.
															// If the exit is successful, continue.
															if (exitMazeResponse.IsSuccessStatusCode) 
															{
																string exitMazeResponseBody = await exitMazeResponse.Content.ReadAsStringAsync(); 
																Console.WriteLine("ExitMaze Success" + (exitMazeResponse.StatusCode)); 
															}
															else
															{
																Console.WriteLine("ExitMaze Error: " + exitMazeResponse.StatusCode); // If the exit is not successful, print the error.
															}

															if (nextResponse.canCollectScoreHere) // If there is a score to collect, collect it.
															{
																totalScore += nextResponse.currentScoreInHand; // Add the score to the total score.
																Console.WriteLine("Collected Score: " + nextResponse.currentScoreInHand); // Print the collected score.
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
												Console.WriteLine("MazeMove Error: " + mazeMoveResponse.StatusCode); // If the move is not successful, print the error.
												return StatusCode((int)mazeMoveResponse.StatusCode); // If the move is not successful, return the error.
											}
										}
										Console.WriteLine("Exit found in " + moveCount + " moves."); // Print the number of moves.
									}
									else
									{
										Console.WriteLine("EnterMaze Error: " + enterMazeResponse.StatusCode); // If the maze is not successfully entered, print the error.
										return StatusCode((int)enterMazeResponse.StatusCode); // If the maze is not successfully entered, return the error.
									}
								}
								else
								{
									Console.WriteLine("GetMazes Error: " + mazesResponse.StatusCode); // If the mazes are not successfully received, print the error.
									return StatusCode((int)mazesResponse.StatusCode); // If the mazes are not successfully received, return the error.
								}
							}
						}
						else
						{
							Console.WriteLine("Register Error: " + response.StatusCode); // If the registration is not successful, print the error.
							return StatusCode((int)response.StatusCode); // If the registration is not successful, return the error.
						}
					}
					else
					{
						Console.WriteLine("Delete Error: " + responseForget.StatusCode); // If the logout is not successful, print the error.
						return StatusCode((int)responseForget.StatusCode); // If the logout is not successful, return the error.
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
				
	}
}
