class Program
{
    public static void Main(string[] args)
    {
        string input;
        string output;

        if (args.Length == 0)
        {
            input = "input.txt";
            output = "output.txt";
        }
        else if (args.Length == 2)
        {
            input = args[0];
            output = args[1];
        }
        else
        {
            Console.WriteLine("Invalid arguments. Usage: <input_file> <output_file>");
            return;
        }

        string line;
        try
        {
            string[] lines = File.ReadAllLines(input);
            if (lines.Length == 0)
            {
                Console.WriteLine("File empty");
                return;
            }
            line = lines[0];
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"File {input} not found.");
            return;
        }

        int[] numbers;
        try
        {
            numbers = line.Split(' ').Select(int.Parse).ToArray();
        }
        catch (FormatException)
        {
            Console.WriteLine($"Invalid input format. Please provide space-separated integers in {input}.");
            return;
        }

        QuickSort(numbers);

        try
        {
            using (StreamWriter writer = new StreamWriter(output))
            {
                foreach (int number in numbers)
                {
                    writer.Write(number + " ");
                }
            }
            Console.WriteLine($"Array from {input} has been sorted and written to {output}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while writing to {output}: {ex.Message}");
        }
    }

    static void QuickSort(int[] arr)
    {
        QuickSort(arr, 0, arr.Length - 1);
    }

    static void QuickSort(int[] arr, int low, int high)
    {
        if (low < high)
        {
            int pi = Partition(arr, low, high);
            QuickSort(arr, low, pi - 1);
            QuickSort(arr, pi + 1, high);
        }
    }

    private static int Partition(int[] arr, int low, int high)
    {
        int pivot = arr[high];
        int i = low - 1;
        for (int j = low; j <= high - 1; j++)
        {
            if (arr[j] < pivot)
            {
                i++;
                int tep = arr[i];
                arr[i] = arr[j];
                arr[j] = tep;
            }
        }
        int temp = arr[i+1];
        arr[i] = arr[high];
        arr[high] = temp;
        return i + 1;
    }
}
