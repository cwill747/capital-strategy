using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CapitalStrategy.GUI
{
    public class StringHelper
    {
        public static List<String> SplitString(String text, int length)
        {
            List<String> output = new List<String>();

            List<String> words = Split(text);

            int i = 0;
            string s = String.Empty;
            while (i < words.Count)
            {
                if (s.Length + words[i].Length <= length)
                {
                    s += words[i];
                    i++;
                }
                else
                {
                    output.Add(s);
                    s = String.Empty;
                }

            }
            output.Add(s);
            //s.Remove(s.Length - 1, 1);// deletes last extra space.

            return output;
        }


        public static List<string> Split(string text)
        {
            List<string> result = new List<string>();
            StringBuilder sb = new StringBuilder();

            foreach (var letter in text)
            {
                if (letter != ' ' && letter != '\t' && letter != '\n')
                {
                    sb.Append(letter);
                }
                else
                {
                    if (sb.Length > 0)
                    {

                        result.Add(sb.ToString());
                    }

                    result.Add(letter.ToString());
                    sb = new StringBuilder();
                }
            }

            return result;
        }
    }
}
