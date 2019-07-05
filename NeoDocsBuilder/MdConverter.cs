using Microsoft.Toolkit.Parsers.Markdown;
using Microsoft.Toolkit.Parsers.Markdown.Blocks;
using Microsoft.Toolkit.Parsers.Markdown.Inlines;
using static System.Web.HttpUtility;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Security.Cryptography;

namespace NeoDocsBuilder
{
    public static class MdConverter
    {
        /// <summary>
        /// 将块级元素转了 HTML
        /// </summary>
        /// <param name="block">MarkDown 块级元素</param>
        /// <returns>HTML</returns>
        public static string ToHtml(this MarkdownBlock block, string args = null)
        {
            var result = string.Empty;
            switch (block.Type)
            {
                case MarkdownBlockType.Code:
                    var code = block as CodeBlock;
                    var lang = string.IsNullOrEmpty(code.CodeLanguage) ? "" : $" class='{code.CodeLanguage.ToHlJs()}' lang='{HtmlEncode(code.CodeLanguage.ToHlJs())}'";
                    var encode = HtmlEncode(code.Text);
                    result += $"\r\n<figure class='highlight'>\r\n<button type='button' class='btn-clipboard' data-original-title='Copy to clipboard' data-clipboard-action='copy' data-toggle='tooltip' data-placement='top' title='Copy to clipboard' data-clipboard-text='{encode}'>Copy</button><pre><code{lang}>{encode}\r\n</code></pre>\r\n</figure>";
                    break;
                case MarkdownBlockType.Header:
                    var header = block as HeaderBlock;
                    var _class = !string.IsNullOrEmpty(args) && args.StartsWith("collapse") && header.HeaderLevel == 2 ? " class='h2-collapse'" : "";
                    result += $"\r\n<h{header.HeaderLevel} id='{header.ToString().ToId(args?.Replace("collapse", ""))}'{_class}><span class='with-space bd-content-title'>";
                    header.Inlines.ToList().ForEach(p => result += p.ToHtml());
                    result += $"<a class='anchorjs-link ' href='{header.ToString().ToAnchorPoint(args?.Replace("collapse", ""))}' aria-label='Anchor' data-anchorjs-icon='#'></a></span></h{header.HeaderLevel}>";
                    break;
                case MarkdownBlockType.HorizontalRule: result += "\r\n<hr />"; break;
                case MarkdownBlockType.LinkReference:
                    var linkReference = block as LinkReferenceBlock;
                    var url = linkReference.Url ?? "javascript:";
                    var tooltip = string.IsNullOrEmpty(linkReference.Tooltip) ? "" : $" title='{linkReference.Tooltip}'";
                    if(url.IsExternalLink())
                        result += $"<a class='with-space' href='{url}' target='_blank'{tooltip}>{linkReference}</a>";
                    else
                        result += $"<a class='with-space' href='{url.Replace(".md", ".html")}'{tooltip}>{linkReference}</a>";
                    break;
                case MarkdownBlockType.List:
                    var list = block as ListBlock;
                    if (list.Style == ListStyle.Bulleted)
                    {
                        result += "\r\n<ul class='with-space'>";
                        list.Items.ToList().ForEach(l => {
                            result += $"\r\n<li>";
                            l.Blocks.ToList().ForEach(p => result += p.ToHtml("li"));
                            result += $"</li>";
                        });
                        result += "\r\n</ul>";
                    }
                    if (list.Style == ListStyle.Numbered)
                    {
                        result += "\r\n<ol>";
                        list.Items.ToList().ForEach(l => {
                            result += $"\r\n<li>";
                            l.Blocks.ToList().ForEach(p => result += p.ToHtml("li"));
                            result += $"</li>";
                        });
                        result += "\r\n</ol>";
                    }
                    break;
                case MarkdownBlockType.Paragraph:
                    if (args != "li")
                        result += "\r\n<p class='with-space'>";
                    (block as ParagraphBlock).Inlines.ToList().ForEach(p => result += p.ToHtml());
                    if (args != "li")
                        result += "</p>";
                    break;
                case MarkdownBlockType.Quote:
                    var blockQuote = block as QuoteBlock;
                    for (int i = 0; i < blockQuote.Blocks.Count; i++)
                    {
                        if (i == 0)
                        {
                            var type = blockQuote.Blocks[0].ToString().ToUpper().Trim();
                            string style;
                            switch (type)
                            {
                                case "[!NOTE]":
                                case "[!INFO]":
                                case "[!TIP]":
                                    style = " bd-callout-info"; break;
                                case "[!WARNING]":
                                    style = " bd-callout-warning"; break;
                                case "[!IMPORTANT]":
                                case "[!DANGER]":
                                case "[!CAUTION]":
                                    style = " bd-callout-danger"; break;
                                default: style = ""; break;
                            }
                            if (!string.IsNullOrEmpty(style))
                            {
                                result += $"\r\n<blockquote class='bd-callout{style} with-space'>";
                            }
                            else
                            {
                                result += $"\r\n<blockquote class='bd-callout with-space'>";
                                result += blockQuote.Blocks[i].ToHtml();
                            }
                        }
                        else
                        {
                            result += blockQuote.Blocks[i].ToHtml();
                        }
                    }
                    result += "\r\n</blockquote>";
                    break;
                case MarkdownBlockType.Table:
                    result += "\r\n<figure class='with-space'><table class='table table-hover'>";
                    var table = block as TableBlock;
                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        var tr = table.Rows[i];
                        result += i == 0 ? "\r\n<thead>" : "";
                        result += i == 1 ? "\r\n<tbody>" : "";
                        result += "<tr>";
                        for (int j = 0; j < tr.Cells.Count; j++)
                        {
                            var td = tr.Cells[j];
                            var style = string.Empty;
                            if (j < table.ColumnDefinitions.Count)
                            {
                                var align = table.ColumnDefinitions[j].Alignment;
                                style = align == ColumnAlignment.Unspecified ? "" : $" style='text-align:{align.ToString().ToLower()};'";
                            }
                            result += i == 0 ? $"<th{style}>" : $"<td{style}>";
                            td.Inlines.ToList().ForEach(p => result += p.ToHtml());
                            result += i == 0 ? "</th>" : "</td>";
                        }
                        result += "</tr>";
                        result += i == 0 ? "</thead>" : "";
                        result += i == table.Rows.Count - 1 ? "\r\n</tbody>" : "";
                    }
                    result += "\r\n</table></figure>";
                    break;

                case MarkdownBlockType.YamlHeader:
                    var yamlHeader = block as YamlHeaderBlock;
                    result += "\r\n<figure><table class='table table-hover d-none'>";
                    result += "\r\n<thead>\r\n<tr>";
                    yamlHeader.Children.ToList().ForEach(p => result += $"<th>{p.Key}</th>");
                    result += "\r\n</tr>\r\n</thead>";
                    result += "\r\n<tbody>\r\n<tr>";
                    yamlHeader.Children.ToList().ForEach(p => result += $"<td>{p.Value}</td>");
                    result += "\r\n</tr>\r\n</tbody>";
                    result += "\r\n</table></figure>";
                    break;
                case MarkdownBlockType.Root: 
                case MarkdownBlockType.ListItemBuilder:
                    //这两个是啥?
                    throw new NotImplementedException();
                default: break;
            }
            return result;
        }

