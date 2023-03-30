using LanguageDetector;

namespace Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var dector = new Detector();
            foreach (var file in Directory.EnumerateFiles("samples", "*.*", SearchOption.AllDirectories))
            {
                Console.WriteLine(file);
                var expected = Enum.Parse<Language>(Path.GetFileName(Path.GetDirectoryName(file))!, true);

                var detected = dector.Detect(File.ReadAllText(file), out var score);
                Console.WriteLine(" " + detected + " (" + score + ")");

                if (expected != detected)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("ERROR expected: " + expected + " detected: " + detected);
                    Console.ResetColor();
                }
            }
        }
    }
}