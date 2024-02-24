
//function saveRowData($tr) {
//    return new Promise(function (resolve, reject) {
//        var semester = {};
//        $tr.find(':input').each(function (index, element) {
//            var $element = $(element);
//            if (!isNullOrEmpty($element.attr('name')) && $element.attr('name').split('.').length > 1) {
//                var name = $element.attr('name').split('.')[1];
//                var value = $element.val();
//                semester[name] = value;
//            }
//        });
//        var _url = area + "Award/PostAward?printProgramme=" + $("#Programme").val() + "&semester=" + $(":input#Semester option:selected").val() + "&marksFor=" + $("#MarksFor option:selected").val();
//        var method = "POST";
//        if (!isNullOrEmpty(semester._ID) && semester._ID != emptyGuid()) {
//            _url = area + "Award/PutAward?printProgramme=" + $("#Programme").val() + "&semester=" + $(":input#Semester option:selected").val() + "&marksFor=" + $("#MarksFor option:selected").val();
//            //method = "PUT";
//        }
//        $tr.nextAll('tr:not(tr.hidden):eq(0)').first().find('input[type=text]:eq(0)').select().focus();

//    });
//}
var area = "";
$(document).ready(function () {

    if ($("#MarksFor").val() == 'Theory') { area = getBaseUrlAdmin(); }
    else { area = getBaseUrlCollege(); }
    function setFocus($pagertable) {
        if ($pagertable.find('tr').length <= 5) {
            $pagertable.find('tr:eq(1) input[id*=ExternalAttendance_AssessmentMarks]').focus();
        }

    }
    var $pagertable = $(".js-pager-table");
    var subjectId = $("#Subject_ID option:selected").val();
    if (subjectId) { loadpagertable(); }
    $(".js-final-submit").click(function () {
        var $form = $("#result-form");
        if (!$form.valid()) {

            var $validator = $form.validate({ ignore: $form.find('tr :not(.jsDirtyRow) input'), focusInvalid: true }); // performance issue
            $($validator.errorList[0].element).select().focus();
            return;
        }
        var $element = $(this);
        var input = {};
        input.Semester = $("#Semester option:selected").val();
        input.Programme = $("#Programme").val();
        input.MarksFor = $("#MarksFor").val();
        input.CombinationSubjects = $("#CombinationSubjects option:selected").val();
        input.Year = $("#Year").val();
        input.IsBacklog = $("#IsBacklog").val();
        if (isNullOrEmpty(input.Semester)) {
            showErrorMessage('Please choose Semester.'); return;
        }
        if (isNullOrEmpty(input.CombinationSubjects)) {
            showErrorMessage('Please choose Subject.'); return;
        }
        var url = area + "Award/IsAwardsubmittedForAll";
        var msg = "<h5>Are you sure you want to Submit award";
        //$.ajax({
        //    url: url,
        //    type: "GET",
        //    contentType: 'application/json; charset=UTF-8',
        //    data: input,
        //    dataType: "json",
        //    success: function (responseData) {
        //        if (responseData.IsSuccess) {
        //        } else {
        //            msg = msg + responseData.ErrorMessage;
        //        }
        msg += '.After that records cannot be modified</h5>';
        showConfirmationDialog(msg);
        //    },
        //    beforeSend: function () { showLoader(); },
        //    complete: function () { hideLoader(); },
        //    error: function (xhr, error, msg) {
        //        console.log(msg);
        //    }
        //});

        $('#confirmationDialog').off('click.confirmation', 'button').on('click.confirmation', 'button', function () {
            var $btn = $(this);
            hideConfirmationDialog();
            if ($btn.data('response') == 'yes') {
                var $form = $("#result-form");
                var ser = $form.serialize();
                if (ser.split("=").length > 2) {    // Use Ajax to submit form data
                    $.ajax({
                        url: $form.attr('action'),
                        type: 'POST',
                        data: $form.serialize() + "&IsFinalSubmit=true",
                        success: function (result) {
                            var $customform = createFormFromModel(input, $element.data('action'));
                            $('body').append($customform);
                            $customform.submit();
                        },
                        beforeSend: function () { showWaitingDialog(); },
                        complete: function () { hideWaitingDialog(); },
                    });
                }
                else { alert("Award Not Found."); }

            } else if ($btn.data('response') === 'no') {
            } else {
            }
        });



    });

    $(document).on('keydown', '.jsSearchOnRegistrationNo', function (event) {
        if (event.which === 13) {
            $(".fa-search-btn").trigger('click');
        }
    });

    $(document).on('click', '.jsDeleteSubject', function (event) {
        var confirmation = confirm("Are you sure ,you want to delete syllabus.");
        var tr = $(this).closest('tr');
        var id = tr.find("input.js_ID").val();
        if (confirmation) {
            var url = area + "Result/DeleteSubject";
            $.ajax({
                url: url + "?id=" + id,
                success: function (data) {
                    if (data === "1") {
                        tr.remove();
                    }
                },
                error: function (xhr, error, msg) {
                    console.log(msg);
                },
                beforeSend: function () { showLoader(); },
                complete: function () { hideLoader(); }
            });
        }
    });



});

