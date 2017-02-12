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
            var inputPostfix = new List<string>();
            ToPostFix(new Queue<char>(input.ToCharArray()), inputPostfix);
            var result = PostfixToResult(new Queue<string>(inputPostfix));
            return result;
        }

        /// <summary>
        /// 對輸入資料的分組處理，讓轉後序動作更簡單
        /// </summary>
        private void ToPostFix(Queue<char> input, List<string> result, int level = 0, char? process = null)
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

                var next = input.Peek();
                if (next == ')') 
                {   // 先放值再放 operand ，就會變成後置
                    if (resultItem != string.Empty) result.Add(resultItem);
                    switch (process)
                    {
                        case '(':
                            input.Dequeue();
                            break;
                        case '!':
                            var lastEqualsIndex = result.FindLastIndex( r => r == "equals");
                            result[lastEqualsIndex] = "not equals";
                            break;
                        case '=':
                            result.Add("equals");
                            break;
                        case '&':
                            result.Add("and");
                            break;
                    }                   
                    return;
                }

                #endregion

                if (next == '\"')
                {
                    input.Dequeue();    // 把第一個 " 先刪掉，之後會在 Recursive 時以 單引號 的方式加入
                    ToPostFix(input, result, level + 1, process: '\"');
                    resultItem = string.Empty;
                    continue;
                }

                if (next == ',')
                {
                    input.Dequeue();    // 把第一個 " 先刪掉，之後會在 Recursive 時以 單引號 的方式加入
                    ToPostFix(input, result, level + 1, process: '\"');
                    resultItem = string.Empty;
                    continue;
                }

                if (next == ':')
                {
                    result.Add(resultItem);
                    input.Dequeue();
                    resultItem = string.Empty;
                    continue;
                }

                resultItem += input.Dequeue();

                // 加到 item 之後的判斷               

                if (resultItem == "and(")
                {
                    ToPostFix(input, result, level + 1, process: '&');
                    resultItem = string.Empty;
                    continue;
                }

                if (resultItem == "equals(")
                {                   
                    ToPostFix(input, result, level + 1, process: '=');
                    resultItem = string.Empty;
                    continue;
                }

                if (resultItem == "not(")
                {
                    ToPostFix(input, result, level + 1, process: '!');
                    resultItem = string.Empty;
                    continue;
                }
            }
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