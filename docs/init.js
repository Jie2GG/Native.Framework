var rootUrl = 'https://native.run/docs';

var versions = [
    { title: 'v1.0', path: '/1.0/' }
]

var latestVersionIndex = 0;
var currentVersionIndex = -1;

versions[latestVersionIndex].title += ' (latest)';

versions.forEach(function (v, idx) {
    if (v.path) {
        var pathname = window.location.pathname;
        if (pathname.lastIndexOf(v.path) == pathname.length - v.path.length) {
            currentVersionIndex = idx;
            rootUrl = window.location.origin + pathname.substr(0, pathname.length - v.path.length);
        }
    }
});

versions.forEach(function (v) {
    if (v.path) {
        v.path = rootUrl + v.path;
    }
});

config.nav.push({
    title: '文档版本: ' + versions[currentVersionIndex].title, type: 'dropdown', items: versions
});

if (currentVersionIndex > latestVersionIndex) {
    config.announcement = {
        type: 'danger',
        html: '你当前正在访问的是旧版本SDK的文档，内容可能与最新版本不相符，点击&nbsp;<span style="cursor: pointer;" '
            + 'onclick="window.location.assign(\'' + versions[latestVersionIndex].path + '\' + window.location.hash)"'
            + '>这里</span>&nbsp;访问最新文档。'
    };
} else if (currentVersionIndex < latestVersionIndex) {
    config.announcement = {
        type: 'primary',
        html: '你当前正在访问的是测试版SDK的文档，测试版本提供了更丰富的功能但可能不稳定，点击&nbsp;<span style="cursor: pointer;" '
            + 'onclick="window.location.assign(\'' + versions[latestVersionIndex].path + '\' + window.location.hash)"'
            + '>这里</span>&nbsp;访问最新稳定版的文档。'
    };
}

docute.init(config);