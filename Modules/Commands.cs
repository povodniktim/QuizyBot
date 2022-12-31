using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using System;
using Discord;
using Discord.Addons.Interactive;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace DIscordBotTest.Modules
{   
    //You can also add separately the commands you want on the bot. To do this,in addition to the main program code, we create a new class that we will call 'Commands'. To add commands so that the bot responds to them, do as I did below.
    public class Commands : InteractiveBase
    {
        [Command("quiz", RunMode = RunMode.Async)]
        public async Task Quiz()
        {
            // Initialize the score to 0
            int score = 0;
            // Initialize the number of questions to 5
            int numberOfQuestions = 5;
            // Initialize the current question number to 1
            int currentQuestionNumber = 1;

            for (int i = 0; i < numberOfQuestions; i++)
            {
                // Create an HTTP client to send a request to the API
                using (var client = new HttpClient())
                {
                    // Send a request to the API to get 10 trivia questions in the category "Science: Computers" with medium difficulty
                    var APIresponse =
                        await client.GetAsync(
                            "https://opentdb.com/api.php?amount=10&category=18&difficulty=medium&type=multiple");

                    // Read the response as a JSON object
                    var json = await APIresponse.Content.ReadAsStringAsync();
                    dynamic data = JsonConvert.DeserializeObject(json);

                    // Get the question and answer from the response
                    string question = data.results[0].question;
                    string correctAnswer = data.results[0].correct_answer;

                    // Remove the "&quot;" symbol from the question using a regular expression
                    question = System.Text.RegularExpressions.Regex.Replace(question, "&quot;", "'");
                    question = System.Text.RegularExpressions.Regex.Replace(question, "&#039;", "'");
                    question = System.Text.RegularExpressions.Regex.Replace(question, "039", "'");

                    // Get the incorrect answers from the response
                    List<string> incorrectAnswers = data.results[0].incorrect_answers.ToObject<List<string>>();

                    // Add the correct answer to the list of incorrect answers
                    incorrectAnswers.Add(correctAnswer);

                    // Shuffle the list of answers
                    incorrectAnswers = incorrectAnswers.OrderBy(x => Guid.NewGuid()).ToList();

                    // Create the options
                    List<string> options = incorrectAnswers;

                    // Create the embed message with the question and options
                    var embed = new EmbedBuilder()
                        .WithTitle($"(Question {currentQuestionNumber}/{numberOfQuestions})\n{question}")
                        .WithColor(new Color(0x00ddff))
                        .WithDescription(string.Join("\n", options.Select((o, i) => $"{EmojiForNumber(i + 1)} {o}\n")));

                    // Send the question and options to the user
                    var questionMessage = await Context.Channel.SendMessageAsync(embed: embed.Build());

                    // Create a task to wait for the user's response
                    var response = await NextMessageAsync();

                    // Wait for either the response or the timer to complete
                    if (response != null)
                    {
                        // Check if the user's response was the option number
                        if (int.TryParse(response.Content, out int optionNumber))
                        {
                            // Get the option corresponding to the number
                            string selectedOption = options[optionNumber - 1];

                            // Check if the user's response was the correct answer
                            if (selectedOption.ToLower() == correctAnswer.ToLower())
                            {
                                score++;
                                await ReplyAsync("Correct!");
                            }
                            else
                            {
                                await ReplyAsync("Incorrect. The correct answer was " + correctAnswer);
                            }
                        }
                        else
                        {
                            if (response.Content.ToLower() == correctAnswer.ToLower())
                            {
                                score++;
                                await ReplyAsync("Correct!");
                            }
                            else
                            {
                                await ReplyAsync("Incorrect. The correct answer was " + correctAnswer);
                            }
                        }
                    }
                    else
                    {
                        // No response
                        await ReplyAsync("Time's up! The correct answer was " + correctAnswer);
                        break;
                    }
                    // Increment the current question number
                    currentQuestionNumber++;
                }
            }
            // At the end of the game, create an embed message to display the score
            var scoreEmbed = new EmbedBuilder()
                .WithTitle("Quiz Results")
                .WithColor(new Color(0x00ddff))
                .AddField("Score", $"{score}/{numberOfQuestions} :trophy:")
                .Build();

            // Send the embed message with the score to the user
            await ReplyAsync(embed: scoreEmbed);
        }

        static IEmote EmojiForNumber(int number)
        {
            switch (number)
            {
                case 1: return new Emoji("1️⃣");
                case 2: return new Emoji("2️⃣");
                case 3: return new Emoji("3️⃣");
                case 4: return new Emoji("4️⃣");
                // Add more cases for more options
                default: return new Emoji("❔");
            }
        }
    }
}
