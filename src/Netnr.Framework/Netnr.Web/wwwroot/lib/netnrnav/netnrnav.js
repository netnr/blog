//netnrnav
(function ($) {
    $.fn.netnrnav = function (options) {
        options = options || {};
        var that = this,
            isHiding = false,
            _event = 0, // 这个临时变量是处理多级树事件判断的，很重要
            defaultOptions = {
                closingCascade: true // 级联关闭所有菜单，仅对垂直导航菜单有效
            };
        if (!$(that).hasClass('netnrnav')) {
            if ($(this).find('.netnrnav').length < 1)
                return;
            that = $(this).find('.netnrnav')[0];
        }
        defaultOptions = $.extend({}, defaultOptions, options);
        var _init = function () {
            if ($(that).hasClass('horizontal')) {
                $(that).find('li').hover(function () {
                    $(this).children('ul').show();
                }, function () {
                    $(this).children('ul').hide();
                });
            } else {
                $(that).find('li').click(function () {
                    if (_event != 0) {
                        // 这里很重要，处理事件回调
                        if ($(this).parent().parent().parent().hasClass('netnrnav')) {
                            _event = 0;
                        }
                        return;
                    }
                    if ("none" == $(this).children('ul').css('display'))
                        $(this).children('ul').show();
                    else {
                        if (defaultOptions.closingCascade) {
                            $(this).find('ul').hide();
                        } else {
                            $(this).children('ul').hide();
                        }
                    }
                    _event++;
                    if ($(this).parent().parent().parent().hasClass('netnrnav')) {
                        _event = 0;
                    }
                });
            }
            $(window).resize(_resize);
        },
            _show = function () {
                if (isHiding)
                    return;
                $(document.body).append('<div class="netnrnav slide-nav"></div>');
                $(document.body).append('<div class="netnrnav nav-mask"></div>');
                $('.slide-nav').html($(that).html());
                $('.slide-nav').find('li').click(function () {
                    if (_event != 0) {
                        // 这里很重要，处理事件回调
                        if ($(this).parent().parent().parent().hasClass('netnrnav')) {
                            _event = 0;
                        }
                        return;
                    }
                    if ("none" == $(this).children('ul').css('display'))
                        $(this).children('ul').show();
                    else {
                        if (defaultOptions.closingCascade) {
                            $(this).find('ul').hide();
                        } else {
                            $(this).children('ul').hide();
                        }
                    }
                    _event++;
                    if ($(this).parent().parent().parent().hasClass('netnrnav')) {
                        _event = 0;
                    }
                });
                $('.nav-mask').click(function () {
                    _hide();
                });
                // 某些浏览器有毒，加个小小的延迟让动画完整展示
                setTimeout(function () {
                    $('.slide-nav').toggleClass('active');
                    $('.nav-mask').toggleClass('active');
                }, 20);
            },
            _hide = function () {
                if (isHiding)
                    return;
                isHiding = true;
                $('.slide-nav').find('li').unbind();
                $('.slide-nav').removeClass('active');
                $('.nav-mask').removeClass('active');
                setTimeout(function () {
                    $('.slide-nav').remove();
                    $('.nav-mask').remove();
                    isHiding = false;
                }, 600);
            },
            _toggle = function () {
                ($('.slide-nav').length > 0) ? _hide() : _show();
            },
            _resize = function () {
                // 窗口改变时需要完成的事情，预留着吧
            };
        _resize();
        _init();
        return {
            show: _show,
            hide: _hide,
            toggle: _toggle
        };
    };
})($);

$(function () {
    var nnv = $('#netnrnav');
    if (nnv.length) {
        $('.MenuToggle').click(nnv.netnrnav().toggle);
    }
});