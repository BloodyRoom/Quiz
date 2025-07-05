using QuizLib;
using System;
using System.Reflection;
using System.Text;

namespace AdminUtility;

internal class Program
{
    static QuizService quizService = new QuizService();

    static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        Console.Clear();
        List<string> errors = new();

        while (true)
        {
            VisualHelper.PrintTitle("Авторизація");
            VisualHelper.PrintMenu(null, ["Введіть логін і пароль", "(лише для доступу до редактора)"], errors, null);
            errors.Clear();

            Console.Write("|   Логін: ");
            string login = Console.ReadLine() ?? "";

            Console.Write("|   Пароль: ");
            string password = Console.ReadLine() ?? "";

            try
            {
                if (!quizService.AdminLogin(login, password))
                {
                    errors.Add("Невірний пароль");
                    Console.Clear();
                    continue;
                }
                break;
            }
            catch
            {
                errors.Add("Користувача не знайдено");
                Console.Clear();
            }
        }

        string action = "-";
        List<string> success = new List<string>();

        do
        {
            Console.Clear();
            VisualHelper.PrintTitle("Редактор Вікторин");
            VisualHelper.PrintMenu(["Створити вікторину", "Редагувати вікторину", "Видалити вікторину", "Список вікторин", "Вихід"], null, errors, success);
            errors.Clear();
            success.Clear();

            Console.Write("|   Вибір: ");
            action = Console.ReadLine() ?? "-";

            switch (action)
            {
                case "1":
                    if (await CreateQuizMenu())
                    {
                        success.Add("Вікторину створенно");
                    }
                    break;
                case "2":
                    await EditQuizMenu();
                    break;
                case "3":
                    await DeleteQuizMenu();
                    break;
                case "4":
                    ShowQuizList();
                    Console.WriteLine("|   Натисніть Enter щоб продовжити...");
                    Console.ReadLine();
                    break;
                case "5":
                    break;
                default:
                    errors.Add("Невірний пункт меню");
                    break;
            }
        }
        while (action != "5");
    }

    // допоміг copilot
    static async Task<bool> CreateQuizMenu()
    {
        Console.Clear();
        List<Question> questions = new();

        VisualHelper.PrintTitle("Створення нової вікторини");

        Console.Write("|   Назва вікторини: ");
        string title = Console.ReadLine() ?? "";

        while (true)
        {
            Console.Clear();
            VisualHelper.PrintTitle($"Питань: {questions.Count} — Додати нове питання");
            Console.Write("|   Текст питання (0 щоб завершити): ");
            string text = Console.ReadLine() ?? "";
            if (text == "0") break;

            List<string> options = new();
            for (int i = 0; i < 4; i++)
            {
                Console.Write($"|   Варіант {i + 1}: ");
                options.Add(Console.ReadLine() ?? "");
            }

            Console.Write("|   Введіть номери правильних відповідей через кому (1-4): ");
            string correct = Console.ReadLine() ?? "";
            var corrects = correct.Split(',').Select(x => int.Parse(x.Trim())).ToList();

            questions.Add(new Question
            {
                QuestionText = text,
                AnswerOptions = options,
                CorrectAnswers = corrects
            });
        }

        await quizService.CreateQuiz(title, questions);
        return true;
    }

    static void ShowQuizList()
    {
        Console.Clear();
        VisualHelper.PrintTitle("Список вікторин");
        for (int i = 0; i < quizService.Quizzes.Count; i++)
        {
            Console.WriteLine($"|   {i + 1}. {quizService.Quizzes[i].Title}");
        }
        Console.WriteLine("|");
    }

    static async Task EditQuizMenu()
    {
        ShowQuizList();
        Console.Write("|   Виберіть номер вікторини для редагування: ");

        string index = Console.ReadLine() ?? "";

        if (index.IsNumber())
        {
            int indexInt = int.Parse(index);
            if (indexInt < 1 || indexInt > quizService.Quizzes.Count)
            {
                Console.WriteLine("|   Невірний номер. Enter...");
                Console.ReadLine();
                return;
            }

            var quiz = quizService.Quizzes[indexInt - 1];
            string action = "";

            do
            {
                Console.Clear();
                VisualHelper.PrintTitle($"Редагування: {quiz.Title}");
                VisualHelper.PrintMenu(["Змінити назву", "Редагувати питання", "Повернутись"], null, null, null);

                Console.Write("|   Вибір: ");
                action = Console.ReadLine() ?? "";

                if (action == "1")
                {
                    Console.Write("|   Нова назва: ");
                    quiz.Title = Console.ReadLine() ?? quiz.Title;
                }
                else if (action == "2")
                {
                    for (int i = 0; i < quiz.Questions.Count; i++)
                    {
                        var q = quiz.Questions[i];
                        Console.Clear();
                        VisualHelper.PrintTitle($"Питання {i + 1}: {q.QuestionText}");
                        Console.WriteLine("|   1. Редагувати");
                        Console.WriteLine("|   2. Видалити");
                        Console.WriteLine("|   Enter — пропустити");
                        Console.Write("|   Вибір: ");
                        string qAction = Console.ReadLine() ?? "";
                        if (qAction == "1")
                        {
                            Console.Write("|   Новий текст: ");
                            q.QuestionText = Console.ReadLine() ?? q.QuestionText;
                        }
                        else if (qAction == "2")
                        {
                            quiz.Questions.RemoveAt(i);
                            i--;
                        }
                    }
                }
            }
            while (action != "3");


            await quizService.UpdateQuiz(quiz);
        }
    }

    static async Task DeleteQuizMenu()
    {
        ShowQuizList();
        Console.Write("|   Виберіть номер для видалення: ");

        string index = Console.ReadLine() ?? "";

        if (index.IsNumber())
        {
            int indexInt = int.Parse(index);
            if (indexInt < 1 || indexInt > quizService.Quizzes.Count)
            {
                Console.WriteLine("|   Невірний номер. Enter...");
                Console.ReadLine();
                return;
            }
        
            Console.Write("|   Ви впевнені? (y/n): ");
            if (Console.ReadLine()?.ToLower() == "y")
            {
                await quizService.RemoveQuiz(quizService.Quizzes[indexInt - 1].Id);
                Console.WriteLine("|   Вікторину видалено.");
                Console.ReadLine();
            }
        }

    }
}
