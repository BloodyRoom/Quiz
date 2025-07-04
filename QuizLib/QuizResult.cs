namespace QuizLib;

public class QuizResult
{
    public int QuizId { get; set; }
    public List<UserResult> Results { get; set; } = new List<UserResult>();
}
