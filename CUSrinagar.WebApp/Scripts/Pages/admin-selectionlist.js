$(document).ready(function () {


    $(document).on('click', '.jsIsSelected', function () {
        var $element = $(this);
        var student_id = $element.val();
        var _url = "/CUSrinagarAdminPanel/" + "SeatAllocation/ISSelected";
        print_Validate(_url, student_id, $element);
    });

    $(document).on('click', '.ReleaseORAdmitSeat', function (e) {
        var button = $(this);
        var type = $(this).data("type");
        var programme = $("#Programme").val();
        var batch = $("#Batch").val();
        var course = $("#Course_ID").val();
        var student_ID = $(button).data("id");
        var rollno = $(button).data("rollno");
        var serial = $(button).data("serial");
        var label = type;
        if (type == "@CUSrinagar.Enums.StudentSelectionStatus.Joined") { label = "Admit"; }
        else { label = "Reject"; }
        var msg = "<h4>Are you sure you want to  " + label + " " + + rollno + "?</h4>";
        showConfirmationDialog(msg);
        $('#confirmationDialog').off('click.confirmation', 'button').on('click.confirmation', 'button', function () {
            var $btn = $(this);
            hideConfirmationDialog();
            if ($btn.data('response') == 'yes') {
                $.ajax({
                    url: "/CUSrinagarAdminPanel/SeatAllocation/ReleaseORAdmitSeat",
                    type: "Get",
                    data: { Course_ID: course, Batch: batch, Student_ID: student_ID, Programme: programme, StudentSelectionStatus: type },
                    traditional: true,
                    success: function (response) {
                        $(button).hide();
                        $("#Status_" + serial).html(type);
                        //$(".fa-search-btn").trigger("click");
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        alert('status code: ' + jqXHR.status + '\n Please check your internet connection or refresh and try again.');
                    }
                });
            }
        });


    });
    $(document).on('click', '#PrintList', function (e) {
        var number = $("#CurrentListNo").val();
        var course = $("#Course_ID option:selected").text();
        $(".labelhead").html("<h2 style='margin:0 !important;padding:0 !important'>Cluster University Srinagar</h2> <h4 style='margin:0 !important;padding:0 !important'>Provisional Selection List - " + number + " (" + course + ") (2022)</h4>");
        window.print();
    });
    $(document).on('change', '.SearchDDL', function (e) {
        var Programme = $(this).val();
        var path = chilDDLPath;
        if (Programme != "" && Programme != undefined) {
            $.ajax({
                url: path,
                data: { Programme: Programme },
                datatype: "json",
                contentType: "application/json; charset=utf-8",
                encode: true,
                type: "Get",
                cache: false,
                async: false,
                success: function (response) {
                    var PartialView = response;
                    $('#Course').html(PartialView); // Modify me

                    ChosenStyle();
                }
            });
        }
    });
    $(document).on('click', '#viewmatrix', function (e) {
        $("#selectionlist").addClass("hidden");
        $("#matrixlist").removeClass("hidden");
        $("#viewmatrix").addClass("hidden");
        $("#viewselection").removeClass("hidden");
        $("#showicon").addClass("hidden");
    });
    $(document).on('click', '#viewselection', function (e) {
        $("#selectionlist").removeClass("hidden");
        $("#matrixlist").addClass("hidden");
        $("#viewselection").addClass("hidden");
        $("#viewmatrix").removeClass("hidden");
        $("#showicon").removeClass("hidden");
    });

});

function print_Validate(_url, student_id, $element) {
    var pp = Number($("#PrintProgramme").val());
    $.ajax({
        url: _url + `?Student_ID=${student_id}&printProgramme=${pp}`,
        type: "GET",
        datatype: "Json",
        async: false,
        success: function (responseData) {
            if (responseData.IsSuccess) {
                showSuccessAlertMessage(responseData.NumberOfRecordsEffected + " record updated successfully.", 300);
                $element.closest('tr').addClass('success');
            } else {
                showErrorMessage(responseData.ErrorMessage);
            }
        },
        error: function (xhr, error, msg) {
            showErrorMessage(msg);
        }
    });

}
