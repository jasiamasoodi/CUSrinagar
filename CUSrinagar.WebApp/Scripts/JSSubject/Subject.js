
$(document).ready(function () {

    $(document).on('click', '.js-delete-row', function () {
        var msg = '<h4>Are you sure you want to delete this subject ?</h4>';
        showConfirmationDialog(msg);
        var $tr = $(this).closest('tr');
        var _id = $tr.find(".jsRow_ID").val();
        $('#confirmationDialog').off('click.confirmation', 'button').on('click.confirmation', 'button', function () {
            var $btn = $(this);
            hideConfirmationDialog();
            if ($btn.data('response') == 'yes') {
                //var _student_id = $("[id$=Student_ID]").val();
                DeleteRow(_id, $tr);
            }
        });
    });
   


    function DeleteRow(_id,$tr) {
        var _url = getBaseUrlAdmin() + "Subject/Delete";
        $.ajax({
            url: _url,
            type: "POST",
            datatype: "Json",
            async: false,
            data: { id: _id },
            success: function (responseData) {
                if (responseData.IsSuccess) {
                    showSuccessMessage(responseData.NumberOfRecordsEffected + responseData.SuccessMessage);
                    $tr.remove();
                } else {
                    showErrorMessage(responseData.ErrorMessage);
                }
            },
            error: function (xhr, error, msg) {
                showErrorMessage(msg);
            }
        });
    }



    var $pagertable = $(".js-pager-table");
    if (Course && Programme) {
        $(".fa-search-btn").trigger("click");
    }

    $("#Programme").change(function () {
        var programme = $(this).val();
        $("#SubjectProgram").val(programme);
    });


});


var form = "";







function IsActive(event) {
    var _url = getBaseUrlAdmin() + "Subject/ChangeStatus";
    $.ajax({
        url: _url,
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
        if (Type == "Subject" || Type == "Semester")
        { id = $("#Course_ID").val(); }
        var childType = $(this).attr('data-ddlchildtype');
        var childSubType = $(this).attr('data-ddlsubchildtype');
        var path = chilDDLPath;
        if (Type != "" && Type != undefined && id != "" && id != undefined) {
            $.ajax({
                url: path,
                data: { id: id, Type: Type, childType: childType, childSubType: childSubType },
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