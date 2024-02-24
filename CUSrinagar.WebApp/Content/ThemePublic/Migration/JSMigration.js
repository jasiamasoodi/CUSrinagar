
$(document).on('click', '#SearchDetail', function (e) {
    var ftype = $(this).data("type");
    var prgme = $("#Programme").val();
    var dob = $("#DOB").val();
    var regno = $("#CUSRegistrationNo").val();
    $("#DOBVal").addClass("hidden");
    $("#CUSRegistrationNoVal").addClass("hidden");
    if ($.trim(dob) == "") { $("#DOBVal").removeClass("hidden"); }
    if ($.trim(regno) == "") { $("#CUSRegistrationNoVal").removeClass("hidden"); }
    $.ajax({
        url: "/Migration/GetStudentDetail",
        data: {
            CusRegistrationNo: $("#CUSRegistrationNo").val(),
            DOB: $("#DOB").val(),
            programme: prgme,
            formType: ftype
        },
        datatype: "json",
        contentType: "application/json; charset=utf-8",
        encode: true,
        type: "Get",
        cache: false,
        async: false,
        success: function (response) {
            if (response.includes("RePrint")) {
                var Student = response.split('=');
                var TypeIs = ftype
                var Student_Id = Student[1];
                var PrintProgramme = prgme;
                var url = "/Migration/Print?Student_Id=" + encodeURIComponent(Student_Id) + "&TypeIs=" + encodeURIComponent(TypeIs) + "&PP=" + encodeURIComponent(PrintProgramme);
                window.location.href = url;
            }
            else {
                var PartialView = response;
                $('#SubDiv').html(PartialView); // Modify me
            }
        },
        error: function (response) {
        }
    });
});
