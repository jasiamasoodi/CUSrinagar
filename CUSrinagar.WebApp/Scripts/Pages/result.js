$(document).ready(function () {

    $(document).on('keydown', '#jsSearchText', function (event) {
        if (event.which == 13) {
            $("#jsSearchButton").trigger('click');
        }
    });

    $("#jsSearchButton").click(function () {
        var searchText = $("#jsSearchText").val();
        if (isNullOrEmpty(searchText) || $("#jsSearchText").hasClass('disabled')) return;
        $("#jsSearchText").prop("disabled", true).addClass('disabled'); 
        var resultNotificaitonModel = $('#ResultNotification').val();
        getStudentResult(resultNotificaitonModel, searchText);
    });


    $(document).on('click', '.jsPrint', function () {
        window.print();
    });

});

function getStudentResult(model, value) {
    $(".jsSingleContent").show();
    model = JSON.parse(model);

    var _url = getBaseUrlWebApp() + "Result/ResultPartial?searchvalue=" + value;
    $.ajax({
        url: _url,
        data: JSON.stringify(model),
        type: 'POST',
        dataType: 'html',
        contentType: 'application/json; charset=UTF-8',
        success: function (html) {
            $(".jsSingleContent").html(html);
            $('[data-toggle="tooltip"]').tooltip();   
        }, beforeSend: function () { showLoader(); }
        , complete: function () {
            hideLoader();
            $("#jsSearchText").removeAttr("disabled").removeClass('disabled');
        }, error: function (xhr,error,message) {
            var xhr ="";
        }
    });
}