//$(document).on('change', '#Subject_ID', function (event) {
//    $(".fa-search-btn").trigger("click");
//});
function SetCheckBoxState(loop) {
    var setValue = ($('#ÏsUpdated_' + loop + ':checkbox:checked').length > 0) ? '@CUSrinagar.Enums.RecordState.New' : '@CUSrinagar.Enums.RecordState.Old';
    $('#semesterModelList_' + loop + '__RecordStatus').val(setValue);
}

$(document).on('change', '.ÏsUpdated', function (event) {
    var loop = $(this).data("loop");
    SetCheckBoxState(loop);
});
$(document).on('submit', '#result-form', function (event) {

    var subjectId = $("#CombinationSubjects option:selected").val();
    $("#defaultSubjectId").val(subjectId);
    var UpdatedRecordCount = $('.js-table input.ÏsUpdated:checked').not('.hidden').length;
    if (UpdatedRecordCount > 0) {
        if (buttonpressed === "Final") {
            setTimeout(function () {
                if (confirm("Do you want to submit " + UpdatedRecordCount + " records.After final submit award will be locked.")) {
                    showWaitingDialog();
                    return;
                }
                else {
                    $('.js-table input.ÏsUpdated').attr('checked', false);
                    return false;

                }

            }, 20);
        }
        else {

            if (confirm("Do you want to update " + UpdatedRecordCount + " records?")) {
                showWaitingDialog();
                return;
            }
            else return false;
        }

    }
    else {
        alert("No record to update.Click on checkbox to update record");
        return false;
    }
});


$(document).on('click', '.subbmit', function (event) {
    var $form = $("#result-form");
    var input = {};
    input.Semester = $("#Semester").val();
    input.Programme = $("#Programme").val();
    input.MarksFor = $("#MarksFor").val();
    input.CombinationSubjects = $("#CombinationSubjects").val();
    input.IsBacklog = $("#IsBacklog").val();
    if (isNullOrEmpty(input.Semester)) {
        showErrorMessage('Please choose Semester.'); return;
    }
    if (isNullOrEmpty(input.CombinationSubjects)) {
        showErrorMessage('Please choose Subject.'); return;
    }
    if (!$form.valid()) {

        var $validator = $form.validate({ ignore: $form.find('tr :not(.jsDirtyRow) input'), focusInvalid: true }); // performance issue
        $($validator.errorList[0].element).select().focus();
        return;
    }
    $('.js-table input.ÏsUpdated').attr('checked', true);
    var UpdatedRecordCount = $('.js-table input.ÏsUpdated:checked').not('.hidden').length;
    if (UpdatedRecordCount > 0) {
        var msg = "<h5>Are you sure you want to Submit award";
        msg += '.After that records cannot be modified</h5>';
        showConfirmationDialog(msg);
        $('#confirmationDialog').off('click.confirmation', 'button').on('click.confirmation', 'button', function () {
            var $btn = $(this);
            hideConfirmationDialog();
            if ($btn.data('response') === 'yes') {
                var ser = $form.serialize();
                if (ser.split("=").length > 2) {    // Use Ajax to submit form data
                    $.ajax({
                        url: $form.attr('action'),
                        type: 'POST',
                        data: $form.serialize() + "&IsFinalSubmit=true",
                        success: function (result) {
                            var i = 0;
                            var stringArray = new Array();
                            $("tr").each(function () {
                                var student_id = $("#semesterModelList_" + i + "__Student_ID").val();
                                if (student_id !== null && $.trim(student_id) !== '') {
                                    stringArray[i] = student_id;
                                    i++;
                                }
                            });


                            var postData = stringArray;
                            $.ajax({
                                type: "POST",
                                url: "/CUCollegeAdminPanel/Award/FinalSubmit",
                                contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
                                data: jQuery.param({ list: postData, dt: input }),
                                success: function (data) {
                                    debugger;
                                    hideWaitingDialog();
                                    window.location.href = "/CUCollegeAdminPanel/Award/Award?Programme=" + input.Programme + "&SubjectId=" + input.CombinationSubjects + "&Semester=" + input.Semester + "&MarksFor=+ " + input.MarksFor;
                                },
                                beforeSend: function () { showWaitingDialog(); },
                                complete: function () { hideWaitingDialog(); },
                            });
                        },
                        beforeSend: function () { showWaitingDialog(); },
                        complete: function () { hideWaitingDialog(); },
                    });
                }
            }
        });
    }
    else {
        $('.js-table input.ÏsUpdated').attr('checked', false);
        return false;
    }
});
var buttonpressed = "Save";
$(document).on('click', '#Final', function (event) {
    buttonpressed = $(this).attr('id');
    $('.js-table input.ÏsUpdated').attr('checked', true);

});
var lastValueInput = '';
$(document).on('keyup paste', '.textboxSub', function (event) {
    var loop = $(this).data("loop");
    $('#ÏsUpdated_' + loop).prop('checked', true);
    SetCheckBoxState(loop);

});



