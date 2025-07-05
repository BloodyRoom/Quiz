using Newtonsoft.Json;
using System.Linq;

namespace QuizLib;
public class QuizService
{
    private User LoginedUser { get; set; } = new User();
    public bool IsLogined { get; private set; } = false;
    public List<Quiz> Quizzes { get; private set; } = new List<Quiz>();
    private List<QuizResult> QuizResults { get; set; } = new List<QuizResult>();


    private string usersPath { get; set; } = string.Empty;
    private string adminsPath { get; set; } = string.Empty;
    private string quizzesPath { get; set; } = string.Empty;
    private string resultsPath { get; set; } = string.Empty;


    public QuizService()
    {
        string folderPath = @"C:\Files\";

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        usersPath = $"{folderPath}Users.json";
        adminsPath = $"{folderPath}Admins.json";
        quizzesPath = $"{folderPath}Quizzes.json";
        resultsPath = $"{folderPath}QuizResults.json";

        if (!File.Exists(usersPath)) File.WriteAllText(usersPath, "[]");
        if (!File.Exists(adminsPath)) File.WriteAllText(adminsPath, "[]");
        if (!File.Exists(quizzesPath)) File.WriteAllText(quizzesPath, "[]");
        if (!File.Exists(resultsPath)) File.WriteAllText(resultsPath, "[]");

        Quizzes = JsonConvert.DeserializeObject<List<Quiz>>(File.ReadAllText(quizzesPath)) ?? new List<Quiz>();
        QuizResults = JsonConvert.DeserializeObject<List<QuizResult>>(File.ReadAllText(resultsPath)) ?? new List<QuizResult>();
    }

    private async Task SaveAll()
    {
        await File.WriteAllTextAsync(quizzesPath, JsonConvert.SerializeObject(Quizzes));
        await File.WriteAllTextAsync(resultsPath, JsonConvert.SerializeObject(QuizResults));
    }

    public string? getLoginById(int id)
    {
        string usersJSON = File.ReadAllText(usersPath);
        User[] users = JsonConvert.DeserializeObject<User[]>(usersJSON) ?? [];

        foreach (var user in users)
        {
            if (user.Id == id) return user.Login;
        }

        return null;
    }


    public bool ContainsUser(string login)
    {
        string usersJSON = File.ReadAllText(usersPath);
        User[] users = JsonConvert.DeserializeObject<User[]>(usersJSON) ?? [];

        foreach (var user in users)
        {
            if (user.Login == login) return true;
        }

        return false;
    }

    private User? findUser(string login)
    {
        string usersJSON = File.ReadAllText(usersPath);
        User[] users = JsonConvert.DeserializeObject<User[]>(usersJSON) ?? [];

        foreach (var user in users)
        {
            if (user.Login == login) return user;
        }

        return null;
    }

    private User? findAdminUser(string login)
    {
        string usersJSON = File.ReadAllText(adminsPath);
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
        else if (user.Password != password) return false;

        LoginedUser = user;
        IsLogined = true;

        return true;
    }

    public bool AdminLogin(string login, string password)
    {
        User? user = findAdminUser(login);

        if (user == null) throw new UserNotFound();
        else if (user.Password != password) return false;

        IsLogined = true;

        return true;
    }

    public async Task<bool> Register(string login, string password, DateTime birthday)
    {
        if (ContainsUser(login)) throw new UserAlreadyExist();

        string usersJSON = File.ReadAllText(usersPath);
        List<User> users = JsonConvert.DeserializeObject<List<User>>(usersJSON) ?? [];

        int id = users.Count > 0 ? users.Last().Id + 1 : 1;
        await File.WriteAllTextAsync(usersPath, JsonConvert.SerializeObject(users.Append<User>(new User() { Id = id, Login = login, Password = password, Birthday = birthday })));

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
                    await SaveAll();
                    return QuizResult.Results.OrderByDescending(result => result.Score).ToList().FindIndex(r => r.UserId == LoginedUser.Id);
                }
                else
                {
                    QuizResult.Results.Add(new UserResult() { UserId = LoginedUser.Id, Score = score });
                    await SaveAll();
                    return QuizResult.Results.OrderByDescending(result => result.Score).ToList().FindIndex(r => r.UserId == LoginedUser.Id);
                }
            }
        }

        QuizResults.Add(new QuizResult() { QuizId = quizId, Results = new List<UserResult>() {new UserResult() { UserId = LoginedUser.Id, Score = score }} });

        await SaveAll();
        return 0;
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

    public async Task EditUserBirthday(DateTime newBirthday)
    {
        string usersJSON = File.ReadAllText(usersPath);
        List<User> users = JsonConvert.DeserializeObject<List<User>>(usersJSON) ?? [];

        var user = users.FirstOrDefault(u => u.Id == LoginedUser.Id) ?? new User();

        user.Birthday = newBirthday;
        LoginedUser.Birthday = newBirthday;

        await File.WriteAllTextAsync(usersPath, JsonConvert.SerializeObject(users));
    }

    public async Task<bool> EditUserPassword(string newPassword)
    {
        string usersJSON = File.ReadAllText(usersPath);
        List<User> users = JsonConvert.DeserializeObject<List<User>>(usersJSON) ?? [];

        var user = users.FirstOrDefault(u => u.Id == LoginedUser.Id) ?? new User();

        user.Password = newPassword;
        LoginedUser.Password = newPassword;

        await File.WriteAllTextAsync(usersPath, JsonConvert.SerializeObject(users));
        return true;
    }

    public async Task CreateQuiz(string title, List<Question> questions)
    {
        int newId = Quizzes.Count > 0 ? Quizzes.Last().Id + 1 : 1;
        var newQuiz = new Quiz() { Id = newId, Title = title, Questions = questions };

        Quizzes.Add(newQuiz);

        await SaveAll();
    }

    public async Task RemoveQuiz(int quizId)
    {
        var quiz = Quizzes.FirstOrDefault(q => q.Id == quizId);
        if (quiz != null)
        {
            Quizzes.Remove(quiz);
        }

        var qr = QuizResults.FirstOrDefault(r => r.QuizId == quizId);
        if (qr != null)
        {
            QuizResults.Remove(qr);
        }

        await SaveAll();
    }

    public async Task<bool> UpdateQuiz(Quiz updatedQuiz)
    {
        var index = Quizzes.FindIndex(q => q.Id == updatedQuiz.Id);
        if (index == -1) return false;

        Quizzes[index] = updatedQuiz;
        await SaveAll();
        return true;
    }
}
