using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;

namespace GvLib
{
    /// <summary>
    /// содержит данные графа соответствующие .gv формату и методы для экспорта 
    /// </summary>
    public class GvGraph
    {
        /// <summary>
        /// путь к .gv файлу
        /// </summary>
        string path;

        /// <summary>
        /// имя графа
        /// </summary>
        public string GraphName = null;

        /// <summary>
        /// показывает, является ли граф ориентированным
        /// </summary>
        bool? isDirected = null;


        /// <summary>
        /// инициализирует новый экземпляр класса GvGraph для ручного задания вершин и ребер
        /// </summary>
        public GvGraph()
        {
            path = "";
        }

        /// <summary>
        /// инициализирует новый экземпляр класса GvGraph
        /// и импортирует граф из .gv файла
        /// </summary>
        /// <param name="path">путь к .gv файлу</param>
        public GvGraph(string path)
        {
            this.path = path;
        }

        /// <summary>
        /// импортирует граф из .gv файла на который указывает поле path
        /// </summary>
        public void ImportFromGv()
        {
            if (path == "")
                throw new Exception("пустой путь к .gv файлу для импорта");
            StreamReader file = new StreamReader(path, Encoding.Default);
            int lineNumber = 0; //номер последней считавшейся строки
            List<char> stack = new List<char>();
            bool isNowComment = false;
            while (!file.EndOfStream && stack.Count < 1)
            {
                string curString = file.ReadLine();
                lineNumber++;
                RemoveComments(ref curString, ref isNowComment);
                var words = RemoveExtraSpace(curString).Split();
                foreach (string word in words)
                {
                    if (word == "{")
                    {
                        if (isDirected == null || stack.Count != 0)
                            throw new IncorrectGvFileContentsException($"Данные файла некорректны (строка {lineNumber})");
                        if (GraphName == "")
                            GraphName = "GraphName";
                        stack.Add('{');
                    }
                    if (isDirected != null && GraphName == null)
                        if (word == "graph")
                            isDirected = false;
                        else if (word == "digraph")
                            isDirected = true;
                }
            }

            while (!file.EndOfStream)
            {
                string s = file.ReadLine();

            }
        }

        /// <summary>
        /// Удаляет из строки все комментарии (учитывая, что сама строка или ее начало может быть комментарием, если isNowComment = 
        /// true) и присваевает isNowComment значение true, если открылся многострочный комментарий и не закрылся в текущей строке
        /// и false — иначе
        /// </summary>
        /// <param name="curString">Текущая обрабатываемая строка</param>
        /// <param name="isNowComment">Показывает, является ли текущая строка (или ее начало) 
        /// частью многострочного комментария</param>
        private static void RemoveComments(ref string curString, ref bool isNowComment)
        {
            if (isNowComment)
            {
                if (curString.Contains("*/"))
                {
                    curString = curString.Substring(curString.IndexOf("*/") + 2);
                    isNowComment = false;
                }
                else
                    return;
            }
            if (curString.Contains("//") && (!curString.Contains("/*") || curString.IndexOf("/*") > curString.IndexOf("//")))
            {
                curString = curString.Substring(0, curString.IndexOf("//"));
                if (curString.Contains("#"))
                    curString = curString.Substring(0, curString.IndexOf("#"));
                return;
            }
            while (curString.Contains("/*"))
            {
                isNowComment = true;
                int i = curString.Substring(curString.IndexOf("/*") + 2).IndexOf("*/");
                if (i == -1)
                {
                    curString = curString.Substring(0, curString.IndexOf("/*"));
                    break;
                }
                int indexOfEndComment = curString.IndexOf("/*") + 2 + i;
                curString = curString.Substring(0, curString.IndexOf("/*")) + curString.Substring(curString.IndexOf("*/") + 2);
                isNowComment = false;
            }
            if (curString.Contains("#"))
                curString = curString.Substring(0, curString.IndexOf("#"));
            if (curString.Contains("//"))
                curString = curString.Substring(0, curString.IndexOf("//"));
        }

        /// <summary>
        /// Возвращает новую строку, в которой удалены пробелы вначале и в конце, а также
        /// заменены двойные пробелы и табуляции на одинарные пробелы
        /// </summary>
        private static string RemoveExtraSpace(string s)
        {
            s = s.Trim();
            while (s.Contains("  "))
                s = s.Replace("  ", " ");
            while (s.Contains('\t'))
                s = s.Replace('\t', ' ');
            return s;
        }
    }
}
