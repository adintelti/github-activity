namespace github_activity
{
    public static class ConsoleMessages
    {
        public static void PrintInfo(string info)
        {
            Console.WriteLine(info);
            Console.ResetColor();
        }

        public static void PrintError(string error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(error);
            Console.ResetColor();
        }

        public static void PrintCommandMessage(string command)
        {
            Console.WriteLine(command);
            Console.ResetColor();
        }
    }
}
