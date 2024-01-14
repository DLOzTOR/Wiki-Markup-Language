using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WikiMarkupLanguageParser
{
    internal class Element
    {
        public Element? Parent;
        public CoreElement CoreElement;
        public List<Element> Children = new List<Element>();
        public string Name;
        public string[]? Param;
        public string? Content;
        public bool isProcessed;
        public Element(Element parent, CoreElement coreElement, string name, string[] param, string content)
        {
            Parent = parent;
            CoreElement = coreElement;
            Name = name;
            Param = param;
            Content = content;
        }
        public override string ToString()
        {
            var t = Parent;
            var i = 0;
            while (t != null)
            {
                i++;
                t = t.Parent;
            }
            var ts = "";
            for (int j = 0; j < i; j++)
            {
                ts += "  ";
            }
            return ts + Name;
        }
    }
}
