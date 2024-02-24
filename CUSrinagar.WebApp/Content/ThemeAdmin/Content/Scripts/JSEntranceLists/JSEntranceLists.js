jQuery(document).ready(function ($) {
    $(".JSListTypeColumn").click(function () {
        var element = $(this);
        var Id = element.attr("data-id");
        var FileName = element.attr("data-fileName");
        if (confirm("Are you sure you want delete the list?")) {
            element.empty().html("Deleting...").prop("disabled", true);
            $.ajax({
                datatype: "json",
                contentType: "application/json; charset=utf-8",
                type: "POST",
                async: true,
                data: JSON.stringify({ Id: Id, FileName: FileName }),
                url: "/CUSrinagarAdminPanel/EntranceLists/Delete",
                success: function (response) {
                    if (response === true) {
                        if (FileName === "nofile") {
                            location.reload();
                        } else {
                            element.closest("td").empty();
                        }
                    } else {
                        element.empty().html('<i class="fa fa- trash"></i>').prop("disabled", false);
                        alert("Nothing deleted.");
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    alert('status code: ' + jqXHR.status + '\n Please check your internet connection or refresh and try again.');
                }
            });
        }
    });


    $("#JSfrmEntranceLists").submit(function () {
        if ($(this).valid()) {
            if (confirm("Are you sure you want to proceed?")) {
                $(".JSSave").empty().html('<i class="fa fa-save"></i>&nbsp; Working...').prop("disabled", true);
                return true;
            }
        }
        return false;
    });

    //------------------ get courses ----------------------------
    $("body").on("change", "#JSfrmEntranceLists #Programme", function () {
        var programmeElement = $("#JSfrmEntranceLists #Programme");
        var prog = programmeElement.val();
        if (prog !== "") {
            $.ajax({
                async: true,
                url: "/CUSrinagarAdminPanel/EntranceLists/GetCourseDDL",
                type: "POST",
                data: JSON.stringify({ programme: prog }),
                datatype: "html",
                contentType: "application/json; charset=utf-8",
                success: function (response) {
                    $("#JSfrmEntranceLists .CourseID").empty().html(response);
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    alert('status code: ' + jqXHR.status + '\n Please check your internet connection or refresh and try again.');
                }
            });
        }
    });

});