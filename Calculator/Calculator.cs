using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Linq;
using System.Globalization;

[assembly: InternalsVisibleTo("CalculatorTests")]

namespace CalculatorNS
{
    public class Calculator
    {
        private const char _plus = '+';
        private const char _minus = '-';
        private const char _mult = '*';
        private const char _div = '/';
        private const char _exp = '^';
        private const char _openBrace = '(';
        private const char _closeBrace = ')';
        private const string _signsString = "+-*/^()";
        private const string _divisionbyZero = "Division by zero is not allowed!";
        private const string _nullInputString = "The entered string is null!";
        private const string _separatedBySpace = "The number cannot be separated by space!";
        private const string _openBraceMissing = "The opening brace is missing!";
        private const string _bracesNotEqual = "The count of opening and closing braces should be equal!";
        private const string _openBraceAfterNumber = "The opening brace after the possible number!";
        private const string _invalidCharacter = "Invalid character after plus, minus, multiplication, division or exponentiation!";
        private const string _invalidSignAfterOpenBrace = "Invalid sign after the opening brace!";
        private const string _invalidSignAfterCloseBrace = "Invalid sign after the closing brace!";
        private readonly int _inputStringLength;
        public double CalculatedNumber { private set; get; }
        internal SortedDictionary<int, string> _possibleNumbers = new SortedDictionary<int, string>();
        internal SortedDictionary<int, char> _signs = new SortedDictionary<int, char>();
        internal SortedDictionary<int, double> _numbers = new SortedDictionary<int, double>();
        public Calculator(string inputString)
        {
            if (inputString == null)
            {
                throw new Exception(_nullInputString);
            }
            else if (inputString == "")
            {
                throw new Exception(inputString);
            }

            CheckPossibleNumbersForForbiddenSpaces(inputString);
            inputString = DeleteAllSpaces(inputString);
            _inputStringLength = inputString.Length;
            int pairsBraces = GetNumberOfPairBraces(inputString);
            FillSignsAndPossibleNumbersCollections(inputString);
            FillNumbersCollection();

            while (pairsBraces > 0)
            {
                CalculateExpressionInBraces();
                pairsBraces--;
            }

            RemoveUnarySigns(_numbers, _signs);
            CalculatedNumber = GetCalculatedNumber(new List<double>(_numbers.Values), new List<char>(_signs.Values));
        }

        internal static void CheckPossibleNumbersForForbiddenSpaces(string inputString)
        {
            Char decSep = Convert.ToChar(NumberFormatInfo.CurrentInfo.CurrencyDecimalSeparator);
            string pattern = @$"(\d|{decSep})\s+(\d|{decSep})";

            if (Regex.IsMatch(inputString, pattern, RegexOptions.IgnoreCase))
            {
                throw new Exception(_separatedBySpace);
            }
        }

        internal static string DeleteAllSpaces(string inputString)
        {
            inputString = inputString.Trim();
            string pattern = @"(\d\s*[()\-+*\/^]+\s*)|(\s*[()\-+*\/^]+\s*\d)|(\s*[()\-+*\/^]+\s*[()\-+*\/^]+\s*)";

            static string DeleteSpaces(Match m)
            {
                return m.Value.Replace(" ", "").ToString();
            }

            return Regex.Replace(inputString, pattern, DeleteSpaces);
        }

        internal static int GetNumberOfPairBraces(string inputString)
        {
            int braces = 0;
            int pairsBraces = 0;

            for (int i = 0; i < inputString.Length; i++)
            {
                if (inputString[i] == _openBrace)
                {
                    braces++;
                    pairsBraces++;
                }

                if (inputString[i] == _closeBrace)
                {
                    braces--;
                }

                if (braces < 0)
                {
                    throw new Exception(_openBraceMissing);
                }
            }

            if (braces != 0)
            {
                throw new Exception(_bracesNotEqual);
            }

            return pairsBraces;
        }

