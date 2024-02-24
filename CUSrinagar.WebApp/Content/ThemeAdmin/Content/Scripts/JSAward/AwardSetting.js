
function IsSubmitted(event) {
    $.ajax({
        url: _ChangeStatus,
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

$(document).on("change", ".jsAllowDownloadForm", function () {
    var RecieverMail = $('#RecieverMail').val();
    var checked = $(this).is(":checked");
    if (checked) {
        $("#AllowFormsModal").modal('show');
        $("#Awardfilter_Id").val($(this).attr("id"));
    } else {
        IsAwardOpen($(this).attr("id"), "", RecieverMail);

    }

});
$(document).on("click", ".jsUploadStatus", function () {
    var _ChangeStatus = '/CUSrinagarAdminPanel/AwardSetting/UpdateAwardStatus';
    var msg = '<h4>Are you sure you want to update the IsPassed bit?</h4>';
    var ids = $(this).attr("id");
    var types = $(this).attr("Typeis");
    //msg += '<h5>Once deleted this student, corresponding student has to reassign all its subject combination details</h5>';
    showConfirmationDialog(msg);
    $('#confirmationDialog').off('click.confirmation', 'button').on('click.confirmation', 'button', function () {
        var $btn = $(this);
        hideConfirmationDialog();
        if ($btn.data('response') == 'yes') {
            $.ajax({
                url: _ChangeStatus,
                type: "GET",
                data: {
                    id: ids, Type: types
                },
                cache: false,
                async: true,
                datatype: "html",
                contentType: "application/html; charset=utf-8",
                success: function (response) {
                    showAlertDialog(response);
                    $(".fa-search-btn").trigger("click");
                },
                error: function (response) {
                }
            });
        }
    });
});



$('#AllowFormsModalBtn').click(function () {

    var RecieverMail = $('#RecieverMail').val();
    var EndDate = $('#Model_EndDate').val();
    var idv = $("#Awardfilter_Id").val();
    $("#err").text('');
    if (EndDate === "") {
        $("#err").text("End Date is required");
        return;
    }
    IsAwardOpen(idv, EndDate);

    $("#MModel_EndDate").val('');
});


$(document).on("click", ".jscancelModel", function () {
    $("#AllowFormsModal").modal('hide');
    $(".fa-search-btn").trigger("click");
    $("#Model_RegularBatch").val('');
});
function IsAwardOpen(idv, EndDate, RecieverMail) {
    var _ChangeStatus = '/CUSrinagarAdminPanel/AwardSetting/OpenAwardLink';
    $.ajax({
        url: _ChangeStatus,
        type: "GET",
        data: {
            id: idv, EndDate: EndDate, RecieverMail: RecieverMail
        },
        cache: false,
        async: true,
        datatype: "html",
        contentType: "application/html; charset=utf-8",
        success: function (response) {
            $("#AllowFormsModal").modal('hide');
            $(".fa-search-btn").trigger("click");
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
        var path = chilDDLPath;
        $.ajax({
            url: path,
            data: { id: id, Type: Type },
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
    });



});


//---------------------Closing Model For Combination----------------------

$('#enddateBtn').click(function () {

    var EndDate = $('#Model_EndDate').val();
    var Batch = $('#BatchDDL').val();
    var PrintProgramme = $('#PrintProgrammeDDL').val();
    var Semester = $('#SemesterDDL').val();

    $("#err").text('');
    if (EndDate === "") {
        $("#err").text("End Date is required");
        return;
    }
    SetCombinationEndDate(Batch, EndDate, PrintProgramme, Semester);

    $("#MModel_EndDate").val('');
});
function SetCombinationEndDate(Batch, EndDate, PrintProgramme, Semester) {
    var _ChangeStatus = '/CUSrinagarAdminPanel/CombinationSetting/SetCombinationEndDate';
    $.ajax({
        url: _ChangeStatus,
        type: "Get",
        data: {
            batch: Batch, endDate: EndDate, printProgramme: PrintProgramme, semester: Semester
        },
        cache: false,
        async: true,
        datatype: "html",
        contentType: "application/Html; charset=utf-8",
        success: function (response) {
            $("#AllowFormsModal").modal('hide');
            if (response == true) {
                alert("changes saved");
            }
            else { alert("changes not saved"); }
        },
        error: function (response) {
            alert("changes not saved");
        }
    });
    return true;
}


//-------------------------Update Backlogs to LaterOn----------------------------

$("body").on("change", ".awarddl", (function () {
    var types = $(this).val();
    if (Number(types) == 0) {
        $("#onlyTheoryText").css("display", "block");
    } else {
        $("#onlyTheoryText").css("display", "block");
    }
}));
$('#updatebacklog').click(function () {
    var Awardfilter_Id = $("#Awardfilter_Id").val();
    console.log(Awardfilter_Id);
    UpdateBacklogsToLateron(Awardfilter_Id);
});

function UpdateBacklogsToLateron(Awardfilter_Id) {
    var _ChangeStatus = '/CUSrinagarAdminPanel/AwardSetting/UpdateBacklogsToLateron';
    $.ajax({
        url: _ChangeStatus,
        type: "Get",
        data: {
            Awardfilter_Id: Awardfilter_Id
        },
        cache: false,
        async: true,
        datatype: "html",
        contentType: "application/Html; charset=utf-8",
        success: function (response) {
            $("#AllowFormsModal").modal('hide');
            if (response > 0) {
                //console.log(response);
                alert("All Backlogs updated to LO");
            }
            else {
                console.log(response);
                //alert("Sorry not updated...There may be some error");
            }
        },
        error: function (response) {
            alert("changes not saved");
        }
    });
    return true;
}


