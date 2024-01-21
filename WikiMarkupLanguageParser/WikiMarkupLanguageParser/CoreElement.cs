using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
//TODO: params check
namespace WikiMarkupLanguageParser
{
    internal class CoreElement
    {
        public static List<string> singleTags = new List<string>() { "WiML", "n", "img", "sl" };
        public static List<string> notNullParam = new List<string>() { "pt" };
        public static List<string> requireData = new List<string>() { "title", "card", "body", "source", "cs", "s", "bl", "nl", "l", "h", "d", "pt", "p", "li", "si", "b", "i" };
        public static Dictionary<string, Dictionary<string, string>> requiredTagsCount = new Dictionary<string, Dictionary<string, string>>() // 1 - required and no more than 1; ? - no more than 1; + - required;
        {
            {"core", new Dictionary<string, string>() { {"WiML", "1"}, {"title", "1"}, { "card", "?" }, {"body", "1"}, {"source", "?"} } },
            {"card", new Dictionary<string, string>() { { "img", "?"}, { "d", "?"}, } },
            {"source", new Dictionary<string, string>() { { "si", "+"}, } },
            {"cs", new Dictionary<string, string>() { { "h", "1"}, } },
            {"s", new Dictionary<string, string>() { { "h", "1"}, } },
            {"bl", new Dictionary<string, string>() { { "li", "+"}, } },
            {"nl", new Dictionary<string, string>() { { "li", "+"}, } },
        };
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
            { "bl", new string[]{"li"} },
            { "nl", new string[]{"li"} },
            //informational tags
            //->core tags
            { "title", new string[]{"b","i", "text"}},
            //->data tags
            { "l", new string[]{"text"} },
            { "sl", new string[]{} },//single
            { "h", new string[]{ "b", "i", "text"} },
            { "d", new string[]{"b","i", "l", "sl", "text"} },
            { "pt", new string[]{ "img", "l", "sl", "b", "i", "n", "text"} },
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
        public Element? Source;
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
            var name = tagName.Match(tag).Value;
            if (requireData.Contains(name) && string.IsNullOrEmpty(data))
                throw new Exception($"{name} mast contains data");
            Element elem;
            if (tagWithOutParam.IsMatch(tag))
            {
                elem = new Element(parent, coreElement, name, null, data);
            }
            else if (tagWithShortParam.IsMatch(tag))
            {
                var t = tag.Substring(1, tag.Length - 1).Split('=', StringSplitOptions.RemoveEmptyEntries);
                t[1] = t[1].Replace("]", "").Trim('/');
                t[1] = t[1].Substring(0, t[1].Length);
                elem = new Element(parent, coreElement, name, new string[] { t[1] }, data);
            }
            else if (tagWithParam.IsMatch(tag))
            {

                var t = tag.Substring(1, tag.Length - 1).Replace(name, "").Replace("]", "").Trim('/').Split(" ", StringSplitOptions.RemoveEmptyEntries);
                elem = new Element(parent, coreElement, name, t, data);
            }
            else
            {
                throw new Exception(tag);
            }
            if (notNullParam.Contains(name))
            {
                if (elem.Param is not null)
                {
                    foreach (var param in elem.Param)
                    {
                        if (string.IsNullOrEmpty(param))
                        {
                            throw new Exception($"require not empty parameters {tag}");
                        }
                    }
                }
                else
                {
                    throw new Exception($"require not empty parameters, {elem.Param} {elem.Name}, {elem.Content}, {tag}");
                }
            }
            return elem;
        }
        public static bool ProcessNode(string data, string name, out List<Element> elements, Element? parent, CoreElement core)
        {
            Stack<string> elem = new Stack<string>();
            elements = new List<Element>();
            data = Regex.Replace(data, @"\s+", " ");
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
                    if (requiredTagsCount.ContainsKey(name))//check children elements
                    {
                        foreach (var rule in requiredTagsCount[name])
                        {
                            var count = elements.Count(x => x.Name == rule.Key);
                            switch (rule.Value)
                            {
                                case "1":
                                    if (count != 1)
                                    {
                                        throw new Exception($"only one {rule.Key} tag allowed in {name}");
                                    }
                                    break;
                                case "?":
                                    if (count > 1)
                                    {
                                        throw new Exception($"no more than one {rule.Key} tag allowed in {name}");
                                    }
                                    break;
                                case "+":
                                    if (count < 1)
                                    {
                                        throw new Exception($"requires at least one tag {rule.Key} in {name}");
                                    }
                                    break;
                                default: throw new Exception($"there is no such rule as {rule.Value}");
                            }
                        }
                    }
                    break; //break main cycle
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
                        elements.Add(new Element(parent, core, "text", null, data.Substring(textIndex, i - textIndex + 1)) { isProcessed = true });
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
                            elements.Add(TagToNode(tag, rowBranchData, parent, core));
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
            return ProcessNode(element.Content, element.Name, out element.Children, element, element.CoreElement);
        }
        public static void ProcessTree(Element node)
        {
            ProcessNode(node);
            foreach (var elem in node.Children)
            {
                if (!elem.isProcessed)
                    ProcessTree(elem);
            }
        }
        public static void PrintTree(Element node)
        {
            Console.WriteLine(node);
            foreach (var elem in node.Children)
            {
                PrintTree(elem);
            }
        }
        public bool Process()
        {
            ProcessNode(Data, "core", out var el, null, this);
            title = el.First(x => x.Name == "title").Content;
            Body = el.First(x => x.Name == "body");
            Card = el.FirstOrDefault(x => x.Name == "card");
            Source = el.FirstOrDefault(x => x.Name == "source");
            if (Source is not null)
            {
                ProcessTree(Source);
            }
            if (Card is not null)
            {
                ProcessTree(Card);
            }
            ProcessTree(Body);
            return true;
        }
        public void PrintAST()
        {
            Console.WriteLine(title);
            if (Card is not null)
            {
                PrintTree(Card);
            }
            PrintTree(Body);
            if (Source is not null)
            {
                PrintTree(Source);
            }
        }
    }
}