$(document).on('change', '.ParentDDL', function (e) {

    var id = $(this).val();
    FillChildAward(id);

});
function FillChildAward(id) {
    if (id == null || id == "") { id = 0; }
    $.ajax({
        url: area + "Award/_GetchildDDL",
        data: {
            semester: id,
            programme: programmeIS
        },
        datatype: "json",
        contentType: "application/json; charset=utf-8",
        encode: true,
        type: "Get",
        cache: false,
        async: false,
        success: function (response) {
            var PartialView = response;
            $('#Subject').html(PartialView); // Modify me
            //  ChosenStyle();
            //BindParentClick();
        },
        error: function (response) {
        }
    });
}


function ShowORHideSubmit() {
    var mininternalmarks = 0;
    if (parseInt(MinMarksInternal) > 0) { mininternalmarks = parseInt(MinMarksInternal); }
    var minexternalmarks = 0;
    if (parseInt(MinMarksExternal) > 0) { minexternalmarks = parseInt(MinMarksExternal); }
    if (mininternalmarks > 0 && minexternalmarks > 0) {
        $(".min_marks").html("Internal Min Marks(Int):" + MinMarksInternal + "&nbsp;&nbsp;External Min Marks(Ext):" + MinMarksExternal);
    }
    else if (mininternalmarks > 0) {
        $(".min_marks").html("Internal Min Marks:" + MinMarksInternal);
    }
    else if (minexternalmarks > 0) {
        $(".min_marks").html("External Min Marks:" + MinMarksExternal);
    }
    $(".submitAwardBtn").removeClass("hidden");
    $(".GenerateEvalBill").addClass("hidden");

    if (isAwardSubmitted === "True") {
        $(".submitAwardBtn").addClass("hidden");
        $(".GenerateEvalBill").removeClass("hidden");
    }
}
$(document).on('click', '.JSDownloadAwardBtn', function (e) {
    e.preventDefault();
    var semis = true;
    var Sem = $("#Semester option:selected").val();
    var subis = true;
    var Subject_ID = $("#CombinationSubjects option:selected").val();
    var Batch = $("#Year").val();
    var batchis = true;
    var haveback = false;
    if ($('#IsBacklog').length) {
        haveback = $("#IsBacklog").val();
    }

    var isValidated = true;
    if (!Sem) {
        $("#SemesterValidator").html("Required");
        semis = false;
    } else {
        semis = true;
        $("#SemesterValidator").html("");
    }
    if (!Batch) {
        batchis = false;
        $("#BatchValidator").html("Required");
    }
    else {
        batchis = true;
        $("#BatchValidator").html("");
    }
    if (!Subject_ID) {
        subis = false;
        $("#CombinationSubjectsValidator").html("Required");
    }
    else {
        subis = true;
        $("#CombinationSubjectsValidator").html("");
    }

    if (subis && batchis && semis) {

        var url = area + "Award/PrintAward/" + Batch;
        url += "/" + Sem;

        url += "/" + Subject_ID;
        url += "?id3=" + haveback;
        url += "&Programme=" + programmeIS;

        window.open(url, '_blank');



    }

    return false;

});
$(document).on('keyup', '.jsinteger', function (e) {
    var val = $(this).val();
    var obj = $(this);

    if (val != "") {
        val = parseInt(val);
        if (!isNaN(val)) {
            if (Number(val) < -3) {
                $(this).val("");
                return;
            }
            $.ajax({
                url: area + "Award/ConvertNumericToWord",
                type: 'Get',
                data: { number: val },
                success: function (result) {
                    var wordform = result;
                    $(obj).parent().nextAll('span:first').html("<span> (" + wordform + ")</span>");
                },
            });
        }
    }

    $(obj).nextAll('span:first').html("<span> </span>");

});
$(document).on('focusout', 'tr', function (e) {
    var tr = $(this);
    var vali = 0;
    var vale = 0;
    $(tr).find('input.intClass').each(function () {
        vali = vali + parseInt($(this).val());
    });
    $(tr).find('input.extClass').each(function () {
        vale = vale + parseInt($(this).val());
    });
    var mininternalmarks = 0;
    if (parseInt(MinMarksInternal) > 0) { mininternalmarks = parseInt(MinMarksInternal); }
    var minexternalmarks = 0;
    if (parseInt(MinMarksExternal) > 0) { minexternalmarks = parseInt(MinMarksExternal); }
    if (vali < mininternalmarks || vale < minexternalmarks) {
        $(tr).css("background-color", "#e05a5a");
    }
    else { $(tr).css("background-color", "#FFFFF)"); }
});
$(document).on('focusin', 'tr', function (e) {
    var tr = $(this);
    $(tr).css("background-color", "#FFFFF)");
});




