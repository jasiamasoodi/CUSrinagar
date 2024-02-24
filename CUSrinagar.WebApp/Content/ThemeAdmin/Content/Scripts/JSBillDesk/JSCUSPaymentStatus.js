jQuery(document).ready(function ($) {
    $("#FeeType").change(function () {
        var FeeTypeSelected = $(this).val();
        var label = $("label[for='SearchQuery']");
        if (FeeTypeSelected == 2 || FeeTypeSelected == 3 || FeeTypeSelected == 4) {
            label.empty().text("SearchQuery (Exam / Xerox / ReEvaluation Form No. / Email / MobileNo)");
        } else if (FeeTypeSelected != "") {
            label.empty().text("SearchQuery(CUS Form No. OR CUSRegn No. OR BoardRegn No.)");
        } else {
            label.empty().text("SearchQuery");
        }

        //Remove old search if any
        $(".DelOnChange").empty().html('<tr> <th colspan="7" class="text-center">No results found, Please check your search parameters.</th> </tr>');
    });





    // Check Payment Status
    $(".ChkStatus").click(function () {
        var CustomerID = $(this).attr("data-CustomerID");
        var clkedElement = $(this).find("button");
        clkedElement.text("Checking...").prop("disabled", true);
        $.ajax({
            async: true,
            url: "/CUSrinagarAdminPanel/BillDesk/CheckPaymentStatus",
            type: "POST",
            data: { Id: CustomerID },
            dataType: "json",
            traditional: true,
            success: function (response) {
                $("#" + CustomerID).empty().html(response);
                $("." + CustomerID).removeClass("hidden");
                $("#" + CustomerID).removeClass("hidden");
                clkedElement.prop("disabled", false).text("Check Status");
            },
            error: function (jqXHR, textStatus, errorThrown) {
                clkedElement.prop("disabled", false).text("Check Status");
                alert('status code: ' + jqXHR.status + '\n Please check your internet connection or refresh and try again.');
            }
        });
    });



    // Close
    $("body").on("click", ".closetr", function () {
        var ClassToClose = $(this).attr('data-Id');
        $("." + ClassToClose).addClass("hidden");
    });




    //Reconcile
    $("body").on("click", ".Reconcile", function () {
        var FeeTypeSelected = $("#FeeTypeSelected").val();
        if (confirm("Caution!\n\nYou have selected " + FeeTypeSelected + " as FeeType,\nthereby Reconcilation will be done for " + FeeTypeSelected + ".\n\nWould you like to proceed.")) {
            var clkedElement = $(this);
            var billdeskresponse = clkedElement.attr("data-billdeskresponse");
            clkedElement.text("Working ...").prop("disabled", true);
            $.ajax({
                async: true,
                url: "/CUSrinagarAdminPanel/BillDesk/Reconcile",
                type: "POST",
                data: { BillDeskResponse: billdeskresponse, FeeType: FeeTypeSelected },
                dataType: "json",
                traditional: true,
                success: function (response) {
                    if (response === true) {
                        clkedElement.text("Reconciled Successfully");
                    } else {
                        clkedElement.prop("disabled", true).text(response);
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    clkedElement.prop("disabled", false).text("Reconcile Failed(try again)");
                    alert('status code: ' + jqXHR.status + '\n Please check your internet connection or refresh and try again.');
                }
            });
        }
    });


    $("#PaymentStatus").submit(function (e) {
        if ($(this).valid()) {
            $("#PaymentStatus #JSSearchPay").empty().html('<i class="fa fa- search"></i>&nbsp; Working....').prop("disabled", true);
            return true;
        }
        return false;
    });

    $("body").on("click", ".copytorefund", function () {
        var copyText = $(this).attr('data-copy');
        var $temp = $("<input>");
        $("body").append($temp);
        $temp.val(copyText.replace(/\/"/g, '"')).select();
        document.execCommand("copy");
        $temp.remove();
        alert("Copied to clipboard");
    });
});