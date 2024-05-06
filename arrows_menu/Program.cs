internal class Program
{
    static void Main(string[] args)
    {
        Console.Title = "Menu";

        int selected = ConsoleOptionManager.Choices("Choose your option", true, "Option 1", "Option 2", "Option 3");
        Console.WriteLine(selected);
    }
}