$(document).ready(function () {

    $(document).on('change', '.jsNewResult', function () {
        var $element = $(this);
        var $td = $element.closest('td');
        if ($element.is(':checked')) {
            $td.find('[name=ResultNotification_ID]').val('');
            $td.find('[name=ExamForm_ID]').val('');
        } else {
            $td.find('[name=ResultNotification_ID]').val($td.find('.jsResultNotification_ID').val());
            $td.find('[name=ExamForm_ID]').val($td.find('.jsExamForm_ID]').val());
        }
    });





});

function saveRapidEntryRow($tr, resolve, reject) {
    var _model = createModelFromForm($tr);
    var programme = $("#Programme").val();
    var subject_id = $("#CombinationSubjects").val();
    var semester = $("#Semester").first().val();
    var _url = getBaseUrlCollege() + `Award/PostResultListItem?SubjectID=${subject_id}&Semester=${semester}&programme=${programme}&marksFor=2`;

    $.ajax({
        url: _url,
        type: "POST",
        data: { model: _model },
        success: function (response) {
            if (response.IsSuccess) {
                bindModelPropertiesToForm($tr, response.ResponseObject);
                showSuccessAlertMessage(response.SuccessMessage, 500);
                resolve($tr);
            } else {
                showErrorMessage(response.ErrorMessage);
                reject($tr);
            }
        },
        error: function (xhr, error, msg) {
            showErrorMessage(msg);
            reject($tr);
        },
        complete: function () { }
    });
}


function validateRows($pagertable) {
    if ($pagertable != null && $pagertable.length > 0 && $pagertable.find('form').length > 0) {
        generalFormValidation();
    }
}

$(document).on('click', '.JSGenerateEvalBill', function () {
    var ans = confirm("Are you sure you want to generate bill for this paper?");
    var _url = getBaseUrlAdmin() + "BillEvaluator/GenerateEvaluatorBill";
    var Subject_ID = $("#SubjectID").val();
    if (ans && Subject_ID != undefined) {
        $.post(_url, { Subject_ID: Subject_ID },
            function (response) {
                if (response.Item1)
                    showSuccessMessage(response.Item2, 1000);
                else
                    showErrorMessage(response.Item2, 1000);
            }).fail(function () {
                showErrorMessage("Some error occured", 1000);
            });
        return true;
    } else {
        return false;
    }
});


