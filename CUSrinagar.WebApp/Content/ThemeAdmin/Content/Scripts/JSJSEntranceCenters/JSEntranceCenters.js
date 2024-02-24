jQuery(document).ready(function ($) {
    $("#Jspleasewait").addClass("hidden");
    //-------------------- delete center ------------------------------
    $("#JSDeleteDataFromCenterAllotment").click(function () {
        if (confirm("Are you sure you want to truncate all data from Center Allotment Table?")) {
            $("#Jspleasewait").removeClass("hidden");

            $.ajax({
                async: true,
                url: "/CUSrinagarAdminPanel/Entrance/DeleteDataFromCenterAllotment",
                type: "POST",
                datatype: "json",
                success: function (response) {
                    if (response === true) {
                        $("#Jspleasewait").addClass("hidden");
                        alert("Delete successfully");
                    } else {
                        $("#Jspleasewait").addClass("hidden");
                        alert(response);
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    $("#Jspleasewait").addClass("hidden");
                    alert('status code: ' + jqXHR.status + '\n Please check your internet connection or refresh and try again.');
                }
            });
        } else {
            $("#Jspleasewait").addClass("hidden");
        }
    });
});