        internal void FillSignsAndPossibleNumbersCollections(string inputString)
        {
            int indexOfNextNumber = 0;

            for (int i = 0; i < inputString.Length - 1; i++)
            {
                if (!_signsString.Contains(inputString[i]))
                {
                    if (inputString[i + 1] == _openBrace)
                    {
                        throw new Exception(_openBraceAfterNumber);
                    }

                    continue;
                }
                else if (inputString[i] == _plus || inputString[i] == _minus)
                {
                    if ((i == 0 || inputString[i - 1] != _closeBrace) && i == indexOfNextNumber && !_signsString.Contains(inputString[i + 1]))
                    {
                        continue;
                    }
                    else
                    {
                        CheckPlusOrMinusOrMultOrDivOrExp(ref i);
                    }
                }
                else if (inputString[i] == _mult || inputString[i] == _div || inputString[i] == _exp)
                {
                    CheckPlusOrMinusOrMultOrDivOrExp(ref i);
                }
                else if (inputString[i] == _openBrace)
                {
                    CheckOpenBrace(ref i);
                }
                else
                {
                    CheckCloseBrace(ref i);
                }
            }

            CheckLastCharacter();

            void AddSignAndPossibleNumberToCollections(int i)
            {
                _signs.Add(i, inputString[i]);
                TryAddPossibleNumberToList(inputString[indexOfNextNumber..i], indexOfNextNumber);
                indexOfNextNumber = i + 1;
            }

            bool TryAddPossibleNumberToList(string possibleNumber, int numberIndex)
            {
                if (possibleNumber == "")
                {
                    return false;
                }

                _possibleNumbers.Add(numberIndex, possibleNumber);
                return true;
            }

            void CheckPlusOrMinusOrMultOrDivOrExp(ref int i)
            {
                if (!_signsString.Contains(inputString[i + 1]) || inputString[i + 1] == _minus || inputString[i + 1] == _plus)
                {
                    AddSignAndPossibleNumberToCollections(i);
                }
                else if (inputString[i + 1] == _openBrace)
                {
                    AddSignAndPossibleNumberToCollections(i);
                    _signs.Add(i + 1, inputString[i + 1]);
                    indexOfNextNumber++;
                    i++;
                }
                else
                {
                    throw new Exception(_invalidCharacter);
                }
            }

            void CheckOpenBrace(ref int i)
            {
                if (!_signsString.Contains(inputString[i + 1]) || inputString[i + 1] == _minus || inputString[i + 1] == _plus)
                {
                    _signs.Add(i, inputString[i]);
                    indexOfNextNumber = i + 1;
                }
                else if (inputString[i + 1] == _openBrace)
                {
                    _signs.Add(i, inputString[i]);
                    _signs.Add(i + 1, inputString[i + 1]);
                    indexOfNextNumber = i + 2;
                    i++;
                }
                else
                {
                    throw new Exception(_invalidSignAfterOpenBrace);
                }
            }

            void CheckCloseBrace(ref int i)
            {
                if (inputString[i + 1] == _openBrace || !_signsString.Contains(inputString[i + 1]))
                {
                    throw new Exception(_invalidSignAfterCloseBrace);
                }
                else
                {
                    AddSignAndPossibleNumberToCollections(i);
                    _signs.Add(i + 1, inputString[i + 1]);
                    indexOfNextNumber++;
                    i++;
                }
            }

            void CheckLastCharacter()
            {
                if (inputString[^1] != _closeBrace)
                {
                    TryAddPossibleNumberToList(inputString[indexOfNextNumber..], indexOfNextNumber);
                }
                else if (inputString[^2] != _closeBrace)
                {
                    _signs.Add(inputString.Length - 1, inputString[^1]);
                    TryAddPossibleNumberToList(inputString[indexOfNextNumber..(inputString.Length - 1)], indexOfNextNumber);
                }
            }
        }

        internal void FillNumbersCollection()
        {
            foreach (KeyValuePair<int, string> possibleNumber in _possibleNumbers)
            {
                if (double.TryParse(possibleNumber.Value, NumberStyles.Float, NumberFormatInfo.CurrentInfo, out double number))
                {
                    _numbers.Add(possibleNumber.Key, number);
                }
                else
                {
                    throw new Exception($"Сannot convert '{possibleNumber.Value}' to number!");
                }
            }
        }

        internal bool CalculateExpressionInBraces()
        {
            for (int indexOpenBrace = 0; indexOpenBrace < _inputStringLength;)
            {
                if (_signs.ContainsKey(indexOpenBrace) && _signs[indexOpenBrace] != _openBrace)
                {
                    indexOpenBrace++;
                    continue;
                }

                for (int indexCloseBrace = indexOpenBrace + 1; indexCloseBrace < _inputStringLength; indexCloseBrace++)
                {
                    if (_signs.ContainsKey(indexCloseBrace) && _signs[indexCloseBrace] == _openBrace)
                    {
                        indexOpenBrace = indexCloseBrace;
                        break;
                    }
                    else if (_signs.ContainsKey(indexCloseBrace) && _signs[indexCloseBrace] == _closeBrace)
                    {
                        SortedDictionary<int, double> numbersToCalculate = new SortedDictionary<int, double>();
                        SortedDictionary<int, char> signsToCalculate = new SortedDictionary<int, char>();

                        FillCollectionsToCalculate(indexOpenBrace, indexCloseBrace, numbersToCalculate, signsToCalculate);
                        _signs.Remove(indexOpenBrace);
                        _signs.Remove(indexCloseBrace);
                        RemoveUnarySigns(numbersToCalculate, signsToCalculate);
                        _numbers.Add(indexOpenBrace, GetCalculatedNumber(new List<double>(numbersToCalculate.Values), new List<char>(signsToCalculate.Values)));

                        return true;
                    }
                }
            }

            return false;
        }

