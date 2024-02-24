
$(document).ready(function () {
    loadpagertableWithDefaultParams();

    //setTimeout(function () { getnotifications(); }, 5000);
    //var program = $("#Program").val();
    //$.ajax({
    //    url: _resultNotificationUrl,
    //    data: { Program:program },
    //    type: 'Get',
    //    dataType: 'html',
    //    contentType: 'application/json; charset=UTF-8',
    //    success: function (html) {
    //        $(".jsTableContent").html(html);
    //    }
    //});

    
    //$(document).on('click', '.jsViewResult', function () {
    //    var $tr = $(this).closest('tr');
    //    var resultNotificaitonModel = $tr.find('[id$=ResultNotification_ID]').data('model');
    //    var actionUrl = getBaseUrlWebApp() + "Result/ResultPG";
    //    // var actionUrl =$(this).data('url');
    //    var $customform = createFormFromModel(resultNotificaitonModel, actionUrl);
    //    var $token = $tr.closest('.js-pager-table').find('[name*=RequestVerificationToken]');
    //    if ($token.length > 0) {
    //        $customform.append($token);
    //    }        
    //    $('body').append($customform);
    //    showLoader();
    //    $customform.submit();
    //});

});
