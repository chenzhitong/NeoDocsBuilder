using Microsoft.Toolkit.Parsers.Markdown;
using Microsoft.Toolkit.Parsers.Markdown.Blocks;
using Microsoft.Toolkit.Parsers.Markdown.Inlines;
using static System.Web.HttpUtility;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

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
                    result += $"\r\n<figure class='highlight'>\r\n<pre><code{lang} data-author-content='{encode}'>{encode}\r\n</code></pre>\r\n</figure>";
                    break;
                case MarkdownBlockType.Header:
                    var header = block as HeaderBlock;
                    result += $"\r\n<h{header.HeaderLevel} id='{header.ToString().ToId()}'>";
                    foreach (var headerInline in header.Inlines)
                    {
                        result += headerInline.ToHtml();
                    }
                    result += $"</h{header.HeaderLevel}>";
                    break;
                case MarkdownBlockType.HorizontalRule: result += "\r\n<hr />"; break;
                case MarkdownBlockType.LinkReference:
                    var linkReference = block as LinkReferenceBlock;
                    var url = linkReference.Url ?? "javascript:";
                    var tooltip = string.IsNullOrEmpty(linkReference.Tooltip) ? "" : $" title='{linkReference.Tooltip}'";
                    result += $"<a href='{url}'{tooltip}>{linkReference}</a>";
                    break;
                case MarkdownBlockType.List:
                    var list = block as ListBlock;
                    if (list.Style == ListStyle.Bulleted)
                    {
                        result += "\r\n<ul>";
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
                        result += "\r\n<p>";
                    foreach (var inline in (block as ParagraphBlock).Inlines)
                    {
                        result += inline.ToHtml();
                    }
                    if (parent != "li")
                        result += "</p>";
                    break;
                case MarkdownBlockType.Quote:
                    result += "\r\n<blockquote>";
                    foreach (var quoteBlock in (block as QuoteBlock).Blocks)
                    {
                        result += quoteBlock.ToHtml();
                    }
                    result += "\r\n</blockquote>";
                    break;
                case MarkdownBlockType.Table:
                    result += "\r\n<figure><table class='table table-hover table-striped table-bordered'>";
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

                case MarkdownBlockType.Root: 
                case MarkdownBlockType.YamlHeader:
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
                        result += $"<img data-original='{image.Url.Split(' ')[0]}'{imageTooltip} referrerPolicy='no-referrer' />";
                    else
                        result += $"<img src='{image.Url.Split(' ')[0]}'{imageTooltip} referrerPolicy='no-referrer' />";
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
                    result += $" <a href='{markdownLinkUrl}'{markdownLinkTooltip}>";
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
                case MarkdownInlineType.TextRun: result += $"{(inline as TextRunInline).ToString().Trim()}"; break;
                case MarkdownInlineType.RawHyperlink:
                    var hyperLink = inline as HyperlinkInline;
                    var imgExName = new string[]{ "jpg", "jpeg", "png", "gif" };
                    if (imgExName.ToList().Any(p => hyperLink.Text.Contains(p)))
                    {
                        result += $" {hyperLink.Text} ";
                    }
                    else
                    {
                        result += $" <a href='{hyperLink.Text}'>{hyperLink.Text}</a> ";
                    }
                    break;

                case MarkdownInlineType.RawSubreddit:
                case MarkdownInlineType.Emoji:
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

        public static string ToId(this string input) => $"{input.Trim(' ', '*').Replace(" ", "-")}";
    }
}
