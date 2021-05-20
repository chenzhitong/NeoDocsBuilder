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
                return $"\r\n<span class='ml-0 my-1 nav-link'>{Name}<i class='fas fa-chevron-right'></i></span>";
            else
                return $"\r\n<a class='ml-0 my-1 nav-link' href='{Href.Replace(".md", ".html")}'>{Name}</a>";
        }
    }
}
