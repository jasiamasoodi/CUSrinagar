jQuery(document).ready(function ($) {

    //---------------- Assign RollNos in Bulk --------------------
    $(".JSSubmitBtn").click(function (e) {
        if ($("#frmVerifyRR").valid()) {
            var URL = $(this).attr("data-url");
            $(".JSSubmitBtn").hide();
            $(".jsExcelbtn").hide();
            $(".errorMsg").empty().text("Working... Please Wait.");

            var _Programme = $(" #frmVerifyRR #Programme").val();
            var Batch = $("#frmVerifyRR #Batch").val();
            var Course_ID = $("#frmVerifyRR #Course_ID").val();
            var EntranceRollNos = $("#frmVerifyRR #EntranceRollNos").val();
            var College_ID = $("#frmVerifyRR #College_ID").val();

            if (URL !== "") {
                $.ajax({
                    url: URL,
                    type: "POST",
                    async: true,
                    data: JSON.stringify({ Programme: _Programme, Batch: Batch, Course_ID: Course_ID, EntranceRollNos: EntranceRollNos, College_ID: College_ID }),
                    dataType: "html",
                    contentType: "application/json",
                    traditional: false,
                    success: function (response) {
                        $("#JSDisplayDetails").empty().html(response);
                        $(".JSSubmitBtn").show();
                        $(".jsExcelbtn").show();
                        $(".errorMsg").empty();
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        $(".JSSubmitBtn").show();
                        $(".jsExcelbtn").show();
                        $(".errorMsg").empty();
                        alert('status code: ' + jqXHR.status + '\n Please check your internet connection or refresh and try again.');
                    }
                });
                return false;
            } else {
                return true;
            }
        }
        return false;
    });


    //------------------ get courses ----------------------------
    $("#frmVerifyRR #Programme,#frmVerifyRR #Batch").change(function () {

        var Id = $("#frmVerifyRR #Programme").val();
        var Id1 = $("#frmVerifyRR #Batch").val();
        if (Id !== "" && Id1 !== "") {
            $("#frmVerifyRR .CourseID").empty().html("Getting Courses...");
            $.ajax({
                async: true,
                url: "/CUSrinagarAdminPanel/Entrance/_GetCoursesRRDDL/" + Id + "/" + Id1,
                type: "POST",
                data: {},
                datatype: "html",
                success: function (response) {
                    $("#frmVerifyRR .CourseID").empty().html(response);
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    alert('status code: ' + jqXHR.status + '\n Please check your internet connection or refresh and try again.');
                    location.reload();
                }
            });
        }
    });

    $("#frmVerifyRR #EntranceRollNos").focusout(function () {
        var _Message = $(this).val();
        $(this).val('').val(_Message.replace(/(\r\n|\n|\r|\s)/gm, ""));
    });
});