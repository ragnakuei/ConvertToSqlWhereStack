using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConvertToSqlWhereStack
{
    internal class ConvertToSql
    {
        public ConvertToSql()
        {
            _delimitersString = _delimiters.Select(c => c.ToString()).ToList();
        }

        internal string Result(string input)
        {
            var inputGroupBy = ToGroupBy(input);
            var inputPostfix = InfixToPostfix(inputGroupBy);
            var result = PostfixToResult(inputPostfix);
            return result;
        }

        private Queue<string> ToGroupBy(string input)
        {
            input = input.Replace("\"", "'");
            char[] inputDelimiters = { ':', '(', ')' };
            var inputByList = input.Split(inputDelimiters).ToList();
            inputByList.RemoveAll(s => s == string.Empty);
            var result = new Queue<string>(inputByList);
            return result;
        }


        private Queue<string> InfixToPostfix(Queue<string> input)
        {
            var result = new Queue<string>();

            var operand = string.Empty;
            var notEqual = false;
            while (input.Count > 0)
            {
                var next = input.Peek();
                switch (next)
                {
                    case "equals":

                        if (notEqual)
                        { operand = "not equals"; }
                        else
                        {
                            operand = "equals";
                            notEqual = false;
                        }

                        input.Dequeue();
                        break;

                    case "not":
                        notEqual = true;
                        input.Dequeue();
                        break;

                    default:
                        if (next != string.Empty)
                            result.Enqueue(input.Dequeue());
                        else
                            input.Dequeue();
                        break;
                }
            }
            result.Enqueue(operand);

            return result;
        }

        private string PostfixToResult(Queue<string> inputPostfix)
        {
            var fields = new Stack<string>();
            StringBuilder result = new StringBuilder();
            while (inputPostfix.Count > 0)
            {
                var operand = string.Empty;

                var next = inputPostfix.Peek();
                switch (next)
                {
                    case "equals":
                        operand = "=";
                        AddEqual(fields, result, operand);
                        inputPostfix.Dequeue();
                        break;
                    case "not equals":
                        operand = "!=";
                        AddEqual(fields, result, operand);
                        inputPostfix.Dequeue();
                        break;
                    default:
                        fields.Push(inputPostfix.Dequeue());
                        break;
                }
            }
            return result.ToString();
        }

        private static void AddEqual(Stack<string> fields, StringBuilder result, string operand)
        {
            var second = fields.Pop();
            var first = fields.Pop();
            result.Append($"{first} {operand} {second}");
        }

        private List<char> _delimiters = new List<char> { ':', '(', ')' };
        private List<string> _delimitersString = new List<string>();
    }
}