using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WikiMarkupLanguageParser
{
    internal class CoreElement
    {
        public static List<string> singleTags = new List<string>() {"WiML","n","img","sl"};
        public static Dictionary<string, string[]> ElementsAllowedTags = new Dictionary<string, string[]>() {
            { "core", new string[]{"WiML", "title", "card","body", "source" } },
            //structural tags
            //->core tags
            { "WiML", new string[]{} },//single
            { "card", new string[]{"img", "d", "cs"} },
            { "body", new string[]{"s"} },
            { "source", new string[]{"si"} },
            //->segment tags
            { "cs", new string[]{ "h", "pt"} },
            { "s", new string[]{ "s", "img", "h", "p", "bl","nl"} },
            { "bl", new string[]{ "li"} },
            { "nl", new string[]{ "li"} },
            //informational tags
            //->core tags
            { "title", new string[]{"b","i", "text"}},
            //->data tags
            { "l", new string[]{"text"} },
            { "sl", new string[]{} },//single
            { "h", new string[]{ "b", "i", "text"} },
            { "d", new string[]{"b","i", "l", "sl", "text"} },
            { "pt", new string[]{ "l", "sl", "b", "i", "n", "text"} },
            { "p", new string[]{ "l", "sl", "b", "i", "n", "text"} },
            { "li", new string[]{ "b", "i", "n", "l", "sl", "text"} },
            { "si", new string[]{ "b", "i", "l", "text"} },
            //->style tags
            { "b", new string[]{ "i", "n", "text"} },
            { "i", new string[]{ "b", "n", "text"} },
            //->separation tags
            { "n", new string[]{} },//single
            //->media content tags
            { "img", new string[]{} },//single
        };
        public string title = "";
        public Element? Card;
        public Element Body;
        public Element Source;
        public string Data;
        public bool isProcessed;
        public static Regex tagWithOutParam = new Regex(@"\[\w+\/?\]");
        public static Regex tagWithShortParam = new Regex(@"\[\w+=.+\/?\]");
        public static Regex tagWithParam = new Regex(@"\[\w+(?:\s+\w+=""[^""]+"")+\/?\]");
        public static Regex tagName = new Regex(@"\w+");
        public CoreElement(string Data)
        {
            this.Data = Data;
        }
        public static Element TagToNode(string tag, string data, Element parent, CoreElement coreElement)
        {
            if(tagName.Match(tag).Value == "b")
            {
                //Console.WriteLine(data);
            }
            if (tagWithOutParam.IsMatch(tag))
            {
                return new Element(parent, coreElement, tagName.Match(tag).Value, null, data);
            }
            else if (tagWithShortParam.IsMatch(tag))
            {
                var t = tag.Substring(1, tag.Length - 1).Split('=');
                return new Element(parent, coreElement, t[0], new string[] { t[1] }, data);
            }
            else if (tagWithParam.IsMatch(tag))
            {
                var name = tagName.Match(tag).Value;
                var t = tag.Substring(1, tag.Length - 1).Replace(name, "").Split(" ");
                return new Element(parent, coreElement, name, t, data);
            }
            else
            {
                throw new Exception(tag);
            }
        }
        public static bool ProcessNode(string data, string name, out List<Element> elements, out string error, Element? parent, CoreElement core)
        {
            Stack<string> elem = new Stack<string>();
            elements = new List<Element>();
            error = string.Empty;
            if (data == string.Empty)
            {
                return true;
            }
            int i = 0;
            int tagStartIndex = 0;
            bool isTagName = false;
            int branchStartIndex = 0;
            bool isBranch = false;
            string branchRootTag = "";
            bool isText = false;
            int textIndex = 0;
            int line = 1;
            int chr = 1;
            while (true)
            {
                if (i > data.Length - 1)
                {
                    break;
                }
                if (data[i] == '[')
                {
                    if (isText)
                    {
                        elements.Add(new Element(parent, core, "text", null, data.Substring(textIndex, i - textIndex)) { isProcessed = true });
                        isText = false;
                    }
                    isTagName = true;
                    tagStartIndex = i;
                }
                if (i == data.Length - 1)
                {
                    if (isText)
                    {
                        elements.Add(new Element(parent, core, "text", null, data.Substring(textIndex, i - textIndex + 1)){isProcessed = true});
                    }
                }
                else
                {
                    if (!isText && !isTagName && !isBranch && !new Regex(@"\s").IsMatch("" + data[i]))
                    {
                        isText = true;
                        textIndex = i;
                    }
                }
                if (data[i] == '\n')
                {
                    line++;
                    chr = 1;
                }
                if (data[i] == ']')
                {
                    if (!isTagName)
                    {
                        throw new Exception();
                    }
                    string temp = data.Substring(tagStartIndex + 1, i - tagStartIndex - 1);
                    if (temp[0] == '/')
                    {
                        var t = elem.Pop();
                        if (tagName.Match(t).Value != tagName.Match(temp).Value)
                        {
                            throw new Exception($"{temp} {t} {line} {chr}");
                        }
                        if (elem.Count == 0)
                        {
                            var rowBranchData = data.Substring(branchStartIndex, i - branchStartIndex + 1);
                            int j = 0;
                            while (rowBranchData[j] != ']' || j > rowBranchData.Length - 1) j++;
                            var tag = rowBranchData.Substring(0, j + 1);
                            string[] param;
                            rowBranchData = rowBranchData.Substring(j + 1, rowBranchData.Length - (branchRootTag.Length + tagName.Match(branchRootTag).Value.Length + 5)).Trim();
                            isBranch = false;
                            elements.Add(TagToNode("[" + tagName.Match(branchRootTag).Value + "]", rowBranchData, parent, core));
                        }
                    }
                    else if (temp[temp.Length - 1] != '/')
                    {
                        elem.Push(temp);
                        if (isBranch == false)
                        {
                            if (!ElementsAllowedTags[name].Contains(tagName.Match(temp).Value))
                            {
                                throw new Exception($"{temp} {name} {line} {chr}");
                            }
                            branchStartIndex = tagStartIndex;
                            isBranch = true;
                            branchRootTag = temp;
                        }
                    }
                    else
                    {
                        if (!singleTags.Contains(tagName.Match(temp).Value))
                        {
                            throw new Exception(temp);
                        }

                        if (!isBranch)
                        {
                            if (!ElementsAllowedTags[name].Contains(tagName.Match(temp).Value))
                            {
                                throw new Exception($"{tagName.Match(temp).Value} {name} {line} {chr}");
                            }
                            elements.Add(TagToNode("[" + temp + "]", string.Empty, parent, core));
                        }
                    }
                    isTagName = false;
                }
                i++;
                chr++;
            }
            return true;
        }
        public static bool ProcessNode(Element element)
        {
            return ProcessNode(element.Content, element.Name, out element.Children, out var error, element, element.CoreElement);
        }
        public static void ProcessTree(Element node)
        {
            ProcessNode(node);
            foreach (var elem in node.Children)
            {
                if(!elem.isProcessed)
                ProcessTree(elem);
            }
        }
        public static void PrintTree(Element node)
        {
            Console.WriteLine(node);
            foreach(var elem in node.Children)
            {
                PrintTree(elem);
            }
        }
        public bool Process()
        {
            ProcessNode(Data, "core", out var el, out var error, null, this);
            foreach(var e in el)
            {
                ProcessTree(e);
                PrintTree(e);
            }
            return true;
        }
    }
}