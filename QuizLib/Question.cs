namespace QuizLib;

public class Question
{
    public string QuestionText { get; set; } = string.Empty;
    public List<string> AnswerOptions { get; set; } = new List<string>();
    public List<int> CorrectAnswers { get; set; } = new List<int>();
}
