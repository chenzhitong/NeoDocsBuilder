using Microsoft.Toolkit.Parsers.Markdown;
using Microsoft.Toolkit.Parsers.Markdown.Blocks;
using Microsoft.Toolkit.Parsers.Markdown.Inlines;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using static System.Web.HttpUtility;

namespace NeoDocsBuilder
{
    public static class MdConverter
    {
        /// <summary>
        /// 将块级元素转了 HTML
        /// </summary>
        /// <param name="block">MarkDown 块级元素</param>
        /// <returns>HTML</returns>
        public static string ToHtml(this MarkdownBlock block, string file, string innerText, bool isCollapse = false, int? anchroPointCount = null)
        {
            var result = string.Empty;
            switch (block.Type)
            {
                case MarkdownBlockType.Code:
                    var code = block as CodeBlock;
                    var lang = string.IsNullOrEmpty(code.CodeLanguage) ? "" : $" class='{code.CodeLanguage.ToHlJs()}' lang='{HtmlEncode(code.CodeLanguage.ToHlJs())}'";
                    var encode = HtmlEncode(code.Text);
                    result += $"\r\n<figure class='highlight'>\r\n<button type='button' class='btn-showall' onclick='showAll(this)'>Show all</button><button type='button' class='btn-clipboard' data-original-title='Copy to clipboard' data-clipboard-action='copy' data-toggle='tooltip' data-placement='top' title='Copy to clipboard' data-clipboard-text='{encode}'>Copy</button><pre><code{lang}>{encode}\r\n</code></pre>\r\n</figure>";
                    break;
                case MarkdownBlockType.Header:
                    var header = block as HeaderBlock;
                    var _class = isCollapse && header.HeaderLevel == 2 ? " class='h2-collapse'" : "";
                    result += $"\r\n<h{header.HeaderLevel} id='{innerText.ToId(anchroPointCount)}'{_class}><span class='with-space bd-content-title'>";
                    header.Inlines.ToList().ForEach(p => result += p.ToHtml(file));
                    result += $"<a class='anchorjs-link' href='{innerText.ToAnchorPoint(anchroPointCount)}' aria-label='Anchor' data-anchorjs-icon='#'></a></span></h{header.HeaderLevel}>";
                    break;
                case MarkdownBlockType.HorizontalRule: result += "\r\n<hr />"; break;
                case MarkdownBlockType.LinkReference:
                    var linkReference = block as LinkReferenceBlock;
                    var url = linkReference.Url ?? "javascript:";
                    var tooltip = string.IsNullOrEmpty(linkReference.Tooltip) ? "" : $" title='{linkReference.Tooltip}'";
                    if(url.IsExternalLink())
                        result += $" <a class='with-space' href='{url}' target='_blank'{tooltip}>{linkReference}</a> ";
                    else
                        result += $" <a class='with-space' href='{url.Replace(".md", ".html")}'{tooltip}>{linkReference}</a> ";
                    LinkCheck(file, url);
                    break;
                case MarkdownBlockType.List:
                    var list = block as ListBlock;
                    if (list.Style == ListStyle.Bulleted)
                    {
                        result += "\r\n<ul class='with-space'>";
                        list.Items.ToList().ForEach(l => {
                            result += $"\r\n<li>";
                            l.Blocks.ToList().ForEach(p => result += p.ToHtml(file, string.Empty));
                            result += $"</li>";
                        });
                        result += "\r\n</ul>";
                    }
                    if (list.Style == ListStyle.Numbered)
                    {
                        result += "\r\n<ol>";
                        list.Items.ToList().ForEach(l => {
                            result += $"\r\n<li>";
                            l.Blocks.ToList().ForEach(p => result += p.ToHtml(file, string.Empty));
                            result += $"</li>";
                        });
                        result += "\r\n</ol>";
                    }
                    break;
                case MarkdownBlockType.Paragraph:
                    result += "\r\n<p class='with-space'>";
                    (block as ParagraphBlock).Inlines.ToList().ForEach(p => result += p.ToHtml(file));
                    result += "</p>";
                    break;
                case MarkdownBlockType.Quote:
                    var blockQuote = block as QuoteBlock;
                    for (int i = 0; i < blockQuote.Blocks.Count; i++)
                    {
                        if (i == 0)
                        {
                            var type = blockQuote.Blocks[0].ToString().ToUpper().Trim();
                            string style = type switch
                            {
                                "[!NOTE]" or "[!INFO]" or "[!TIP]" => " bd-callout-info",
                                "[!WARNING]" => " bd-callout-warning",
                                "[!IMPORTANT]" or "[!DANGER]" or "[!CAUTION]" => " bd-callout-danger",
                                _ => "",
                            };
                            if (!string.IsNullOrEmpty(style))
                            {
                                result += $"\r\n<blockquote class='bd-callout{style} with-space'>";
                            }
                            else
                            {
                                result += $"\r\n<blockquote class='bd-callout with-space'>";
                                result += blockQuote.Blocks[i].ToHtml(file, string.Empty);
                            }
                        }
                        else
                        {
                            result += blockQuote.Blocks[i].ToHtml(file, string.Empty);
                        }
                    }
                    result += "\r\n</blockquote>";
                    break;
                case MarkdownBlockType.Table:
                    result += "\r\n<figure class='with-space'><table class='table'>";
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
                            td.Inlines.ToList().ForEach(p => result += p.ToHtml(file));
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
        public static string ToHtml(this MarkdownInline inline, string file)
        {
            var result = string.Empty;
            var text1 = inline.ToString();
            switch (inline.Type)
            {
                case MarkdownInlineType.Comment: result += inline; break;
                case MarkdownInlineType.Bold:
                    result += " <strong>";
                    (inline as BoldTextInline).Inlines.ToList().ForEach(p => result += p.ToHtml(file));
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
                    (inline as ItalicTextInline).Inlines.ToList().ForEach(p => result += p.ToHtml(file));
                    result += "</em>";
                    break;
                case MarkdownInlineType.MarkdownLink:
                    var markdownLink = inline as MarkdownLinkInline;
                    var markdownLinkUrl = markdownLink.Url ?? "javascript:";
                    var markdownLinkTooltip = string.IsNullOrEmpty(markdownLink.Tooltip) ? "" : $" title='{markdownLink.Tooltip}'";

                    if (markdownLinkUrl.IsExternalLink())
                        result += $" <a href='{markdownLinkUrl}' target='_blank'{markdownLinkTooltip}>";
                    else
                        result += $" <a href='{markdownLinkUrl.Replace(".md", ".html")}'{markdownLinkTooltip}>";
                    LinkCheck(file, markdownLinkUrl);
                    markdownLink.Inlines.ToList().ForEach(p => result += p.ToHtml(file));
                    result += "</a> ";
                    break;
                case MarkdownInlineType.Strikethrough:
                    result += "<del>";
                    (inline as StrikethroughTextInline).Inlines.ToList().ForEach(p => result += p.ToHtml(file));
                    result += "</del>";
                    break;
                case MarkdownInlineType.Subscript:
                    result += "<sub>";
                    (inline as SubscriptTextInline).Inlines.ToList().ForEach(p => result += p.ToHtml(file));
                    result += "</sub>";
                    break;
                case MarkdownInlineType.Superscript:
                    result += "<sup>";
                    (inline as SuperscriptTextInline).Inlines.ToList().ForEach(p => result += p.ToHtml(file));
                    result += "</sup>";
                    break;
                case MarkdownInlineType.TextRun:
                    var text = inline.ToString();
                    if (text.StartsWith(":::"))
                    {
                        if (text.StartsWith(":::note") || text.StartsWith(":::info"))
                        {
                            result += $"\r\n<blockquote class='bd-callout bd-callout-info with-space'>";
                        }
                        else if (text.StartsWith(":::warning"))
                        {
                            result += $"\r\n<blockquote class='bd-callout bd-callout-warning with-space'>";
                        }
                        else if (text.StartsWith(":::danger") || text.StartsWith(":::important") || text.StartsWith(":::caution"))
                        {
                            result += $"\r\n<blockquote class='bd-callout bd-callout-danger with-space'>";
                        }
                        else
                        {
                            result += "\r\n</blockquote>";
                        }
                    }
                    var reg = new Regex("\\s*</?(div|p|img|br|b|i|br|a|link|table|strong|tr|td|th|tbody|em|u|s|del|kbd)(\\W+|(\\s+.*?/?>))", RegexOptions.IgnoreCase);
                    var textRun = (inline as TextRunInline).ToString().Replace("&#124;", "|").Replace(":::note", "").Replace(":::info", "")
                        .Replace(":::warning", "").Replace(":::danger", "").Replace(":::important", "").Replace(":::caution", "").Replace(":::", "").Trim();
                    if (reg.IsMatch(textRun))
                        result += textRun;
                    else
                        result += $"{HtmlEncode(textRun)}";
                    if (text.EndsWith(":::"))
                    {
                        result += "\r\n</blockquote>";
                    }
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
                            result += $" <a href='{hyperLink.Text}' target='_blank'>{hyperLink.Text}</a> ";
                        else
                        {
                            result += $" <a href='{hyperLink.Text.Replace(".md", ".html")}'>{hyperLink.Text}</a> ";
                            LinkCheck(file, hyperLink.Text);
                        }
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
            return input switch
            {
                "c#" => "csharp",
                "c++" => "cpp",
                _ => input.ToLower(),
            };
        }

        public static string ToAnchorPoint(this string input, int? count) => $"#{input.ToId(count)}";

        public static string ToId(this string input, int? count)
        {
            var id = input.Trim();
            if (count != null && count > 0)
            {
                id = $"{id}-{count}";
            }
            var encodeList = "~`!@#$%^&*()+=[]\\{}|;':\",./<>?！（）；‘’：“”《》？，。";
            encodeList.ToList().ForEach(p => id = id.Replace(p.ToString(), ""));
            return id.Replace(" ", "-").ToLower();
        }

        public static bool IsExternalLink(this string link) => link.StartsWith("http");

        public static readonly List<string> ErrorLogs = new();

        private static void LinkCheck(string pathBase, string link)
        {
            
            var fullPath = Path.GetFullPath(pathBase);
            var fullLink = Path.GetFullPath(!link.StartsWith("/") ? "../" + link : link, fullPath);
            if (Path.GetExtension(fullLink) != ".md") return;
            if (link.StartsWith("http") || File.Exists(UrlDecode(fullLink)))
            {
                return;
            }
            else
            {
                ErrorLogs.Add($"Error link: {pathBase} ({link})");
                return;
            }
        }
    }
}
