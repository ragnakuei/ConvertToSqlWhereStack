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

        private string PostfixToResult(Queue<string> inputPostfix)
        {
            var fields = new Stack<string>();
            StringBuilder result = new StringBuilder();
            while (inputPostfix.Count > 0)
            {
                var next = inputPostfix.Peek();
                switch (next)
                {
                    case "equals":
                        var second = fields.Pop();
                        var first = fields.Pop();
                        result.Append($"{first} = {second}");
                        inputPostfix.Dequeue();
                        break;
                    default:
                        fields.Push(inputPostfix.Dequeue());
                        break;
                }
            }
            return result.ToString();
        }

        private Queue<string> ToGroupBy(string input)
        {
            input = input.Replace("\"", "'");
            char[] inputDelimiters = { ':', '(', ')' };
            var inputByList = input.Split(inputDelimiters);
            var result = new Queue<string>(inputByList);
            return result;
        }

        private Queue<string> InfixToPostfix(Queue<string> input)
        {
            var result = new Queue<string>();

            var operand = string.Empty;
            while (input.Count > 0)
            {
                var next = input.Peek();
                switch (next)
                {
                    case "equals":
                        operand = "equals";
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

        private List<char> _delimiters = new List<char> { ':', '(', ')' };
        private List<string> _delimitersString = new List<string>();
    }
}