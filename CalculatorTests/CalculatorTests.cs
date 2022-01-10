using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Xunit;

namespace CalculatorNS
{
    public class CalculatorTests
    {
        private const string _divisionbyZero = "Division by zero is not allowed!";
        private const string _nullInputString = "The entered string is null!";
        private const string _bracesNotEqual = "The count of opening and closing braces should be equal!";
        private const string _openBraceMiss = "The opening brace is missing!";
        private const string _cannotConvertWordToNumber = "Сannot convert 'word' to number!";
        private const string _cannotConverNumberWithAnotherSeparator = "Сannot convert '4.815162342' to number!";
        private const string _separatedBySpace = "The number cannot be separated by space!";
        private const string _allRight = "All right!";
        private const char _mult = '*';
        private const char _div = '/';
        private const char _plus = '+';
        private const char _minus = '-';
        private const char _openBrace = '(';
        private const char _closeBrace = ')';

        [Theory]
        [InlineData("./Resources/ExpectedExpressions.txt", "./Resources/ExpressionsToCalculate.txt")]
        public void CheckFileService(string expectedFilePath, string path)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("ru-RU");
            new FileService().WriteCalculatedNumber(path);
            int fileNameIndex = path.LastIndexOf('/') + 1;
            string actualFileText = File.ReadAllText("Calculated " + path[fileNameIndex..]);
            string expectedFileText = File.ReadAllText(expectedFilePath);

            Assert.Equal(expectedFileText, actualFileText);
        }

        [Theory]
        [InlineData(1, "(((47+53)/10 + 50,75) + 40,25 - (-99)) / (8 * ((-59)+84))")]
        [InlineData(-22, "-(-(-(-(1+12)+14)*20)+-7)+(-4+-5)")]
        [InlineData(3, " - 2 + +2 - ( - 3 ) ")]
        [InlineData(-4, " - (+2+2) ")]
        [InlineData(2, "1--1")]
        [InlineData(-10, "2,5*-4")]
        [InlineData(-8, "(1+1) * -(2+2)")]
        [InlineData(23, "-(-23)")]
        [InlineData(10, "-(-(10,5 - 5,5) + -(9,5 - 4,5))")]
        [InlineData(3, "(-((-2)--2))+3")]
        [InlineData(-3, "(-((-2)--2))+-3")]
        [InlineData(64, "(-2 ^ 3) ^ 2")]
        [InlineData(256, "((-2 + 4) ^ 7) / (-2 ^ 3) ^ 2")]

