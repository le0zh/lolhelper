using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LolWikiApp
{
    public class LetvSourceConverter
    {
        public string Decode(string input)
        {
            int k = 0;
            StringBuilder j = new StringBuilder();
            if (string.IsNullOrEmpty(input)) return input;
            input += "";
            do
            {
                int d = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=".IndexOf(input[k++]);
                int a = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=".IndexOf(input[k++]);
                int e = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=".IndexOf(input[k++]);
                int h = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=".IndexOf(input[k++]);
                int b = d << 18 | a << 12 | e << 6 | h;
                d = b >> 16 & 255;
                a = b >> 8 & 255;
                b &= 255;
                if (e == 64)
                {
                    j.Append((char) d);
                }
                else
                {
                    if (h == 64)
                    {
                        j.Append(Convert.ToChar(d)).Append(Convert.ToChar(a));
                    }
                    else
                    {
                        j.Append(Convert.ToChar(d)).Append(Convert.ToChar(a)).Append(Convert.ToChar(b));
                    }
                }               
            } while (k < input.Length);
            return j.ToString();
        }
    }
}
