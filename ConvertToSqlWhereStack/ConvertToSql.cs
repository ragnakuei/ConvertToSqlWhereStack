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
            var result = new List<string>();
            Convert(new Queue<char>(input.ToCharArray()), result);
            return string.Join(" ",result);
        }

        /// <summary>
        /// 對輸入資料的分組處理，讓轉後序動作更簡單
        /// </summary>
        private void Convert(Queue<char> input, List<string> result, int level = 0, char? process = null)
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
                {
                    
                    switch (process)
                    {
                        case '(':
                            input.Dequeue();
                            if (resultItem != string.Empty) result.Add(resultItem);
                            result.Add(")");
                            break;
                        case '!':  // 不等於的處理方式:用修改上一次的等於
                            result[result.Count - 2] = "!=";
                            if (resultItem != string.Empty) result.Add(resultItem);
                            break;
                        case '=':
                            input.Dequeue();

                            var lastResult = result[result.Count-1];
                            if (lastResult[0] == '\'' && lastResult[lastResult.Length-1] == '\'')
                            {   // 如果上一個項目是字串，插入 = 
                                result.Insert(result.Count-1,"=");
                            }
                            else
                            {   // 如果上一個項目是不是字串 
                                result.Add("=");
                                if (resultItem != string.Empty) result.Add(resultItem);
                            }

                            break;
                        case '&':
                            input.Dequeue();
                            if (resultItem != string.Empty) result.Add(resultItem);
                            if (level > 1) result.Add(")");
                            break;
                    }                   
                    return;
                }

                #endregion

                if (next == '\"')
                {
                    input.Dequeue();    // 把 " 先刪掉
                    Convert(input, result, level + 1, process: '\"');
                    resultItem = string.Empty;
                    continue;
                }

                if (next == ',' && process == '&')
                {
                    input.Dequeue();    // 把 , 先刪掉
                    //result.Add(resultItem);
                    result.Add("and");
                    resultItem = string.Empty;
                    continue;
                }

                if (next == ':')
                {
                    input.Dequeue();    // 把 : 先刪掉
                    result.Add(resultItem);
                    resultItem = string.Empty;
                    continue;
                }

                resultItem += input.Dequeue();

                // 加到 item 之後的判斷               

                if (resultItem == "and(")
                {
                    if(level != 0) result.Add("(");
                    Convert(input, result, level + 1, process: '&');
                    resultItem = string.Empty;
                    continue;
                }

                if (resultItem == "equals(")
                {                   
                    Convert(input, result, level + 1, process: '=');
                    resultItem = string.Empty;
                    continue;
                }

                if (resultItem == "not(")
                {
                    Convert(input, result, level + 1, process: '!');
                    resultItem = string.Empty;
                    continue;
                }
            }
        }

        private static void AddEqual(Stack<string> fields, StringBuilder result, string operand)
        {
            var second = fields.Pop();
            var first = fields.Pop();
            result.Append($"{first} {operand} {second}");
        }
    }
}