﻿namespace QuizLib;

internal class User
{
    public int Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public DateTime Birthday { get; set; }
}
