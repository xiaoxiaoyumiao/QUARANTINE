// CREDIT: https://www.cnblogs.com/lyh916/p/8588218.html 

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

/* Brief intro to every method here:
 * 
 *  GetCSVFormat(str) -> str
 *      把需要填入单个单元格的文本str合法化，
 *      逻辑似乎并不严谨，但不包含奇怪字符时应该可以使用
 *  GetCSVFormatLine(List<str>) -> str
 *      把需要填入一行的文本str列表
 *      连接成csv的一个合法行返回，调用了GetCSVFormat().
 *  ParseLine(str) -> List<str>
 *      把csv的一个合法行解析成str列表返回。
 *      逻辑比较复杂，暂且信任
 *  Read(filePath, encoding) -> List<List<str>> 
 *      常规调用的接口，从字符串描述的filePath文件
 *      （按encoding编码）中读取csv数据并直接返回
 *      解析后的元素的列表的列表。
 *  Write(filePath, encoding, List<List<str>>) -> void
 *      常规调用的接口，向字符串描述的filePath文件
 *      （按encoding编码）写入元素的列表的列表数据。
 * */

public class CSVTool
{

    private static char _csvSeparator = ',';
    private static bool _trimColumns = false;

    //获取一个单元格的写入格式
    public static string GetCSVFormat(string str)
    {
        string tempStr = str;
        if (str.Contains(",")) // why?
        {
            if (str.Contains("\""))
            {
                tempStr = str.Replace("\"", "\"\"");
            }
            tempStr = "\"" + tempStr + "\"";
        }
        return tempStr;
    }

    //获取一行的写入格式
    public static string GetCSVFormatLine(List<string> strList)
    {
        string tempStr = "";
        for (int i = 0; i < strList.Count - 1; i++)
        {
            string str = strList[i];
            tempStr = tempStr + GetCSVFormat(str) + ",";
        }
        tempStr = tempStr + GetCSVFormat(strList[strList.Count - 1]) + "\r\n";
        return tempStr;
    }

    //解析一行
    public static List<string> ParseLine(string line)
    {
        StringBuilder _columnBuilder = new StringBuilder();
        List<string> Fields = new List<string>();
        bool inColumn = false;  //是否是在一个列元素里
        bool inQuotes = false;  //是否需要转义
        bool isNotEnd = false;  //读取完毕未结束转义
        _columnBuilder.Remove(0, _columnBuilder.Length);

        //空行也是一个空元素,一个逗号是2个空元素
        if (line == "")
        {
            Fields.Add("");
        }

        // Iterate through every character in the line
        for (int i = 0; i < line.Length; i++)
        {
            char character = line[i];

            // If we are not currently inside a column
            if (!inColumn)
            {
                // If the current character is a double quote then the column value is contained within
                // double quotes, otherwise append the next character
                inColumn = true;
                if (character == '"')
                {
                    inQuotes = true;
                    continue;
                }

            }

            // If we are in between double quotes
            if (inQuotes)
            {
                if ((i + 1) == line.Length)//这个字符已经结束了整行
                {
                    if (character == '"') //正常转义结束，且该行已经结束
                    {
                        inQuotes = false;
                        continue;     //当前字符不用添加，跳出后直结束后会添加该元素
                    }
                    else //异常结束，转义未收尾
                    {
                        isNotEnd = true;
                    }
                }
                else if (character == '"' && line[i + 1] == _csvSeparator) //结束转义，且后面有可能还有数据
                {
                    inQuotes = false;
                    inColumn = false;
                    i++; //跳过下一个字符
                }
                else if (character == '"' && line[i + 1] == '"') //双引号转义
                {
                    i++; //跳过下一个字符
                }
                else if (character == '"') //双引号单独出现（这种情况实际上已经是格式错误，为了兼容可暂时不处理）
                {
                    throw new Exception("格式错误，错误的双引号转义");
                }
                //其他情况直接跳出，后面正常添加

            }
            else if (character == _csvSeparator)
                inColumn = false;

            // If we are no longer in the column clear the builder and add the columns to the list
            if (!inColumn) //结束该元素时inColumn置为false，并且不处理当前字符，直接进行Add
            {
                Fields.Add(_trimColumns ? _columnBuilder.ToString().Trim() : _columnBuilder.ToString());
                _columnBuilder.Remove(0, _columnBuilder.Length);

            }
            else // append the current column
                _columnBuilder.Append(character);
        }

        // If we are still inside a column add a new one （标准格式一行结尾不需要逗号结尾，而上面for是遇到逗号才添加的，为了兼容最后还要添加一次）
        if (inColumn)
        {
            if (isNotEnd)
            {
                _columnBuilder.Append("\r\n");
            }
            Fields.Add(_trimColumns ? _columnBuilder.ToString().Trim() : _columnBuilder.ToString());
        }
        else  //如果inColumn为false，说明已经添加，因为最后一个字符为分隔符，所以后面要加上一个空元素
        {
            Fields.Add("");
        }


        return Fields;
    }

    //读取文件
    public static List<List<string>> Read(string filePath, Encoding encoding)
    {
        List<List<string>> result = new List<List<string>>();
        string content = File.ReadAllText(filePath, encoding);
        string[] lines = content.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < lines.Length; i++)
        {
            List<string> line = ParseLine(lines[i]);
            result.Add(line);
        }
        return result;
    }

    //写入文件
    public static void Write(string filePath, Encoding encoding, List<List<string>> result)
    {
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < result.Count; i++)
        {
            List<string> line = result[i];
            builder.Append(GetCSVFormatLine(line));
        }
        File.WriteAllText(filePath, builder.ToString(), encoding);
    }

    //打印
    public static void Debug(List<List<string>> result)
    {
        for (int i = 0; i < result.Count; i++)
        {
            List<string> line = result[i];
            for (int j = 0; j < line.Count; j++)
            {
                UnityEngine.Debug.LogWarning(line[j]);
            }
        }
    }
}