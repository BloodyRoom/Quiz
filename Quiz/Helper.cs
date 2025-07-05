namespace Quiz;

static internal class Helper
{
    static public bool IsNumber(this string numberStr)
    {
        if (numberStr == string.Empty) return false;

        foreach (var sym in numberStr)
        {
            if (sym < '0' || sym > '9')
            {
                return false;
            }
        }
        return true;
    }
}
