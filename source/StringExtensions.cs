using System;

namespace RemoveTableAndBlobs
{
    public static class StringExtensions
    {
        public static void WriteLine(this string text, ConsoleColor color)
        {
            System.Console.BackgroundColor = ConsoleColor.Black;
            System.Console.ForegroundColor = color;
            System.Console.WriteLine(text);
        }

        public static void WriteLine(this string text)
        {
            System.Console.BackgroundColor = ConsoleColor.Black;
            System.Console.WriteLine(text);
        }

        public static void Write(this string text, ConsoleColor color)
        {
            System.Console.BackgroundColor = ConsoleColor.Black;
            System.Console.ForegroundColor = color;
            System.Console.Write(text);
        }

        public static void Write(this string text)
        {
            System.Console.BackgroundColor = ConsoleColor.Black;
            System.Console.Write(text);
        }

    }
}
