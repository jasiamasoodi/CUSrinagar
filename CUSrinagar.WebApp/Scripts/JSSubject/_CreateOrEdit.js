ChosenStyle();

$(document).on('change', '.ParentDDL', function (e) {

    var id = $(this).val();
    var Type = $(this).attr('data-ddltype');
    var childType = $(this).attr('data-ddlchildtype');
    var childSubType = $(this).attr('data-ddlsubchildtype');
    var path = "/CUSrinagarAdminPanel/Subject/_GetChildDDL"
    if (Type != "" && Type != undefined) {
        $.ajax({
            url: path,
            data: { id: id, Type: Type, childType: childType, childSubType: childSubType },
            datatype: "json",
            contentType: "application/json; charset=utf-8",
            encode: true,
            type: "Get",
            cache: false,
            async: false,
            success: function (response) {
                var PartialView = response;
                $('#' + Type).html(PartialView); // Modify me
                ChosenStyle();

            }
        });
    }
});
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
var id = $("#Course_ID").val();
if (id == '4A583C48-313C-4FEC-80B6-2BEC220E71E6') {
    $(".collegeDDL").removeClass("hidden");
    ChosenStyle();
}
$(document).on('focusOut', '#SubjectNumber', function (e) {
    
    setSubjectCode();
});

$(document).on('change', '#SubjectType', function (e) {

    setSubjectCode();
});
$(document).on('change', '#Semester', function (e) {
    var id = $("#Course_ID").val();
    var sem = $(this).val();

    if (id != "" && id != undefined && sem != "" && sem != undefined) {
        $.ajax({
            url: subjectnumberpath,
            data: { CourseId: id, Semester: sem },
            datatype: "json",
            contentType: "application/json; charset=utf-8",
            encode: true,
            type: "Get",
            cache: false,
            async: false,
            success: function (code) {
                $('#SubjectNumber').val(code); // Modify me
                $('#SubjectType').val(""); // Modify me
                $('#SubjectType').trigger("chosen:updated");
                $('#SubjectCode').val(""); // Modify me
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
function setSubjectCode()
{
    var dte = new Date();
    var year = dte.getFullYear().toString().substring(2, 4);
    var sno = $("#SubjectNumber").val();
    var sem = $("#Semester").val();
    var id = $("#Course_ID").val();
    var st = $("#SubjectType").val();
    $.ajax({
        url: enumpath,
        data: { ColType: "SubjectType", Value: st },
        datatype: "json",
        contentType: "application/json; charset=utf-8",
        encode: true,
        type: "Get",
        cache: false,
        async: false,
        success: function (val) {
            st = val;
        }
    });
    var prg = $("#ProgrammeId").val();
    if ($.isNumeric(prg)) {
        $.ajax({
            url: enumpath,
            data: { ColType: "Programme", Value: prg },
            datatype: "json",
            contentType: "application/json; charset=utf-8",
            encode: true,
            type: "Get",
            cache: false,
            async: false,
            success: function (val) {
                prg = val; // Modify me
            }
        });
    }
    var sname = $("#SubjectFullName").val().substring(0, 3).toUpperCase();;
    if (sem.length == 1)
    { sem = "0" + sem; }
    if (sno.length == 1)
    { sno = "0" + sno; }
    //UGHIN17C0119
    $("#SubjectCode").val(prg + sname + year+st+sem+sno);
  }

