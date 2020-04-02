# NeoDocsBuilder 

NeoDocsBuilder —— 一个超牛逼的 MarkDown 转网站的工具，包括解析、编译、样式处理以及高度定制化功能

| 周期 | 功能                             | 状态   | 说明                                                         | 开源库                                 |
| ---- | -------------------------------- | ------ | ------------------------------------------------------------ | -------------------------------------- |
| 1    | MarkDown 解析                    | 已完成 | 对 MarkDown 文档进行解析                                     | Microsoft.Toolkit.Parsers.Markdown     |
| 1    | MarkDown - HTML 的转换           | 已完成 | 将已解析的 MarkDown 转换为 HTML                              |                                        |
| 1    | 多文档自动转换                   | 已完成 | 针对一个文件夹内的多级目录以  及多个 .md 文件自动生成 HTML   |                                        |
| 1    | Bootstrap4 集成                  | 已完成 | 生成带 Bootstrap4 样式的 HTML                                | Bootstrap                              |
| 1    | 自动目录生成 `已弃用`            | 已完成 | 根据目录结构生成文档目录。参见：[auto-generate-catalog-orderd-by-name](https://github.com/chenzhitong/NeoDocsBuilder/tree/auto-generate-catalog-orderd-by-name) 分支 |                                        |
| 1    | 目录中文件夹的自定义配置`已弃用` | 已完成 | 配置 folder.json 实现文件夹的重命名和隐藏                    |                                        |
| 1    | 手动目录生成 `新`                | 已完成 | 根据 toc.yml 生成目录                                        |                                        |
| 1    | 网站模板                         | 已完成 | 开发 docs.neo.org 的前端页面的模板                           |                                        |
| 1    | 自适应 HTML 编码                 | 已完成 | 根据 MarkDown 中的标签内容，进行正确的 HtmlEncode，  以使某些标签按文本显示，某些标签按代码显示 |                                        |
| 1    | 动态目录生成 `已弃用`            | 已完成 | 为每篇文档生成不同的相对路径的目录，  以兼容 [file://](file:///) 协议 |                                        |
|      | Less 支持 `新`                   | 已完成 | 用 Less 重写 CSS 以快速更改主题色，与 files:// 协议不兼容    |                                        |
| 1    | 文章摘要和锚点的自动生成         | 已完成 | 为每篇文档自动生成右侧的章节列表，  点击可进行文档内的跳转   |                                        |
| 1    | 滚动监听                         | 已完成 | 右侧章节列表的滚动监听，文章滚动到某个章节，右侧章节列表高亮显示 | bootstrap - scrollspy                  |
| 1    | 当前位置定位，及目录高亮         | 已完成 | 对当前阅读的文档和标题进行定位，在左侧目录和右侧摘要处高亮显示 |                                        |
| 1    | 懒加载                           | 已完成 | 对图片进行懒加载                                             | jquey.lazyload                         |
| 1    | 代码高亮                         | 已完成 | 对代码进行高亮显示                                           | highlight.js  Visual Studio-like style |
| 1    | 针对标题进行折叠展开             | 已完成 | 适用于 FAQ 之类的大量需要折叠的内容                          |                                        |
| 1    | 多语言切换                       | 已完成 | 网站多语言切换以及内容的多语言切换                           |                                        |
| 1    | GitHub 链接                      | 已完成 | 对每篇文档添加对应的 GitHub 链接                             |                                        |
| 2    | 死链检测                         | 已完成 | 对所引用的 .md 做死链检测                                    |                                        |
| 2    | 全局搜索                         | 已完成 | 对文档内容进行全文搜索（独立部署）                           |                                        |
| 2    | 代码片段复制                     | 已完成 | 一键复制文档中的代码片断                                     | clipboard.js                           |
| 2    | 版本切换                         | 已完成 | 在网站中可以设置版本，并且支持切换                           |                                        |
| 2    | 多主题切换                       | 已完成 | 支持自定义主题，如深色模式                                   |                                        |
| 2    | Tab 页支持                       | 未完成 | 为 MarkDown 支持页面内的 Tabs                                |                                        |
| 2    | 反馈建议                         | 未完成 | 支持文档的打分和反馈（以及时知道对开发者是否有帮助）         |                                        |

## 运行

安装 [.NET Core Runtime](https://dotnet.microsoft.com/download)

然后进入程序目录，启动命令行，运行

```powershell
dotnet NeoDocsBuilder.dll
```

或

```powershell
dotnet NeoDocsBuilder.dll config.json
```

## 配置文件说明

**config.json** 

origin：存储 MarkDown 文件的文件夹，作为编译的输入

template：存储网站模板的文件夹，作为编译时候的模板

destination：存储编译结果的文件夹，作为编译的输出

webRoot：网站根据目录，程序会将 template 中的内容（如 CSS，JS）复制到网站根目录

```json
{
  "ApplicationConfiguration": [
    {
      "origin": "origin\\zh-cn",
      "template": "template",
      "destination": "wwwroot\\zh-cn",
      "catalog": "docs\\zh-cn\\toc.yml",
      "webRoot": "wwwroot",
      "git": "https://github.com/neo-project/docs/blob/master/zh-cn/"
    }
  ]
}
```

也可以这么写，以支持多语言和多目录：

```json
{
  "ApplicationConfiguration": [
    {
      "origin": "docs\\zh-cn",
      "template": "template",
      "destination": "wwwroot\\docs\\zh-cn",
      "catalog": "docs\\zh-cn\\toc.yml",
      "webRoot": "wwwroot",
      "git": "https://github.com/neo-project/docs/blob/master/docs/zh-cn/"
    },
    {
      "origin": "docs\\en-us",
      "template": "template",
      "destination": "wwwroot\\docs\\en-us",
      "catalog": "docs\\en-us\\toc.yml",
      "webRoot": "wwwroot",
      "git": "https://github.com/neo-project/docs/blob/master/docs/en-us/"
    },
    {
      "origin": "faq\\zh-cn",
      "template": "template",
      "destination": "wwwroot\\faq\\zh-cn",
      "catalog": "faq\\zh-cn\\toc.yml",
      "webRoot": "wwwroot",
      "git": "https://github.com/neo-project/docs/blob/master/faq/zh-cn/"
    },
    {
      "origin": "faq\\en-us",
      "template": "template",
      "destination": "wwwroot\\faq\\en-us",
      "catalog": "faq\\en-us\\toc.yml",
      "webRoot": "wwwroot",
      "git": "https://github.com/neo-project/docs/blob/master/faq/en-us/"
    }
  ]
}
```

如果支持多语言的话，可以在 `wwwroot` 目录添加以下文件，从而进行自动跳转，也可以将该文件复制到 `template` 中，每次编译后自动复制到网站根目录。

**index.html**

```html
<!DOCTYPE html>
<html>

<head>
    <meta charset="utf-8">
    <meta http-equiv="X-UA-Compatible" content="IE=edge,chrome=1">
    <title> </title>
    <meta name="viewport" content="width=device-width">
    <meta name="title" content="">
    <meta name="generator" content="NeoDocsBuilder">
</head>

<body>
    <script>
        var savelang = localStorage.getItem("lang");
        var lang = !!savelang ? savelang : (navigator.language || navigator.browserLanguage).toLowerCase();
        if (lang != 'zh-cn') lang = 'en-us';
        location.href = "/docs/" + lang + "/index.html"; //此处要根据网站目录结构进行修改
    </script>
</body>

</html>
```

**folder.json**

如果配置该文件，请将其放在 config.json 中配置的 origin 文件夹中，如上述例子中的 `faq\\zh-cn`

collapse：生成文档内容时，对二级标题下的所有内容进行折叠，单击二级标题时展开内容，适用于大量需要折叠的内容，如 FAQ。

```json
{
  "collapse":[
    "basic.md",
    "client.md",
    "dev.md",
    "sc.md"
  ]
}
```

## 配置 SearchEngine

SearchEngine 是一个 ASP.NET Core 3.0 的网站，为文档提供后端的搜索功能，部署方式如下：

1、发布 SearchEngine 项目，生成如下文件

```
appsettings.Development.json
appsettings.json
Newtonsoft.Json.dll
SearchEngine.deps.json
SearchEngine.dll
SearchEngine.exe
SearchEngine.pdb
SearchEngine.runtimeconfig.json
web.config
```

2、发布 NeoDocsBuilder 项目，生成如下文件

```
template
config.json
Microsoft.Toolkit.dll
Microsoft.Toolkit.Parsers.dll
NeoDocsBuilder.deps.json
NeoDocsBuilder.dll
NeoDocsBuilder.exe
NeoDocsBuilder.pdb
NeoDocsBuilder.runtimeconfig.json
Newtonsoft.Json.dll
```

3、将文档项目（如  https://github.com/neo-project/docs ）和前两步生成的文件放到一起

4、运行 NeoDocsBuilder.exe 编译文档，最终的文件如下图所示

```
articles
docs
faq
template
tutorial
wwwroot
appsettings.Development.json
appsettings.json
config.json
log.txt
Microsoft.Toolkit.dll
Microsoft.Toolkit.Parsers.dll
NeoDocsBuilder.deps.json
NeoDocsBuilder.dll
NeoDocsBuilder.exe
NeoDocsBuilder.pdb
NeoDocsBuilder.runtimeconfig.json
Newtonsoft.Json.dll
SearchEngine.deps.json
SearchEngine.dll
SearchEngine.exe
SearchEngine.pdb
SearchEngine.runtimeconfig.json
web.config
```

5、运行 SearchEngine 网站，可以在 IIS 中运行，也可以在本地直接运行 SearchEngine.exe。

需要安装 [.NET Core 3.0 Runtime & Hosting Bundle](https://dotnet.microsoft.com/download/dotnet-core/3.0/runtime)。