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
        }

        internal string Result(string input)
        {
            var inputPerString = new List<string>();
            ToQueue(new Queue<char>(input.ToCharArray()), inputPerString);
            var inputPostfix = InfixToPostfix(new Queue<string>(inputPerString));
            var result = PostfixToResult(inputPostfix);
            return result;
        }

        /// <summary>
        /// 對輸入資料的分組處理，讓轉後序動作更簡單
        /// </summary>
        private void ToQueue(Queue<char> input, List<string> result, int level = 0, char? process = null)
        {
            var resultItem = string.Empty;

            while (input.Count > 0)
            {
                // 加到 item 之前的判斷


                #region process 是 Recursive 時的判斷

                // 字串處理，高優先序，不用做多餘的判斷
                if (process == '\"')
                {
                    resultItem += input.Dequeue();

                    if (input.Peek() == '\"')
                    {
                        input.Dequeue();
                        result.Add($"'{resultItem}'");
                        return;
                    }

                    continue;
                }

                if (process == '(' || process == '!')
                {
                    // resultItem += input.Dequeue();

                    if (input.Peek() == ')')
                    {
                        input.Dequeue();
                        if (resultItem != string.Empty) result.Add(resultItem);
                        return;
                    }
                }

                #endregion

                if (input.Peek() == '\"')
                {
                    input.Dequeue();    // 把第一個 " 先刪掉，之後會在 Recursive 時以 單引號 的方式加入
                    ToQueue(input, result, level + 1, process: '\"');
                    resultItem = string.Empty;
                    continue;
                }

                if (input.Peek() == ':')
                {
                    result.Add(resultItem);
                    input.Dequeue();
                    resultItem = string.Empty;
                    continue;
                }

                resultItem += input.Dequeue();

                // 加到 item 之後的判斷               

                if (resultItem == "equals(")
                {
                    if (process == '!')
                    {
                        result.Add("not equals");                      
                    }
                    else
                    {
                        result.Add("equals");
                    }

                    
                    ToQueue(input, result, level + 1, process: '(');
                    resultItem = string.Empty;
                    continue;
                }

                if (resultItem == "not(")
                {
                    ToQueue(input, result, level + 1, process: '!');
                    resultItem = string.Empty;
                    continue;
                }
            }
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
                        operand = input.Dequeue();
                        break;

                    case "not equals":
                        operand = input.Dequeue();
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
    }
}