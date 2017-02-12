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
            return string.Join(" ", result);
        }

        /// <summary>
        /// 對輸入的資料進行轉換 & 分組
        /// </summary>
        private void Convert(Queue<char> input, List<string> result, int level = 0, char? process = null)
        {
            var resultItem = string.Empty;

            while (input.Count > 0)
            {
                var next = input.Dequeue();

                #region  加到 item 之前的判斷

                #region process 是 Recursive 時的判斷

                // 字串處理，高優先序，不用做多餘的判斷
                if (process == '\"')
                {
                    resultItem += next;

                    if (input.Peek() == '\"')     // 配對的 close 單引號
                    {
                        input.Dequeue();
                        result.Add($"'{resultItem}'");
                        return;
                    }

                    continue;
                }


                if (next == ')')            // close quotation 處理
                {
                    switch (process)
                    {
                        // 可以搭配任意的 ( ) 來使用，不過 open quotation 處理未實作
                        //case '(':
                        //    if (resultItem != string.Empty) result.Add(resultItem);
                        //    result.Add(")");
                        //    break;

                        case '!':  // 不等於的處理方式:用修改上一次的等於
                            result[result.Count - 2] = "!=";
                            if (resultItem != string.Empty) result.Add(resultItem);
                            break;

                        case '=':
                            var lastResult = result[result.Count - 1];
                            if (lastResult[0] == '\'' && lastResult[lastResult.Length - 1] == '\'')
                            {   // 如果上一個項目是字串，插入 = 
                                result.Insert(result.Count - 1, "=");
                            }
                            else
                            {   // 如果上一個項目是不是字串 
                                result.Add("=");
                                if (resultItem != string.Empty) result.Add(resultItem);
                            }
                            break;

                        case '&':
                            if (resultItem != string.Empty) result.Add(resultItem);
                            if (level > 1) result.Add(")");
                            break;

                        case '|':
                            if (resultItem != string.Empty) result.Add(resultItem);
                            if (level > 1) result.Add(")");
                            break;
                    }
                    return;
                }

                #endregion

                switch (next)
                {
                    case '\"':       // 配對的 open 單引號
                        Convert(input, result, level + 1, process: '\"');
                        resultItem = string.Empty;
                        continue;
                    case ',':        // and、or 串接會用到
                        if (process == '&')
                        {
                            result.Add("and");
                            resultItem = string.Empty;
                        }
                        if (process == '|')
                        {
                            result.Add("or");
                            resultItem = string.Empty;
                        }
                        continue;
                    case ':':        // 欄位 delimiter
                        result.Add(resultItem);
                        resultItem = string.Empty;
                        continue;
                }

                #endregion

                resultItem += next;

                #region 加到 item 之後的判斷 

                switch (resultItem)    // open quotation 處理
                {
                    case "and(":
                        if (level != 0) result.Add("(");
                        Convert(input, result, level + 1, process: '&');
                        resultItem = string.Empty;
                        break;
                    case "or(":
                        if (level != 0) result.Add("(");
                        Convert(input, result, level + 1, process: '|');
                        resultItem = string.Empty;
                        break;
                    case "equals(":
                        Convert(input, result, level + 1, process: '=');
                        resultItem = string.Empty;
                        break;
                    case "not(":
                        Convert(input, result, level + 1, process: '!');
                        resultItem = string.Empty;
                        break;
                }

                #endregion
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