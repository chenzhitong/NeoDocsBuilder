using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace SearchEngine
{
    public class RedirectHtmlMiddleware
    {
        private readonly RequestDelegate _next;

        public RedirectHtmlMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var requestedPath = context.Request.Path.Value;

            // 检查 URL 是否不以 .html 结尾且不以斜杠结尾
            if (!requestedPath.EndsWith(".html") && !requestedPath.EndsWith("/"))
            {
                // 重定向到带 .html 后缀的 URL
                context.Response.Redirect($"{requestedPath}.html", true);
                return; // 终止请求处理
            }

            await _next(context); // 继续处理请求
        }
    }

}
