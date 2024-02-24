$(document).ready(function () {
    $("body").on("change", '.jsSemesterDDL', function () {
        if ($(".jsSemesterDDL option:selected").val() === "1") {
            $(".jsimport").removeClass("hidden");
            $("#jssearchHelp").empty().text("Please check search filters like program, semester (or try to import if it is new admission)...");
        } else {
            $("#jssearchHelp").empty().text("Please check search filters like program, semester...");
            $(".jsimport").addClass("hidden");
        }
    });

    $("body").on("click", '.jsimport', function () {
        var _cusformno = $("#Form_RegistrationNumber").val();
        if (_cusformno === "") {
            alert("Form no. is required");
            return;
        }
        if (confirm("Are you sure you want to import?")) {
            showLoader();
            $(".jsimport").prop("disabled", true);
            $(".jssearchbtn").prop("disabled", true);
            var _url = getBaseUrlCollege() + "AssignCombination/ImportFromIHtoUG";
            $.ajax({
                url: _url,
                type: "POST",
                datatype: "Json",
                async: true,
                data: { cusformno: _cusformno },
                success: function (responseData) {
                    if (responseData.IsSuccess) {
                        showSuccessMessage(responseData.message);
                        $(".jsimport").addClass("hidden");
                        $("#printProg").val("1");
                        $(".jssearchbtn").prop("disabled", false).click();
                    } else {
                        showErrorMessage(responseData.message);
                    }
                },
                error: function (xhr, error, msg) {
                    showErrorMessage(msg);
                },
                complete: function () {
                    $(".jssearchbtn").prop("disabled", false);
                    $(".jsimport").prop("disabled", false);
                    hideLoader();
                }
            });
        }
    });

    $("body").on("click", '.jsimportPGtoIH', function () {
        var _cusformno = $("[name=StudentFormNo]").val();
        if (_cusformno === "") {
            alert("Form no. is required");
            return;
        }
        if (confirm("Are you sure you want to import?")) {
            showLoader();
            $(".jsimport").prop("disabled", true);
            $(".jssearchbtn").prop("disabled", true);
            var _url = getBaseUrlCollege() + "AssignCombination/ImportFromPGtoIH";
            $.ajax({
                url: _url,
                type: "POST",
                datatype: "Json",
                async: true,
                data: { cusformno: _cusformno },
                success: function (responseData) {
                    if (responseData.IsSuccess) {
                        showSuccessMessage(responseData.message);
                        //$(".jsimport").addClass("hidden");
                        //$("#printProg").val("1");
                        $(".jssearchbtn").prop("disabled", false).click();
                    } else {
                        showErrorMessage(responseData.message);
                    }
                },
                error: function (xhr, error, msg) {
                    showErrorMessage(msg);
                },
                complete: function () {
                    $(".jssearchbtn").prop("disabled", false);
                    $(".jsimport").prop("disabled", false);
                    hideLoader();
                }
            });
        }
    });

});