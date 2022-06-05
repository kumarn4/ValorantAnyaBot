using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValorantAnyaBot.Data.Cryptography
{
    public class RandomizedStringGenerateService
    {
        private static readonly string _Alpha_U = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static readonly string _Alpha_L = "abcdefghijklmnopqrstuvwxyz";
        private static readonly string _Number  = "0123456789";

        private static Random Rdm;

        public static string Generate(int length)
        {
            string src = _Alpha_U + _Alpha_L + _Number;
            string dst = "";
            Rdm = new Random();

            for (int i = 0; i < length; i++)
            {
                dst += src[Rdm.Next(0, src.Length - 1)].ToString();
            }

            return string.Join("", dst.OrderBy(n => Rdm.Next()));
        }
    }
}
