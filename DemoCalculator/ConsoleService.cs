using System;

namespace CalculatorNS
{
    public class ConsoleService : IProduct
    {
        private const string _enterExpression = "Enter expression to calculate: ";
        private const string _emptyExpression = "The entered expression is empty!"!;
        public void WriteCalculatedNumber()
        {
            Console.Write(_enterExpression);
            
            string inputString = Console.ReadLine();

            if (inputString == "")
            {
                throw new Exception(_emptyExpression);
            }

            Console.Write($"Calculated number is {new Calculator(inputString).CalculatedNumber}");
        }
    }
}