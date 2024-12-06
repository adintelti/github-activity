using System.Text.Json;
using System.Web;

namespace github_activity
{
    internal class Program
    {
        static async Task Main()
        {
            ShowWelcomeMessage();

            while (true)
            {
                ConsoleMessages.PrintCommandMessage("Inform username or type exit to quit: ");
                string input = Console.ReadLine() ?? string.Empty;

                if (string.IsNullOrEmpty(input))
                {
                    RestartCommand();
                    continue;
                }

                List<string> commands = Utility.ParseInput(input);
                if (commands.Count != 1 && !commands[0].Equals("exit"))
                {
                    RestartCommand();
                    continue;
                }

                if (commands[0].Equals("exit"))
                {
                    break;
                }

                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.TryParseAdd("request");

                var username = commands[0];
                if (string.IsNullOrEmpty(username))
                {
                    ConsoleMessages.PrintError("User name not entered! Type <username> to get started.");
                    continue;
                }

                var events = await GetUserActivities(client, username);
                Dictionary<string, int> table = new();
                List<ActivityInfo> infoList = new List<ActivityInfo>();
                if (events.Count != 0)
                {
                    var groups = events.GroupBy(x => x.Type);
                    foreach (var group in groups)
                    {
                        var info = new ActivityInfo();
                        info.Type = group.Key;
                        info.RepoNames = group.Select(x => x.Repo.Name).Distinct().ToList();
                        info.Count = info.RepoNames.Count;
                        infoList.Add(info);
                    }

                    Console.WriteLine("\nOutput:");

                    foreach (var item in infoList)
                    {
                        switch (item.Type)
                        {
                            case "CreateEvent":
                                Console.WriteLine($"Created {item.Count} new repositories called {ReturnNamesInStringForm(item.RepoNames)}");
                                break;

                            case "PushEvent":
                                Console.WriteLine($"Pushed {item.Count} new changes in repositories {ReturnNamesInStringForm(item.RepoNames)}");
                                break;

                            case "PullRequestEvent":
                                Console.WriteLine($"Opened {item.Count} new pull requestes in repositories {ReturnNamesInStringForm(item.RepoNames)}");
                                break;

                            case "IssueCommentEvent":
                                Console.WriteLine($"Added {item.Count} new comments in repositories {ReturnNamesInStringForm(item.RepoNames)}");
                                break;

                            case "IssuesEvent":
                                Console.WriteLine($"Opened {item.Count} new issues in repositories {ReturnNamesInStringForm(item.RepoNames)}");
                                break;

                            case "WatchEvent":
                                Console.WriteLine($"Starred {item.Count} {ReturnNamesInStringForm(item.RepoNames)}");
                                break;

                            default:
                                break;
                        }
                    }
                }
                else
                {
                    ConsoleMessages.PrintError("No meaning - full activities found for this user. Try with some other user");
                    continue;
                }
            }
        }

        static async Task<List<Activity>> GetUserActivities(HttpClient client, string username)
        {
            try
            {
                var encodedUsername = HttpUtility.UrlEncode(username);

                var url = $"https://api.github.com/users/{encodedUsername}/events";
                var httpResponse = await client.GetAsync(url);
                httpResponse.EnsureSuccessStatusCode();

                var content = await httpResponse.Content.ReadAsStringAsync();
                var activities = JsonSerializer.Deserialize<List<Activity>>(content);

                return activities ?? new();
            }
            catch (Exception ex)
            {
                ConsoleMessages.PrintError("There is problem in fetching your git hub activities.");
                throw;
            }
        }

        private static void RestartCommand()
        {
            ConsoleMessages.PrintError("Wrong command! inform a valid <username>.");
        }

        private static void ShowWelcomeMessage()
        {
            ConsoleMessages.PrintInfo("Welcome to Github Activity console");
            ConsoleMessages.PrintInfo("Type <username> to retrieve data.");
        }

        static string ReturnNamesInStringForm(List<string> repoNames)
        {
            if (repoNames.Count == 0)
            {
                return string.Empty;
            }

            if (repoNames.Count == 1)
            {
                return repoNames.FirstOrDefault() ?? string.Empty;
            }

            if (repoNames.Count > 1)
            {
                string names = string.Empty;

                for (int i = 0; i < repoNames.Count - 1; i++)
                {
                    names += repoNames[i] + ", ";
                }

                names += "& " + repoNames[repoNames.Count - 1];
                return names;
            }

            return string.Empty;
        }
    }
}
