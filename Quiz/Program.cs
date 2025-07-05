using QuizLib;
using System.Globalization;
using System.Text;

namespace Quiz;

internal class Program
{
    private static QuizService quizService = new QuizService();
    static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        if (!await AuthMenu()) return;

        if (quizService.IsLogined)
        {
            List<string> errors = new List<string>();

            string mainMenuAction = "0";

            do
            {
                Console.Clear();

                VisualHelper.PrintTitle($"Добрий день, {quizService.GetLogin()}");
                VisualHelper.PrintMenu(["Почати вікторину", "Переглянути пройдені вікторини", "Редагувати користувача", "Вихід"], null, errors, null);
                errors.Clear();

                Console.Write("|   Вибір: ");
                mainMenuAction = Console.ReadLine() ?? "0";

                switch (mainMenuAction)
                {
                    case "1":
                        await AllQuizzesMenu();
                        break;
                    case "2":
                        MyResultsMenu();
                        break;
                    case "3":
                        break;
                    case "4":
                        break;
                    default:
                        errors.Add("Невірний пункт меню");
                        break;
                }
            }
            while (mainMenuAction != "4");
        }
    }

    static async Task<bool> AuthMenu()
    {
        string authAction = "0";
        List<string> errors = new List<string>();

        do
        {
            VisualHelper.PrintTitle("Аунтентифікація");
            VisualHelper.PrintMenu(["Вхід", "Реєстрація", "Вихід"], null, errors, null);
            errors.Clear();

            Console.Write("|   Вибір: ");
            authAction = Console.ReadLine() ?? "0";

            string login = string.Empty;
            string password = string.Empty;
            string birthday = string.Empty;

            switch (authAction)
            {
                case "1":
                    Console.Clear();

                    do
                    {
                        VisualHelper.PrintTitle("Вхід");
                        VisualHelper.PrintMenu(null, ["Введіть логін та пароль", "від вашого аккаунту", "(0 щоб повернутися)"], errors, null);
                        errors.Clear();

                        Console.Write("|   Логін: ");
                        login = Console.ReadLine() ?? string.Empty;
                        if (login == "0") break;

                        Console.Write("|   Пароль: ");
                        password = Console.ReadLine() ?? string.Empty;
                        if (password == "0") break;

                        try
                        {
                            bool res = quizService.Login(login, password);
                            if (!res)
                            {
                                errors.Add("Пароль не вірний");
                            }
                            else
                            {
                                return true;
                            }

                        }
                        catch (UserNotFound)
                        {
                            errors.Add("Користувача не знайдено");
                        }
                        catch (Exception ex)
                        {
                            errors.Add("Невідома помилка: " + ex.Message);
                        }
                        
                        Console.Clear();
                    }
                    while (true);

                    break;
                case "2":
                    Console.Clear();

                    do
                    {
                        VisualHelper.PrintTitle("Реєстрація");
                        VisualHelper.PrintMenu(null, ["Введіть логін, пароль", "та дату народження", "(0 щоб повернутися)"], errors, null);
                        errors.Clear();

                        Console.Write("|   Логін: ");
                        login = Console.ReadLine() ?? string.Empty;
                        if (login == "0") break;

                        Console.Write("|   Пароль: ");
                        password = Console.ReadLine() ?? string.Empty;
                        if (password == "0") break;
                        
                        Console.Write("|   Дата народження(дд/мм/рррр): ");
                        birthday = Console.ReadLine() ?? string.Empty;
                        if (birthday == "0") break;

                        DateTime dateTime = DateTime.ParseExact(birthday, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                        try
                        {
                            await quizService.Register(login, password, dateTime);
                            authAction = "1";
                        }
                        catch (UserAlreadyExist)
                        {
                            errors.Add("Користувача вже зареєстрований");
                        }
                        catch (Exception ex)
                        {
                            errors.Add("Невідома помилка: " + ex.Message);
                        }

                        Console.Clear();
                    }
                    while (true);

                    break;
                case "3":
                    return false;
                default:
                    errors.Add("Невірний пункт меню");
                    break;
            }

            Console.Clear();
        }
        while (authAction != "3");
        return false;
    }

    static async Task AllQuizzesMenu()
    {
        Console.Clear();

        string action = "-";
        List<string> errors = new List<string>();

        do
        {
            VisualHelper.PrintTitle("Всі вікторини");
            string[] quizzesTitle = quizService.Quizzes.Select(q => q.Title).ToArray();
            VisualHelper.PrintMenu(quizzesTitle, ["Виберіть вікторину", "з переліку", "(0 для виходу)"], errors, null);
            errors.Clear();

            Console.Write("|   Вибір: ");
            action = Console.ReadLine() ?? "-";
            if (action == "0") return;

            if (action.IsNumber())
            {
                int quiz = int.Parse(action);
                if (quiz >= 1 && quiz <= quizService.Quizzes.Count)
                {
                    var selectedQuiz = quizService.Quizzes[quiz - 1];
                    string quizAction = "0";

                    do
                    {
                        Console.Clear();

                        VisualHelper.PrintTitle(selectedQuiz.Title);
                        VisualHelper.PrintMenu(["Пройти вікторину", "Переглянути ТОП-20", "Назад"], [$"Запитань: {selectedQuiz.Questions.Count}"], errors, null);

                        Console.Write("|   Вибір: ");
                        quizAction = Console.ReadLine() ?? "0";

                        switch (quizAction)
                        {
                            case "1":
                                int score = 0;
                                for (int i = 0; i < selectedQuiz.Questions.Count; i++)
                                {
                                    var q = selectedQuiz.Questions[i];
                                    Console.Clear();
                                    VisualHelper.PrintTitle($"Питання {i + 1}/{selectedQuiz.Questions.Count}");
                                    VisualHelper.PrintMenu(q.AnswerOptions.ToArray(), [q.QuestionText], null, null);

                                    Console.Write("|   Ваша відповідь: ");
                                    string input = Console.ReadLine() ?? "0";
                                    if (input.IsNumber() && q.CorrectAnswers.Contains(int.Parse(input))) score++;
                                }

                                Console.Clear();
                                int? place = await quizService.EndQuiz(selectedQuiz.Id, score);
                                VisualHelper.PrintTitle("Пройдено");
                                VisualHelper.PrintMenu(null, [
                                    $"Ви набрали {score} з {selectedQuiz.Questions.Count} балів!",
                                    $"Ваше місце в рейтингу: {place + 1}", "",
                                    "Нажміть Enter щоб продовжити..."
                                ], null, null);

                                Console.ReadLine();
                                break;

                            case "2":
                                Console.Clear();
                                var top = quizService.GetTopOfQiuz(selectedQuiz.Id);
                                if (top == null || top.Length == 0)
                                {
                                    VisualHelper.PrintTitle("ТОП-20 відсутній");
                                }
                                else
                                {
                                    VisualHelper.PrintTitle("ТОП-20 по вікторині " + selectedQuiz.Title);
                                    Console.WriteLine("|");
                                    for (int i = 0; i < top.Length; i++)
                                    {
                                        Console.WriteLine($"|   {i + 1}. {quizService.getLoginById(top[i].UserId)} - {top[i].Score}");
                                    }
                                }
                                VisualHelper.PrintMenu(null, ["Нажміть Enter щоб повернутися..."], null, null);
                                Console.ReadLine();
                                break;

                            case "3":
                                break;

                            default:
                                errors.Add("Невірний пункт меню");
                                break;
                        }
                    }
                    while (quizAction != "3");
                }
                else
                {
                    errors.Add("Невірний пункт меню");
                }
            }
            else
            {
                errors.Add("Невірний пункт меню");
            }

            Console.Clear();
        }
        while (action != "0");
    }

    static void MyResultsMenu()
    {
        Console.Clear();
        List<string> errors = new();

        var userResults = quizService.GetUserResults();
        if (userResults == null || userResults.Length == 0)
        {
            VisualHelper.PrintTitle("Ви ще не пройшли ні одної вікторини");
            Console.WriteLine("|\n|   Натисніть Enter щоб повернутися... \n|");
            Console.ReadLine();
            return;
        }

        VisualHelper.PrintTitle("Пройдені вікторини");
        Console.WriteLine("|");
        foreach (var result in userResults)
        {
            var quiz = quizService.Quizzes.FirstOrDefault(q => q.Id == result.QuizId);
            if (quiz != null)
            {
                Console.WriteLine($"|   {quiz.Title} — {result.Results[0].Score}");
            }
        }

        Console.WriteLine("|\n|   Натисніть Enter щоб повернутися... \n|");
        Console.ReadLine();
    }

}
