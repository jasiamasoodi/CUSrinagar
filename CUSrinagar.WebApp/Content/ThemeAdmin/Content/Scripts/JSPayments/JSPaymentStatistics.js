jQuery(document).ready(function ($) {
    $("#searchs").mouseover(function () {
        $("#psfrm").attr("action", "/CUSrinagarAdminPanel/Statistics/PaymentStatistics");
    });
    $("#pc").mouseover(function () {
        $("#psfrm").attr("action", "/CUSrinagarAdminPanel/Statistics/ProgrammeCollegeSemesterAdmissionPayments");
    });

    $("#cc").mouseover(function () {
        $("#psfrm").attr("action", "/CUSrinagarAdminPanel/Statistics/CourseCollegeSemesterAdmissionPayments");
    });
    $("#ipd").mouseover(function () {
        $("#psfrm").attr("action", "/CUSrinagarAdminPanel/Statistics/IndividualAdmissionPayments");
    });
});