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

namespace NeoDocsBuilder
{
    public static class MdConverter
    {
        public static string ToHtml(this MarkdownBlock block, string parent = null)
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
                    var headerHtml = string.Empty;
                    result += $"\r\n<h{header.HeaderLevel} class='with-space' id='{{0}}'>";
                    headerHtml += $"\r\n<h{header.HeaderLevel}>";
                    foreach (var headerInline in header.Inlines)
                    {
                        var innerHtml = headerInline.ToHtml();
                        result += innerHtml;
                        headerHtml += innerHtml;
                    }
                    result += $"</h{header.HeaderLevel}>";
                    headerHtml += $"</h{header.HeaderLevel}>";

                    XmlDocument xml = new XmlDocument();
                    xml.LoadXml(headerHtml);
                    result = string.Format(result, xml.InnerText.ToId());

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
                        foreach (var li in list.Items)
                        {
                            result += $"\r\n<li>";
                            foreach (var liBlock in li.Blocks)
                            {
                                result += liBlock.ToHtml("li");
                            }
                            result += $"</li>";
                        }
                        result += "\r\n</ul>";
                    }
                    if (list.Style == ListStyle.Numbered)
                    {
                        result += "\r\n<ol>";
                        foreach (var li in list.Items)
                        {
                            result += $"\r\n<li>";
                            foreach (var liBlock in li.Blocks)
                            {
                                result += liBlock.ToHtml("li");
                            }
                            result += $"</li>";
                        }
                        result += "\r\n</ol>";
                    }
                    break;
                case MarkdownBlockType.Paragraph:
                    if (parent != "li")
                        result += "\r\n<p class='with-space'>";
                    foreach (var inline in (block as ParagraphBlock).Inlines)
                    {
                        result += inline.ToHtml();
                    }
                    if (parent != "li")
                        result += "</p>";
                    break;
                case MarkdownBlockType.Quote:
                    var blockQuote = block as QuoteBlock;
                    for (int i = 0; i < blockQuote.Blocks.Count; i++)
                    {
                        if (i == 0)
                        {
                            var style = string.Empty;
                            var type = blockQuote.Blocks[0].ToString().ToUpper().Trim();
                            switch (type)
                            {
                                case "[!NOTE]":
                                case "[!TIP]":
                                    style = " bd-callout-info"; break;
                                case "[!WARNING]":
                                    style = " bd-callout-warning"; break;
                                case "[!IMPORTANT]":
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
                    result += "\r\n<figure class='with-space'><table class='table table-hover table-striped table-bordered'>";
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
                            foreach (var tdInline in td.Inlines)
                            {
                                result += tdInline.ToHtml();
                            }
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
                    result += "\r\n<figure><table class='table table-hover table-striped table-bordered d-none'>";
                    result += "\r\n<thead>\r\n<tr>";
                    foreach (var item in yamlHeader.Children)
                    {
                        result += $"<th>{item.Key}</th>";
                    }
                    result += "\r\n</tr>\r\n</thead>";
                    result += "\r\n<tbody>\r\n<tr>";
                    foreach (var item in yamlHeader.Children)
                    {
                        result += $"<td>{item.Value}</td>";
                    }
                    result += "\r\n</tr>\r\n</tbody>";
                    result += "\r\n</table></figure>";
                    break;
                case MarkdownBlockType.Root: 
                case MarkdownBlockType.ListItemBuilder: throw new NotImplementedException();
                default: break;
            }
            return result;
        }

        public static string ToHtml(this MarkdownInline inline)
        {
            var result = string.Empty;
            switch (inline.Type)
            {
                case MarkdownInlineType.Comment: result += inline; break;
                case MarkdownInlineType.Bold:
                    result += " <strong>";
                    foreach (var boldInline in (inline as BoldTextInline).Inlines)
                    {
                        result += boldInline.ToHtml();
                    }
                    result += "</strong> ";
                    break;
                case MarkdownInlineType.Code: result += $" <code>{(inline as CodeInline).Text}</code> "; break;
                case MarkdownInlineType.Image:
                    var image = inline as ImageInline;
                    var imageTooltip = string.IsNullOrEmpty(image.Tooltip) ? "" : $" alt='{image.Tooltip}'";
                    if(Config.Lazyload == true)
                        result += $"<img class='img-fluid' data-original='{image.Url.Split(' ')[0]}'{imageTooltip} referrerPolicy='no-referrer' />";
                    else
                        result += $"<img class='img-fluid src='{image.Url.Split(' ')[0]}'{imageTooltip} referrerPolicy='no-referrer' />";
                    break;
                case MarkdownInlineType.Italic:
                    result += "<em>";
                    foreach (var italicInline in (inline as ItalicTextInline).Inlines)
                    {
                        result += italicInline.ToHtml();
                    }
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
                    foreach (var linkInline in markdownLink.Inlines)
                    {
                        result += linkInline.ToHtml();
                    }
                    result += "</a> ";
                    break;
                case MarkdownInlineType.Strikethrough:
                    result += "<del>";
                    foreach (var strikethroughTextInline in (inline as StrikethroughTextInline).Inlines)
                    {
                        result += strikethroughTextInline.ToHtml();
                    }
                    result += "</del>";
                    break;
                case MarkdownInlineType.Subscript:
                    result += "<sub>";
                    foreach (var subscriptTextInline in (inline as SubscriptTextInline).Inlines)
                    {
                        result += subscriptTextInline.ToHtml();
                    }
                    result += "</sub>";
                    break;
                case MarkdownInlineType.Superscript:
                    result += "<sup>";
                    foreach (var superscriptTextInline in (inline as SuperscriptTextInline).Inlines)
                    {
                        result += superscriptTextInline.ToHtml();
                    }
                    result += "</sup>";
                    break;
                case MarkdownInlineType.TextRun:
                    var reg = new Regex("<(p|img|br|b|i|br|a|link)(\\s|>).*>");
                    var textRun = (inline as TextRunInline).ToString().Trim();
                    if(reg.IsMatch(textRun))
                        result += $"{textRun}";
                    else
                        result += $" {HtmlEncode(textRun)} ";
                    break;
                case MarkdownInlineType.RawHyperlink:
                    var hyperLink = inline as HyperlinkInline;
                    var imgExName = new string[]{ "jpg", "jpeg", "png", "gif" };
                    if (imgExName.ToList().Any(p => hyperLink.Text.Contains(p)))
                    {
                        result += $" {hyperLink.Text} ";
                    }
                    else
                    {
                        if (hyperLink.Text.IsExternalLink())
                            result += $" <a href='{hyperLink.Text}' target='_blank'>{hyperLink.Text}</a> ";
                        else
                            result += $" <a href='{hyperLink.Text.Replace(".md", ".html")}'>{hyperLink.Text}</a> ";
                    }
                    break;
                case MarkdownInlineType.Emoji:
                    var emoji = inline as EmojiInline;
                    result += emoji.Text;
                    break;
                case MarkdownInlineType.RawSubreddit:
                case MarkdownInlineType.LinkReference:
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

        public static string ToAnchorPoint(this string input) => $"#{input.ToId()}";

        public static string ToId(this string input) => $"{input.Trim(' ', '*').Replace(" ", "")}";

        public static bool IsExternalLink(this string link) => link.StartsWith("http");
    }
}
