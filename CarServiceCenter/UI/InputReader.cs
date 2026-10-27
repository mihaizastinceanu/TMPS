namespace CarServiceCenter.UI;

/// <summary>
/// Reads input from the console and keeps asking until the value has a valid format.
/// Business rules (duplicates, existing IDs) are checked by the services, not here.
/// </summary>
public class InputReader
{
    /// <summary>
    /// Reads a non-empty line of text.
    /// </summary>
    public string ReadText(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string input = ReadLine().Trim();

            if (input.Length > 0)
            {
                return input;
            }

            Console.WriteLine("  This field is required. Please enter a value.");
        }
    }

    /// <summary>
    /// Reads a whole number between min and max (inclusive).
    /// </summary>
    public int ReadInt(string prompt, int min, int max)
    {
        while (true)
        {
            Console.Write(prompt);
            string input = ReadLine().Trim();

            if (int.TryParse(input, out int number) && number >= min && number <= max)
            {
                return number;
            }

            Console.WriteLine($"  Invalid input. Please enter a whole number between {min} and {max}.");
        }
    }

    /// <summary>
    /// Asks a yes/no question. Accepts y/yes/n/no in any letter case.
    /// </summary>
    public bool ReadYesNo(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string input = ReadLine().Trim().ToLowerInvariant();

            if (input is "y" or "yes")
            {
                return true;
            }

            if (input is "n" or "no")
            {
                return false;
            }

            Console.WriteLine("  Please answer y (yes) or n (no).");
        }
    }

    /// <summary>
    /// Reads a comma-separated list of numbers between min and max, e.g. "1, 3".
    /// Duplicates are removed and at least one number is required.
    /// </summary>
    public List<int> ReadNumberList(string prompt, int min, int max)
    {
        while (true)
        {
            Console.Write(prompt);
            string[] parts = ReadLine().Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var numbers = new List<int>();
            bool allValid = parts.Length > 0;

            foreach (string part in parts)
            {
                if (int.TryParse(part, out int number) && number >= min && number <= max)
                {
                    numbers.Add(number);
                }
                else
                {
                    allValid = false;
                    break;
                }
            }

            if (allValid)
            {
                return numbers.Distinct().ToList();
            }

            Console.WriteLine($"  Invalid selection. Enter at least one number between {min} and {max}, separated by commas.");
        }
    }

    /// <summary>
    /// Reads one line. Throws EndOfStreamException if the input has ended (e.g. piped input ran out).
    /// </summary>
    private static string ReadLine()
    {
        return Console.ReadLine() ?? throw new EndOfStreamException("No more input.");
    }
}
