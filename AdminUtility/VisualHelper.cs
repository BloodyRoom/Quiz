namespace AdminUtility;

static public class VisualHelper
{
    public static void PrintTitle(string title)
    {
        Console.Write("+--");
        foreach (var sym in title)
        {
            Console.Write("-");
        }
        Console.WriteLine("--+");

        Console.WriteLine($"|  {title}  |");

        Console.Write("+--");
        foreach (var sym in title)
        {
            Console.Write("-");
        }
        Console.WriteLine("--+");
    }
    public static void PrintMenu(string[]? actions, string[]? additionalInfo, List<string>? errors, List<string>? success)
    {
        if (errors != null && errors.Count != 0)
        {
            Console.WriteLine("|");

            foreach (var item in errors)
            {
                Console.Write($"|   ");

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(item);
                Console.ResetColor();
            }
        }

        if (success != null && success.Count != 0)
        {
            Console.WriteLine("|");

            foreach (var item in success)
            {
                Console.Write($"|   ");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(item);
                Console.ResetColor();
            }
        }

        if (additionalInfo != null && additionalInfo.Length != 0)
        {
            Console.WriteLine("|");

            foreach (var item in additionalInfo)
            {
                Console.WriteLine($"|   {item}");
            }
        }

        if (actions != null && actions.Length != 0)
        {
            Console.WriteLine("|");

            for (int i = 0; i < actions.Length; i++)
            {
                Console.WriteLine($"|   {i + 1}. {actions[i]}");
            }
        }

        Console.WriteLine("|");
    }
}