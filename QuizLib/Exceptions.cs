namespace QuizLib;

public class UserNotLogined : Exception
{
    public UserNotLogined() : base() { }
    public UserNotLogined(string? message) : base(message) { }
}

public class UserAlreadyExist : Exception
{
    public UserAlreadyExist() : base() { }
    public UserAlreadyExist(string? message) : base(message) { }
}

public class UserNotFound : Exception
{
    public UserNotFound() : base() { }
    public UserNotFound(string? message) : base(message) { }
}

public class IncorectPassword : Exception
{
    public IncorectPassword() : base() { }
    public IncorectPassword(string? message) : base(message) { }
}