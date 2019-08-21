function alert(msg) {
    var ga = $('#globalalert');
    ga.find('.toast-body').html(msg);
    ga.toast('show');
}
window.onbeforeunload = function () {
    $('#LoadingMask').fadeIn(200);
    setTimeout(function () {
        $('#LoadingMask').fadeOut(200);
    }, 10000)
}
$(window).on('load', function () {
    $('#LoadingMask').fadeOut(200);
})