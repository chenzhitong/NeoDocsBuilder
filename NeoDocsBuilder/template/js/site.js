//显示主题
if (localStorage.getItem("theme") === "dark") {
    $("html").addClass("theme-dark");
    $(".btn-light").removeClass("btn-light").addClass("btn-dark");
}
else {
    $("html").removeClass("theme-dark");
    $(".btn-dark").removeClass("btn-dark").addClass("btn-light");
}
//为中文和英文之间添加空格
text_replace(".with-space");
//代码高亮
$(function () {
    hljs.initHighlightingOnLoad();
});
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
    var allLinks = $(".catalog a");
    var link = null;
    // 遍历所有链接，找到与 pathName 匹配的链接
    for (var i = 0; i < allLinks.length; i++) {
        if ($(allLinks[i]).attr("href").toLowerCase() == pathName.toLowerCase()) {
            link = allLinks[i];
            break;
        }
    }
    if (link) {
        $(link).addClass("active");
        $(link).parent("span").addClass("active");
        $(link).parents("nav").show();
        $(link).parent().next("nav").show();
        $(link).parents("span").addClass('expand');
        $(link).parents().prev().addClass('expand');
    }
    //上一页下一页
    var allLinks = $(".catalog a");
    for (var i = 0; i < allLinks.length; i++) {
        if ($(allLinks[i]).attr("href").toLowerCase() == pathName.toLowerCase()) {
            if (i != 0) {
                $("#prevPage").show();
                $("#prevPage .prevText").text($(allLinks[i - 1]).text());
                $("#prevPage").attr("href", $(allLinks[i - 1]).attr("href"));
            }
            if (i != allLinks.length - 1) {
                $("#nextPage").show();
                $("#nextPage .nextText").text($(allLinks[i + 1]).text());
                $("#nextPage").attr("href", $(allLinks[i + 1]).attr("href"));
            }
        }
    }
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
    localStorage.setItem("lang", lang);
    if(lang == "zh")
    {
        location.href = location.href.replace(window.location.host, window.location.host + "/zh");
    }
    else
    {
        location.href = location.href.replace(window.location.host + "/zh", window.location.host);
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
    $(".bd-callout-info").prepend("<img src='/img/info.svg' /> <p>NOTE</p>");
    $(".bd-callout-warning").prepend("<img src='/img/warning.svg' /> <p>WARNING</p>");
    $(".bd-callout-danger").prepend("<img src='/img/danger.svg' /> <p>DANGER</p>");
    $(".bd-callout").not(".bd-callout-info").not(".bd-callout-warning").not(".bd-callout-danger").prepend("<img src='/img/callout.svg'/>");
});

//关灯/开灯
function turnOff() {
    if (localStorage.getItem("theme") !== "dark") {
        $("html").addClass("theme-dark");
        localStorage.setItem("theme", "dark");
        $(".btn-light").removeClass("btn-light").addClass("btn-dark");
    }
    else {
        $("html").removeClass("theme-dark");
        localStorage.setItem("theme", "light");
        $(".btn-dark").removeClass("btn-dark").addClass("btn-light");
    }
}
$("#navbar-sidenav .nav-link").each(function () {
    if ($(this).next("nav").length > 0) {
        $(this).removeClass("nav-link")
    }
});
//Google Analyse
window.dataLayer = window.dataLayer || [];
function gtag() { dataLayer.push(arguments); }
gtag('js', new Date());
gtag('config', 'UA-130525731-2');

//设置内容图片最大宽度为 min(100%, 700px)
function setMaxWidth() {
    $("main img").each(function () {
        var parentWidth = $(this).parent().width();
        if (parentWidth > 100)
            $(this).css("max-width", Math.min(700, $(this).parent().width()));
    });
}
var resize = window.onresize;
window.onresize = function () {
    if (resize) resize();
    setTimeout(setMaxWidth, 300);
};
setMaxWidth();

$("#sInput").bind({
    focus: function () {
        searchBar();
    },
    keyup: function (e) {
        clearTimeout();
        if (e.which == 13) {
            searchBar();
        }
        setTimeout(searchBar, 300);
    },
    paste: function () {
        searchBar();
    }
});

$(".search-de").click(function () {
    $("#sInput").val("");
    $("#sResult").html("");
})

var url = window.location.origin;

function searchBar() {
    var k = $("#sInput").val();
    var l = localStorage.getItem("lang") || navigator.language || "en-us";
    if (!k) $("#sResult").html("");
    $.ajax({
        type: "GET",
        url: url + "/?k=" + encodeURIComponent(k) + "&l=" + encodeURIComponent(l),
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        success: function (data) {
            var html = ""

            if (data.length == 0) {
                html += "<li><a><span> No results found. </span></a></li > ";
            }
            data.forEach(function (v) {
                html += "<li><a href=" + v.Link + "><strong>" + v.Title + "</strong><br />";
                html += "<span>" + v.Line + "</span></a></li>";
            });
            $("#sResult").html(html);
        },
        fail: function () {
            alert("fail");
        }
    });
}
//版本切换
$(function () {
    if (location.href.indexOf("/v2/") > 0) {
        $(".n2").addClass("active");
        $(".navbar-nav .nav-link").each(function () {
            if ($(this).attr("href").indexOf(".html") > 0)
                $(this).attr("href", "/v2" + $(this).attr("href"));
        });
    }
    else {
        $(".n3").addClass("active");
    }
});

function showAll(obj) {
    $(obj).parent().find("code").css("max-height", "initial");
}

//Only for Neo docs homepage
$("#sInput2").bind({
    focus: function () {
        searchBar2();
    },
    keyup: function (e) {
        clearTimeout();
        if (e.which == 13) {
            searchBar2();
        }
        setTimeout(searchBar2, 300);
    },
    paste: function () {
        searchBar2();
    }
});
//Only for Neo docs homepage
$(".search-de2").click(function () {
    $("#sInput2").val("");
    $("#sResult2").html("");
})
//Only for Neo docs homepage
function searchBar2() {
    var k = $("#sInput2").val();
    var l = localStorage.getItem("lang") || navigator.language || "en-us";
    if (!k) $("#sResult2").html("");
    $.ajax({
        type: "GET",
        url: url + "/?k=" + encodeURIComponent(k) + "&l=" + encodeURIComponent(l),
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        success: function (data) {
            var html = ""

            if (data.length == 0) {
                html += "<li><a><span> No results found. </span></a></li > ";
            }
            data.forEach(function(v) {
                html += "<li><a href=" + v.Link + "><strong>" + v.Title + "</strong><br />";
                html += "<span>" + v.Line + "</span></a></li>";
            });
            $("#sResult2").html(html);
        },
        fail: function () {
            alert("fail");
        }
    });
}