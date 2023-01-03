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
    public class Commands : InteractiveBase
    {
        [Command("quiz", RunMode = RunMode.Async)]
        public async Task Quiz()
        {
            int score = 0;
            int numberOfQuestions = 5;
            int currentQuestionNumber = 1;

            for (int i = 0; i < numberOfQuestions; i++)
            {
                using (var client = new HttpClient())
                {
                    // Send a request to the API to get 10 trivia questions in the category "Science: Computers" with medium difficulty
                    var APIresponse =
                        await client.GetAsync(
                            "https://opentdb.com/api.php?amount=10&category=18&difficulty=medium&type=multiple");

                    var json = await APIresponse.Content.ReadAsStringAsync();
                    dynamic data = JsonConvert.DeserializeObject(json);

                    string question = data.results[0].question;
                    string correctAnswer = data.results[0].correct_answer;

                    question = System.Text.RegularExpressions.Regex.Replace(question, "&quot;", "'");
                    question = System.Text.RegularExpressions.Regex.Replace(question, "&#039;", "'");
                    question = System.Text.RegularExpressions.Regex.Replace(question, "039", "'");

                    List<string> incorrectAnswers = data.results[0].incorrect_answers.ToObject<List<string>>();

                    incorrectAnswers.Add(correctAnswer);

                    incorrectAnswers = incorrectAnswers.OrderBy(x => Guid.NewGuid()).ToList();

                    List<string> options = incorrectAnswers;

                    var embed = new EmbedBuilder()
                        .WithTitle($"(Question {currentQuestionNumber}/{numberOfQuestions})\n{question}")
                        .WithColor(new Color(0x00ddff))
                        .WithDescription(string.Join("\n", options.Select((o, i) => $"{EmojiForNumber(i + 1)} {o}\n")));

                    var questionMessage = await Context.Channel.SendMessageAsync(embed: embed.Build());

                    var response = await NextMessageAsync();

                    if (response != null)
                    {
                        // Check if the user's response was the option number
                        if (int.TryParse(response.Content, out int optionNumber))
                        {
                            // Get the option corresponding to the number
                            string selectedOption = options[optionNumber - 1];

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

            var scoreEmbed = new EmbedBuilder()
                .WithTitle("Quiz Results")
                .WithColor(new Color(0x00ddff))
                .AddField("Score", $"{score}/{numberOfQuestions} :trophy:")
                .Build();

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
                default: return new Emoji("❔");
            }
        }
    }
}
