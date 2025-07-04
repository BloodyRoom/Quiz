using Newtonsoft.Json;
using System.Linq;

namespace QuizLib;
public class QuizService
{
    private User LoginedUser { get; set; } = new User();
    public bool IsLogined { get; private set; } = false;
    public List<Quiz> Quizzes { get; private set; } = new List<Quiz>();
    private List<QuizResult> QuizResults { get; set; } = new List<QuizResult>();

    public QuizService()
    {
        if (!File.Exists("Users.json")) File.Create("Users.json");
        if (!File.Exists("Quizzes.json")) File.Create("Quizzes.json");
        if (!File.Exists("QuizResults.json")) File.Create("QuizResults.json");

        Quizzes = JsonConvert.DeserializeObject<List<Quiz>>(File.ReadAllText("Quizzes.json")) ?? new List<Quiz>();
        QuizResults = JsonConvert.DeserializeObject<List<QuizResult>>(File.ReadAllText("QuizResults.json")) ?? new List<QuizResult>();
    }

    private async Task SaveAll()
    {
        await File.WriteAllTextAsync("Quizzes.json", JsonConvert.SerializeObject(Quizzes));
        await File.WriteAllTextAsync("QuizResults.json", JsonConvert.SerializeObject(QuizResults));
    }

    public bool ContainsUser(string login)
    {
        string usersJSON = File.ReadAllText("users.json");
        User[] users = JsonConvert.DeserializeObject<User[]>(usersJSON) ?? [];

        foreach (var user in users)
        {
            if (user.Login == login) return true;
        }

        return false;
    }

    private User? findUser(string login)
    {
        string usersJSON = File.ReadAllText("users.json");
        User[] users = JsonConvert.DeserializeObject<User[]>(usersJSON) ?? [];

        foreach (var user in users)
        {
            if (user.Login == login) return user;
        }

        return null;
    }

    public bool Login(string login, string password)
    {
        User? user = findUser(login);

        if (user == null) throw new UserNotFound();
        else if (user.Password != login) return false;

        LoginedUser = user;
        IsLogined = true;

        return true;
    }

    public async Task<bool> Register(string login, string password, DateTime birthday)
    {
        if (ContainsUser(login)) throw new UserAlreadyExist();

        string usersJSON = File.ReadAllText("users.json");
        User[] users = JsonConvert.DeserializeObject<User[]>(usersJSON) ?? [];

        await File.WriteAllTextAsync("users.json", JsonConvert.SerializeObject(users.Append<User>(new User() { Id = users.Last().Id + 1, Login = login, Password = password, Birthday = birthday })));

        return true;
    }

    public string GetLogin()
    {
        if (!IsLogined) throw new UserNotLogined();

        return LoginedUser.Login;
    }

    public UserResult[]? GetTopOfQiuz(int quizId)
    {
        if (QuizResults == null) return null;

        foreach (var QuizResult in QuizResults)
        {
            if (QuizResult.QuizId == quizId)
            {
                return QuizResult.Results.OrderByDescending(result => result.Score).Take(20).ToArray();
            }
        }

        return null;
    }

    public async Task<int?> EndQuiz(int quizId, int score)
    {
        if (QuizResults == null) return null;

        foreach (var QuizResult in QuizResults)
        {
            if (QuizResult.QuizId == quizId)
            {
                var res = QuizResult.Results.FirstOrDefault(user => user.UserId == LoginedUser.Id);

                if (res != null)
                {
                    QuizResult.Results.Remove(res);
                    QuizResult.Results.Add(new UserResult() { UserId = LoginedUser.Id, Score = score });
                    return QuizResult.Results.OrderByDescending(result => result.Score).ToList().IndexOf(new UserResult() { UserId = LoginedUser.Id, Score = score });
                }
                else
                {
                    QuizResult.Results.Add(new UserResult() { UserId = LoginedUser.Id, Score = score });
                    return QuizResult.Results.OrderByDescending(result => result.Score).ToList().FindIndex(r => r.UserId == LoginedUser.Id);
                }
            }
        }

        await SaveAll();
        return null;
    }

    // трохи допоміг copilot
    public QuizResult[]? GetUserResults()
    {
        if (QuizResults == null) return null;

        var resultsForUser = QuizResults.Where(qr => qr.Results.Any(res => res.UserId == LoginedUser.Id)).Select(qr =>
            {
                var userRes = qr.Results.First(r => r.UserId == LoginedUser.Id);

                return new QuizResult{ QuizId = qr.QuizId, Results = new List<UserResult> { userRes }};
            }).ToArray();

        return resultsForUser;
    }

    public async Task CreateQuiz(string title, List<Question> questions)
    {
        int newId = Quizzes.Count > 0 ? Quizzes.Last().Id + 1 : 1;
        var newQuiz = new Quiz() { Id = newId, Title = title, Questions = questions };

        Quizzes.Add(newQuiz);

        await SaveAll();
    }
}
