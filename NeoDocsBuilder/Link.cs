namespace NeoDocsBuilder
{
    public class Link
    {
        public string Name;
        public string Href;
        public bool Complete() => !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Href);
        public string ToHtml()
        {
            if (string.IsNullOrEmpty(Href))
                return $"\r\n<span class='ml-0 my-1 nav-link with-space'><i class='fas fa-caret-right'></i>{Name}</span>";
            else
                return $"\r\n<a class='ml-0 my-1 nav-link with-space' href='{Href.Replace(".md", ".html")}'>{Name}</a>";
        }
    }
}
