$(document).ready(function ($) {
    $("input").attr("autocomplete", "off");
    $(".jsvalminm").change(function () {
        var maxMarks = $(this).closest("tr").find(".jsvalmm").val();
        var minMarks = $(this).val();
        if (Number(maxMarks) < Number(minMarks)) {
            $(this).val('');
            alert("Min. Marks Obtained should be less or equal to Max. Marks");
        }
    });

    $(".jsvalmm").change(function () {
        var minMarks = $(this).closest("tr").find(".jsvalminm").val();
        var maxMarks = $(this).val();
        if (Number(maxMarks) < Number(minMarks)) {
            $(this).closest("tr").find(".jsvalminm").val('');
            alert("Max. Marks should be greater or equal to Min. Marks");
        }
    });

    $(document).on('submit', '.frmidc', function (e) {
        var examName = $("#Exam").val();
        $(".errenmsg").text('');
        if ($.trim(examName) == "") {
            $(".errenmsg").text('Required');
        }
        if ($(this).valid() && $.trim(examName) != "") {
            $("#StudentPreviousQualification_ExamName").val(examName);

            if (confirm("Are you sure you want to Proceed? It is recommanded to check your details.")) {
                $("#jsSave").val("Saving....").prop("disabled", true);
                return true;
            }
            return false;
        } else {
            return false;
        }
    });

    //---------------------------- get qualifying exam details ---------------
    $("#Exam").change(function () {
        var examName = $.trim($("#Exam").val());
        $(".errenmsg").text('');
        if (examName == "") {
            $(".errenmsg").text('Required');
            return;
        }

        var programme = $.trim($("#PrintProgramme").val());
        var Student_ID = $.trim($("#Student_ID").val());

        if (examName != "" && programme != "" && Student_ID != "") {
            $("#Jspleasewait").show(150);
            $.ajax({
                url: "/ApplicationForms/GetStudentPreviousQualification",
                type: "POST",
                data: { s: Student_ID, p: programme, e: examName },
                dataType: "json",
                traditional: true,
                success: function (response) {
                    removeDetails();
                    if (response.error.trim() == "") {
                        try {
                            if (response.qualifyingexam.Session != null && response.qualifyingexam.Session + "".trim() != "") {
                                $(".qualifyingExm").find('[id$="_Session"]').val(response.qualifyingexam.Session);
                            }
                            if (response.qualifyingexam.Subjects != null && response.qualifyingexam.Subjects + "".trim() != "") {
                                $(".qualifyingExm").find('[id$="_Subjects"]').val(response.qualifyingexam.Subjects);
                            }
                            if (response.qualifyingexam.Year != null) {
                                $(".qualifyingExm").find('[id$="_Year"]').val(response.qualifyingexam.Year);
                            }
                            if (response.qualifyingexam.RollNo != null && response.qualifyingexam.RollNo + "".trim() != "") {
                                $(".qualifyingExm").find('[id$="_RollNo"]').val(response.qualifyingexam.RollNo);
                            }
                            if (response.qualifyingexam.MaxMarks != null) {
                                $(".qualifyingExm").find('[id$="_MaxMarks"]').val(response.qualifyingexam.MaxMarks);
                            }
                            if (response.qualifyingexam.MarksObt != null) {
                                $(".qualifyingExm").find('[id$="_MarksObt"]').val(response.qualifyingexam.MarksObt);
                            }
                            if (response.qualifyingexam.ExamBody != null && response.qualifyingexam.ExamBody + "".trim() != "") {
                                $(".qualifyingExm").find('[id$="_ExamBody"]').val(response.qualifyingexam.ExamBody);
                            }
                            if (response.qualifyingexam.Stream != null && response.qualifyingexam.Stream + "".trim() != "") {
                                $(".qualifyingExm").find('[id$="_Stream"]').val(response.qualifyingexam.Stream);
                            }
                        } catch (e) { }
                        if (response.qualifyingexam.Qualification_ID != null && response.qualifyingexam.Qualification_ID + "".trim() != "") {
                            $(".qualifyingExm").find('[id$="_Qualification_ID"]').val(response.qualifyingexam.Qualification_ID);
                        }
                    }
                    else {
                        alert(response.error);
                    }
                    $("#Jspleasewait").hide(150);
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    removeDetails();
                    $("#Jspleasewait").hide(150);
                    alert('status code: ' + jqXHR.status + '\n We are unable to fetch your Board details ,\nplease check your internet connection or refresh and try again.');
                }
            });
        }
        else {
            removeDetails();
            $("#Jspleasewait").hide(150);
        }
    });

    function removeDetails() {
        $(".qualifyingExm").find('[id$="_Session"]').val('');
        $(".qualifyingExm").find('[id$="_Subjects"]').val('');
        $(".qualifyingExm").find('[id$="_Year"]').val('');
        $(".qualifyingExm").find('[id$="_RollNo"]').val('');
        $(".qualifyingExm").find('[id$="_MaxMarks"]').val('');
        $(".qualifyingExm").find('[id$="_MarksObt"]').val('');
        $(".qualifyingExm").find('[id$="_ExamBody"]').val('');
        $(".qualifyingExm").find('[id$="_Stream"]').val('');
        $(".qualifyingExm").find('[id$="_Qualification_ID"]').val('00000000-0000-0000-0000-000000000000');
    }
});