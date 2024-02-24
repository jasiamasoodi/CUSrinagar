function numbersOnly(e) {
    var charCode;
    if (window.event)         // IE
    { charCode = e.keyCode }

    else if (e.which)            // Netscape/Firefox/Opera
    { charCode = e.which }
    if (charCode == undefined || charCode == 8 || charCode == 9 || (charCode >= 48 && charCode <= 57))
        return true;
    else
        return false;
}
$(document).on('focusout', '#BasicPay', function (e) {
    var EmployeeType = $("#EmployeeType").val();
    if (EmployeeType === contractual || EmployeeType === needbasis)
    { return;}
    var sal = $(this).val();
    var isnps = $("#ISNPS").val();
    var path = "/CUSrinagarAdminPanel/Salary/CalculateSalary";
    if (sal !== "" && sal !== undefined) {
        $.ajax({
            url: path,
            data: { BasicPay: sal, isNPS: isnps },
            datatype: "json",
            contentType: "application/json; charset=utf-8",
            encode: true,
            type: "Get",
            cache: false,
            async: false,
            success: function (response) {
                if (response !== null && response.d !== null) {
                    var data = response;
                    data = $.parseJSON(data);
                    $("#DA").val(data.DA);
                    $("#HRA").val(data.HRA);
                    $("#MA").val(data.MA);
                    $("#CCA").val(data.CCA);
                    $("#SPL_Pay").val(data.SPL_Pay);
                    $("#ChargeAllow").val(data.ChargeAllow);
                    $("#GPF_Sub").val(data.GPF_Sub);
                    $("#GPF_ref").val(data.GPF_ref);
                    $("#GroupIns").val(data.GroupIns);
                    $("#SLI_I").val(data.SLI_I);
                    $("#SLI_II").val(data.SLI_II);
                    $("#I_Tax").val(data.I_Tax);
                    $("#RecoveryExcess").val(data.RecoveryExcess);
                    $("#NPSShare").val(data.NPSShare);
                }
            }
        });
    }
});
$(document).on('change', '#Course_ID', function (e) {
    var id = $(this).val();
    var sem = $("#Semester").val();
    if (id == '4A583C48-313C-4FEC-80B6-2BEC220E71E6') {
        $(".collegeDDL").removeClass("hidden");
        ChosenStyle();
    }
    else { $(".collegeDDL").addClass("hidden"); }

});