        /// <summary>
        /// 将行级元素转了 HTML
        /// </summary>
        /// <param name="block">MarkDown 行级元素</param>
        /// <returns>HTML</returns>
        public static string ToHtml(this MarkdownInline inline)
        {
            var result = string.Empty;
            switch (inline.Type)
            {
                case MarkdownInlineType.Comment: result += inline; break;
                case MarkdownInlineType.Bold:
                    result += "<strong>";
                    (inline as BoldTextInline).Inlines.ToList().ForEach(p => result += p.ToHtml());
                    result += "</strong> ";
                    break;
                case MarkdownInlineType.Code: result += $" <code>{HtmlEncode((inline as CodeInline).Text)}</code> "; break;
                case MarkdownInlineType.Image:
                    var image = inline as ImageInline;
                    var imageTooltip = string.IsNullOrEmpty(image.Tooltip) ? "" : $" alt='{image.Tooltip}'";
                    result += $"<img class='d-inline-block img-fluid' data-original='{image.Url.Split(' ')[0]}'{imageTooltip} referrerPolicy='no-referrer' />";
                    break;
                case MarkdownInlineType.Italic:
                    result += "<em>";
                    (inline as ItalicTextInline).Inlines.ToList().ForEach(p => result += p.ToHtml());
                    result += "</em>";
                    break;
                case MarkdownInlineType.MarkdownLink:
                    var markdownLink = inline as MarkdownLinkInline;
                    var markdownLinkUrl = markdownLink.Url ?? "javascript:";
                    var markdownLinkTooltip = string.IsNullOrEmpty(markdownLink.Tooltip) ? "" : $" title='{markdownLink.Tooltip}'";
                    if (markdownLinkUrl.IsExternalLink())
                        result += $"<a href='{markdownLinkUrl}' target='_blank'{markdownLinkTooltip}>";
                    else
                        result += $"<a href='{markdownLinkUrl.Replace(".md", ".html")}'{markdownLinkTooltip}>";
                    markdownLink.Inlines.ToList().ForEach(p => result += p.ToHtml());
                    result += "</a> ";
                    break;
                case MarkdownInlineType.Strikethrough:
                    result += "<del>";
                    (inline as StrikethroughTextInline).Inlines.ToList().ForEach(p => result += p.ToHtml());
                    result += "</del>";
                    break;
                case MarkdownInlineType.Subscript:
                    result += "<sub>";
                    (inline as SubscriptTextInline).Inlines.ToList().ForEach(p => result += p.ToHtml());
                    result += "</sub>";
                    break;
                case MarkdownInlineType.Superscript:
                    result += "<sup>";
                    (inline as SuperscriptTextInline).Inlines.ToList().ForEach(p => result += p.ToHtml());
                    result += "</sup>";
                    break;
                case MarkdownInlineType.TextRun:
                    var reg = new Regex("<(p|img|br|b|i|br|a|link|table|strong|tr|td|th|tbody|em|u|s|del|kbd)\\s*.*?/?>", RegexOptions.IgnoreCase);
                    var textRun = (inline as TextRunInline).ToString().Trim().Replace("&#124;", "|");
                    if (reg.IsMatch(textRun))
                        result += textRun;
                    else
                        result += $"{HtmlEncode(textRun)}";
                    break;
                case MarkdownInlineType.RawHyperlink:
                    var hyperLink = inline as HyperlinkInline;
                    var imgExName = new string[]{ "jpg", "jpeg", "png", "gif" };
                    if (imgExName.ToList().Any(p => hyperLink.Text.Contains(p)))
                    {
                        result += hyperLink.Text;
                    }
                    else
                    {
                        if (hyperLink.Text.IsExternalLink())
                            result += $"<a href='{hyperLink.Text}' target='_blank'>{hyperLink.Text}</a>";
                        else
                            result += $"<a href='{hyperLink.Text.Replace(".md", ".html")}'>{hyperLink.Text}</a>";
                    }
                    break;
                case MarkdownInlineType.Emoji:
                    var emoji = inline as EmojiInline;
                    result += emoji.Text;
                    break;
                case MarkdownInlineType.LinkReference:
                    result += (inline as LinkAnchorInline).Raw;
                    break;
                case MarkdownInlineType.RawSubreddit:
                    //这是啥?
                    throw new NotImplementedException();
                default: break;
            }
            return result;
        }

        /// <summary>
        /// https://highlightjs.org/usage/
        /// </summary>
        public static string ToHlJs(this string input)
        {
            string result;
            switch (input)
            {
                case "c#": result = "csharp"; break;
                case "c++": result = "cpp"; break;
                default: result = input.ToLower(); break;
            }
            return result;
        }

        public static string ToAnchorPoint(this string input, string nacl = null) => $"#{input.ToId(nacl)}";

        public static string ToId(this string input, string nacl = null) => $"{input.Trim()}{nacl}".Sha256().TrimStart('1', '2', '3', '4', '5', '6', '7', '8', '9', '0').Substring(0, 8);

        public static bool IsExternalLink(this string link) => link.StartsWith("http");

        public static string Sha256(this string text)
        {
            string hashString = string.Empty;
            new SHA256Managed().ComputeHash(Encoding.Unicode.GetBytes(text)).ToList().ForEach(p => hashString += String.Format("{0:x2}", p));
            return hashString;
        }
    }
}
