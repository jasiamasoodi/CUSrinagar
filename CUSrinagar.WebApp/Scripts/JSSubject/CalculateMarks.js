jQuery(function ($) {
    $("#TheoryAttendance").focusout(function () {
        var TheoryAttendance = Number($(this).val());
        if (TheoryAttendance > 0) {
            var TheoryMaxMarks = (TheoryAttendance * 15) - TheoryAttendance;
            var TheoryMinMarks = parseInt((40 / 100) * TheoryMaxMarks);
            $("#TheoryMinPassMarks").val("").val(TheoryMinMarks);
            $("#TheoryMaxMarks").val("").val(TheoryMaxMarks);
        } else {
            $("#TheoryMinPassMarks").val("").val("0");
            $("#TheoryMaxMarks").val("").val("0");
        }
    });

    $("#PracticalAttendence").focusout(function () {
        var PracticalAttendance = Number($(this).val());
        var TheoryAttendance = Number($("#TheoryAttendance").val());
        if (PracticalAttendance > 0 && TheoryAttendance > 0) {
            var PracticalMaxMarks = (PracticalAttendance * 15) - PracticalAttendance;
            var PracticalFourtyPercent = PracticalMaxMarks + PracticalAttendance + TheoryAttendance;

            var PracticalMinMarks = Math.round((40 / 100) * PracticalFourtyPercent);
            $("#PracticalMinPassMarks").val("").val(PracticalMinMarks);
            $("#PracticalMaxMarks").val("").val(PracticalMaxMarks);
        } else {
            $("#PracticalMinPassMarks").val("").val("0");
            $("#PracticalMaxMarks").val("").val("0");
        }
    });
});