﻿using System.Text;
namespace WikiMarkupLanguageParser
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var t = string.Join( "\n",File.ReadAllLines("./article.wiml"));
            new CoreElement(t).Process();
        }
    }
}