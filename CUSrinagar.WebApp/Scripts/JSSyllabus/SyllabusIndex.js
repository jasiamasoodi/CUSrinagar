
$(document).ready(function () {

    var $pagertable = $(".js-pager-table");

    loadpagertableWithDefaultParams();

});
$('.date-picker').datepicker({
    autoclose: true,
    todayHighlight: true,
    dateFormat: 'MM/dd/yyyy',
    format: 'MM/dd/yyyy'
});

var form = "";







function IsActive(event) {
    $.ajax({
        url: ChangeStatusPath,
        type: "GET",
        data: { id: $(event).attr("id") },
        cache: false,
        async: true,
        datatype: "html",
        contentType: "application/html; charset=utf-8",
        success: function (response) {
            alert(response);

        },
        error: function (response) {
        }
    });
    return true;
}

jQuery(function ($) {


    $(document).on('change', '.SearchDDL', function (e) {
        var id = $(this).val();
        var Type = $(this).attr('data-ddltype');
        var Semester = $("#SemesterName").val();
        if (Type == "Subject" || Type == "Semester")
        { id = $("#Course_ID").val(); }
        if (Semester == "" || Semester == undefined)
        { Semester = 0; }
        else
        { Semester = Semester.replace("Semester ", ""); }
        var childType = $(this).attr('data-ddlchildtype');
        var childSubType = $(this).attr('data-ddlsubchildtype');
        var path = chilDDLPath;
        if (Type != "" && Type != undefined && id != "" && id != undefined) {
            $.ajax({
                url: path,
                data: { id: id, Type: Type, childType: childType, childSubType: childSubType, Semester: Semester },
                datatype: "json",
                contentType: "application/json; charset=utf-8",
                encode: true,
                type: "Get",
                cache: false,
                async: false,
                success: function (response) {
                    var PartialView = response;
                    $('#' + Type).html(PartialView); // Modify me

                    ChosenStyle();
                }
            });
        }
    });


    $(document).on('click', '.JSDelete', function (e) {

        var ans = confirm("Are you sure ,you want to delete syllabus.");
        if (ans) {
            $.post(DeletePath, { id: $(this).attr("id") },
                function (response) {
                    var $pagetable = $(".js-pager-table");
                    loadpagertable($pagetable);
                    alert(response);

                }).fail(function () {
                    console.log("error");
                });

            return true;
        } else {
            return false;
        }
    });
});