        public void CheckCalculatorConstructor(double calculatedNumber, string inputString)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("ru-RU");
            double actualCalculatedNumber = new Calculator(inputString).CalculatedNumber;
            Assert.Equal(calculatedNumber, actualCalculatedNumber);
        }

        [Theory]
        [InlineData(_nullInputString, null)]
        [InlineData("", "")]
        public void CheckCalculatorConstructorException(string expectedMessage, string inputString)
        {
            string actualMessage = _allRight;

            try
            {
                new Calculator(inputString);
            }
            catch (Exception e)
            {
                actualMessage = e.Message;
            }

            Assert.Equal(expectedMessage, actualMessage);
        }

        [Theory]
        [InlineData(_separatedBySpace, "123 + 23 23")]
        [InlineData(_separatedBySpace, "12, 3 + 23")]
        public void CheckForbiddenSpaces(string expectedMessage, string inputString)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("ru-RU");
            string actualMessage = _allRight;

            try
            {
                Calculator.CheckPossibleNumbersForForbiddenSpaces(inputString);
            }
            catch (Exception e)
            {
                actualMessage = e.Message;
            }

            Assert.Equal(expectedMessage, actualMessage);
        }

        [Theory]
        [InlineData("123+23^23,23-(-2)++(3*2)", " 123 + 23 ^ 23,23 - (-2) + +(3 * 2)")]
        public void CheckDeleteAllSpaces(string expectedString, string inputString)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("ru-RU");
            string actualString = Calculator.DeleteAllSpaces(inputString);

            Assert.Equal(expectedString, actualString);
        }

        [Theory]
        [InlineData("((1+2/3)+23+(2*43))", 3)]
        [InlineData("1+2/3+23+2*43", 0)]
        public void CheckValidBraces(string lineWithBraces, int expectedPairsBraces)
        {
            int pairsBraces = Calculator.GetNumberOfPairBraces(lineWithBraces);

            Assert.Equal(expectedPairsBraces, pairsBraces);
        }

        [Theory]
        [InlineData("((1+2/3)+23+((2*43))", _bracesNotEqual)]
        [InlineData("((1+2/3)))+23+((2*43)", _openBraceMiss)]
        public void CheckInvalidBraces(string lineWithBraces, string expectedMessage)
        {
            string actualMessage = _allRight;

            try
            {
                Calculator.GetNumberOfPairBraces(lineWithBraces);
            }
            catch (Exception e)
            {
                actualMessage = e.Message;
            }

            Assert.Equal(expectedMessage, actualMessage);
        }

        [Theory]
        [InlineData(_cannotConvertWordToNumber, "word+5,2-2,1241")]
        [InlineData(_cannotConverNumberWithAnotherSeparator, "4.815162342 + 5,2 - 2,1241")]
        public void CheckFillNumbersCollectionException(string expectedMessage, string inputString)
        {
            string actualMessage = _allRight;
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("ru-RU");

            try
            {
                Calculator calc = new Calculator(inputString);
            }
            catch (Exception e)
            {
                actualMessage = e.Message;
            }

            Assert.Equal(expectedMessage, actualMessage);
        }


        [Theory]
        [InlineData(0, new double[] { 10.5, 2, 21, -1 }, new char[] { '*', '/', '+' })]
        public void CheckGetCalculatedNumber(double calculatedNumber, double[] numbersArray, char[] signsArray)
        {
            List<double> numbers = new List<double>(numbersArray);
            List<char> signs = new List<char>(signsArray);

            Assert.Equal(calculatedNumber, Calculator.GetCalculatedNumber(numbers, signs));
        }

        [Theory]
        [InlineData(0, new double[] { 10.5, 2, 21, -1 }, new char[] { '*', '/', '+' }, new double[] { 21, 21, -1 }, new char[] { '/', '+' })]
        public void CheckReplaceSpecifiedElement(int specifiedElement, double[] numbersArray, char[] signsArray, double[] expectedNumbersArray, char[] expectedSignsArray)
        {
            List<double> numbers = new List<double>(numbersArray);
            List<char> signs = new List<char>(signsArray);
            List<double> expectedNumbers = new List<double>(expectedNumbersArray);
            List<char> expectedSigns = new List<char>(expectedSignsArray);

            Calculator.ReplaceSpecifiedElement(numbers, signs, specifiedElement);

            Assert.True(expectedNumbers.SequenceEqual(numbers) && expectedSigns.SequenceEqual(signs));
        }

        [Theory]
        [InlineData(-50, -12.5, 4, '*')]
        [InlineData(1000, 42324, 42.324, '/')]
        [InlineData(1984, 2021, -37, '+')]
        [InlineData(1, 0.323232, -0.676768, '-')]
        [InlineData(16, -2, 4, '^')]
        public void CheckСalculateWithValidValues(double calculatedNumber, double firstNumber, double secondNumber, char sign)
        {
            Assert.Equal(calculatedNumber, Calculator.Сalculate(firstNumber, secondNumber, sign));
        }

        [Theory]
        [InlineData(_divisionbyZero, 42324, 0, '/')]
        public void CheckСalculateDivisonByZero(string expectedMessage, double firstNumber, double secondNumber, char sign)
        {
            string actualMessage = _allRight;

            try
            {
                Calculator.Сalculate(firstNumber, secondNumber, sign);
            }
            catch (Exception e)
            {
                actualMessage = e.Message;
            }

            Assert.Equal(expectedMessage, actualMessage);
        }
    } 
}