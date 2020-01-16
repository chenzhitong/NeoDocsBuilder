//显示主题
if (localStorage.getItem("theme") === "dark") {
    $("html").addClass("theme-dark");
}
else {
    $("html").removeClass("theme-dark");
}
//为中文和英文之间添加空格
text_replace(".with-space");
//代码高亮
hljs.initHighlightingOnLoad();
//目录展开和折叠
$(function () {
    $(".catalog nav span").click(function () {
        var currentExpand = $(this).hasClass('expand');
        //折叠所有旁系目录
        $(".catalog nav nav").not($(this).parents("nav")).not($(this).children("nav")).not($(this).next("nav")).hide();
        $(".catalog nav span").removeClass('expand');
        //展开所有父目录
        $(this).parents("nav").prev().addClass('expand');
        $(this).parents("nav").show("fast");
        //折叠所有子目录
        $(this).children("nav").prev().removeClass('expand');
        $(this).children("nav").hide("fast");
        //展开或折叠当前目录
        if (currentExpand) {
            $(this).removeClass('expand');
            $(this).next("nav").hide("fast");
        } else {
            $(this).addClass('expand');
            $(this).next("nav").show("fast");
        }
    });
});
//根据网址自动展开到对应目录
var cachedOnload = window.onload;
window.onload = function () {
    if (cachedOnload) {
        cachedOnload();
    }
    var pathName = decodeURI(location.pathname);
    var link = $(".catalog").find("[href='" + pathName + "']")[0];
    $(link).addClass("active");
    $(link).parents("nav").show();
    setTimeout(function(){ //为 less 编译预留时间
        $(link).parents("nav").show();
    }, 200);
    $(link).parents().prev().addClass('expand');
    //导航栏高亮
    var href = pathName.split('/')[1];
    $(".navbar-nav [href='/" + href + "/index.html']").each(function () {
        $(this).addClass("active");
    });
};

//懒加载
$(function () {
    $('[data-original]').lazyload({
        threshold: 200,
        effect: "fadeIn"
    });
});
//代码复制
$(function () {
    var clipboard = new ClipboardJS('.btn-clipboard');
    clipboard.on('success', function (t) {
        $(t.trigger).attr("title", "Copied!").tooltip("_fixTitle").tooltip("show").attr("title", "Copy to clipboard").tooltip("_fixTitle");
    });
    clipboard.on("error", function (t) {
        var e = /Mac/i.test(navigator.userAgent) ? "⌘" : "Ctrl-";
        var n = "Press " + e + "C to copy";

        $(t.trigger).attr("title", n).tooltip("_fixTitle").tooltip("show").attr("title", "Copy to clipboard").tooltip("_fixTitle");
    });
});
//语言切换
function language(lang) {
    var rgExp = /\/\w{2}-\w{2}\//;
    localStorage.setItem("lang", lang);
    if (location.href.search(rgExp) >= 0) {
        location.href = location.href.replace(rgExp, '/' + lang + '/');
    }
}
//工具提示
$('[data-toggle="tooltip"]').tooltip();
//小屏时显示隐藏目录
function showCatalog() {
    if ($('.catalog').hasClass('show'))
        $('.catalog').removeClass('show');
    else
        $('.catalog').addClass('show');
}
//为块引用添加图标
$(function () {
    $(".bd-callout-info h4").prepend("<i class='fas fa-info-circle'></i>");
    $(".bd-callout-warning h4").prepend("<i class='fas fa-exclamation-circle'></i>");
    $(".bd-callout-danger h4").prepend("<i class='fas fa-exclamation-triangle'></i>");
});

//滚动到底部显示页脚
function showFooter()
{
    if ($(document).height() - ($(window).scrollTop() + $(window).height()) < 1) {
        $("footer").attr("style", "display:flex");
    }
    else {
        $("footer").attr("style", "display:none");
    }
}
setTimeout(showFooter,1000);
$(window).scroll(showFooter);

//关灯/开灯
function turnOff() {
    if (localStorage.getItem("theme") !== "dark") {
        $("html").addClass("theme-dark");
        localStorage.setItem("theme", "dark");
    }
    else {
        $("html").removeClass("theme-dark");
        localStorage.setItem("theme", "light");
    }
}

//Google Analyse
window.dataLayer = window.dataLayer || [];
function gtag() { dataLayer.push(arguments); }
gtag('js', new Date());
gtag('config', 'UA-130525731-2');

//设置内容图片最大宽度为 min(100%, 700px)
var resize = window.onresize;
window.onresize = function () {
    if (resize) resize();
    setMaxWidth();
};
setMaxWidth();
function setMaxWidth() {
    $("main img").css("max-width", Math.min(700, $("main").width()));
}