using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WikiMarkupLanguageParser
{
    static internal class WiMLToHTML
    {
        public static string Convert(string WiML)
        {
            string html = "";
            var core = new CoreElement(WiML);
            core.Process();
            core.PrintAST();
            html += $"<h1>{core.title}</h1>";
            if(core.Card is not null)
            {
                html += $"<h3>{core.title}</h3><div class=\"card\">{childrenToHtml(core.Card)}</div>";
            }
            html += $"<div class=\"main\">{childrenToHtml(core.Body)}</div>";
            if (core.Source is not null)
            {
                html += $"<div class=\"source\">{childrenToHtml(core.Source)}</div>";
            }
            return html;
        }
        static string childrenToHtml(Element el)
        {
            return string.Join("", el.Children.Select((x) => ElementToHTML(x)));
        }
        static string ElementToHTML(Element element)
        {
            string html = "";

            switch (element.Name) {
                case "img":
                    html += "<img src=" + element.Param[0].Split("=", StringSplitOptions.RemoveEmptyEntries)[1];
                    if (element.Param.Length > 1)
                    {
                        html += "alt=" + element.Param[1].Split("=")[1] + ">";
                        html += $"<div class=\"img-description\">{element.Param[1].Split("=", StringSplitOptions.RemoveEmptyEntries)[1].Trim('"')}</div>";
                    }
                    break;
                case "d":
                    html += $"<p>{childrenToHtml(element)}</p>";
                    break;
                case "cs":
                    html += $"<table>{childrenToHtml(element)}</table>";
                    break;
                case "h":
                    if (element.Parent.Name == "cs")
                    {
                        html += $"<caption>{childrenToHtml(element)}</caption>";
                    }
                    else
                    {
                        html += $"<h2>{childrenToHtml(element)}</h2>";
                    }
                    break;
                case "pt":
                    html += $"<tr><th>{element.Param[0].Trim('"')}</th><td>{childrenToHtml(element)}</td></tr>";
                    break;
                case "l":
                    html += $"<a href={element.Param[0]}>{childrenToHtml(element)}</a>";
                    break;
                case "sl":
                    html += $"<sup>{element.Param[0].Trim('"')}</sup>";
                    break;
                case "b":
                    html += $"<b>{childrenToHtml(element)}</b>";
                    break;
                case "i":
                    html += $"<i>{childrenToHtml(element)}</i>";
                    break;
                case "n":
                    html += "<br/>";
                    break;
                case "s":
                    html += $"<div>{childrenToHtml(element)}</div>";
                    break;
                case "p":
                    html += $"<p>{childrenToHtml(element)}</p>";
                    break;
                case "nl":
                    html += $"<ol>{childrenToHtml(element)}</ol>";
                    break;
                case "bl":
                    html += $"<ul>{childrenToHtml(element)}</ul>";
                    break;
                case "li":
                case "si":
                    html += $"<li>{childrenToHtml(element)}</li>";
                    break;
                case "text":
                    html += element.Content;
                    break;
            }
            return html;
        }
    }
}