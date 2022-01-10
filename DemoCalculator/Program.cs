using System;

namespace CalculatorNS
{
    class Program
    {
        private const string _matterOfChoice = "Do you want to calculate file?\nPress y/n";
 
        static void Main(string[] args)
        {
            Console.WriteLine(_matterOfChoice);

            ConsoleKeyInfo key = Console.ReadKey(true);

            if (key.Key == ConsoleKey.Y)
            {
                try
                {
                    new FileServiceCreator().FactoryMethod().WriteCalculatedNumber();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            else if (key.Key == ConsoleKey.N)
            {
                try
                {
                    new ConsoleServiceCreator().FactoryMethod().WriteCalculatedNumber();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}