        internal void FillCollectionsToCalculate(int indexOpenBrace, int indexCloseBrace, SortedDictionary<int, double> numbersToCalculate, SortedDictionary<int, char> signsToCalculate)
        {
            for (int i = indexOpenBrace + 1; i < indexCloseBrace; i++)
            {
                if (_signs.ContainsKey(i))
                {
                    _signs.Remove(i, out char sign);
                    signsToCalculate.Add(i, sign);
                }

                if (_numbers.ContainsKey(i))
                {
                    _numbers.Remove(i, out double number);
                    numbersToCalculate.Add(i, number);
                }
            }
        }

        internal int RemoveFirstUnarySign(SortedDictionary<int, double> numbers, SortedDictionary<int, char> signs)
        {
            int firstIndex = 0;

            if (numbers.Count <= signs.Count)
            {
                int firstNumberIndex = numbers.First().Key;
                int firstSignIndex = signs.First().Key;

                if (firstNumberIndex < firstSignIndex)
                {
                    firstIndex = firstSignIndex;
                }
                else
                {
                    firstIndex = firstNumberIndex + 1;

                    if (signs[firstSignIndex] == _minus)
                    {
                        numbers[firstNumberIndex] = -numbers[firstNumberIndex];
                    }

                    signs.Remove(firstSignIndex);
                }
            }

            return firstIndex;
        }

        internal void RemoveUnarySigns(SortedDictionary<int, double> numbers, SortedDictionary<int, char> signs)
        {
            int firstIndex = RemoveFirstUnarySign(numbers, signs);

            for (int i = firstIndex; numbers.Count <= signs.Count; i++)
            {
                if (signs.ContainsKey(i))
                {
                    for (int indexAfterSign = i + 1; ; indexAfterSign++)
                    {
                        if (numbers.ContainsKey(indexAfterSign))
                        {
                            break;
                        }

                        if (signs.ContainsKey(indexAfterSign))
                        {
                            if (signs[indexAfterSign] == _minus)
                            {
                                int indexOfNearestNumber = indexAfterSign + 1;

                                while (!numbers.ContainsKey(indexOfNearestNumber))
                                {
                                    indexOfNearestNumber++;
                                }

                                numbers[indexOfNearestNumber] = -numbers[indexOfNearestNumber];
                            }

                            signs.Remove(indexAfterSign);
                            break;
                        }
                    }
                }
            }
        }

        internal static double GetCalculatedNumber(List<double> numbers, List<char> signs)
        {
            for (int i = 0; i < signs.Count;)
            {
                if (signs[i] == _mult || signs[i] == _div || signs[i] == _exp)
                {
                    ReplaceSpecifiedElement(numbers, signs, i);
                }
                else
                {
                    i++;
                }
            }

            for (int i = 0; i < signs.Count;)
            {
                ReplaceSpecifiedElement(numbers, signs, i);
            }

            return numbers[0];
        }

        internal static void ReplaceSpecifiedElement(List<double> numbers, List<char> signs, int i)
        {
            numbers[i] = Сalculate(numbers[i], numbers[i + 1], signs[i]);
            numbers.RemoveAt(i + 1);
            signs.RemoveAt(i);
        }

        internal static double Сalculate(double firstNumber, double secondNumber, char sign)
        {
            if (sign == _mult)
            {
                firstNumber *= secondNumber;
            }
            else if (sign == _div)
            {
                firstNumber /= secondNumber != 0 ? secondNumber : throw new DivideByZeroException(_divisionbyZero);
            }
            else if (sign == _exp)
            {
                firstNumber = Math.Pow(firstNumber, secondNumber);
            }
            else if (sign == _plus)
            {
                firstNumber += secondNumber;
            }
            else
            {
                firstNumber -= secondNumber;
            }

            return firstNumber;
        }
    }
}