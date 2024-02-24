$(document).ready(function () {
    if ($("#StudentSearchCombinationInfoForm").length > 0) {
        bindDetailFormViewEvents();
    }
    if ($('#Sem_I_CombinationForm').length > 0) {
        bindSemester_I_Events();
    } else if ($("#CombinationForm").length > 0) {
        bindCombinationFormEvents();
    } else if ($("#std-comb-list").length > 0) {
        bindCourseWiseScreenEvents();
    } else if ($("#combination-wise-list").length > 0) {
        bindCombinationWizeScreenEvents();
    } else if ($("#subject-wise-list").length > 0) {
        bindsubjectWizeScreenEvents();
    }
});

function bindCombinationFormEvents() {
    $("#CombinationForm").submit(function () { return false; });
    $("#jsSaveUpdateCombinationWithRollNumber,#ClassRollNo").keypress(function (e) {
        if (e.which == "13") {
            $("#jsSaveUpdateCombinationWithRollNumber").trigger('click');
        }
    });
    $("#jsSaveUpdateCombinationWithRollNumber").click(function () {
        var $form = $("#CombinationForm");
        var combinationSetting = JSON.parse($("#CombinationSetting").val());
        var listOfSubjectAssigned = [];
        var milSubject = "";
        if (!combinationSetting.SameAsPrevSemester) {
            $(".jsCoreSubjectSection").each(function (index, section) {
                var $section = $(section);
                if ($section.find('input[type=checkbox]:checked').length > 0) {
                    var subject = { Subject_ID: $section.find('.jsCoreSubject_ID').val(), subjectType: 'Core', SubjectFullName: $section.find('.jsCoreSubjectID').val() }; //1
                    listOfSubjectAssigned.push(subject);
                    if (subject.SubjectFullName.toLowerCase().indexOf('mil') >= 0) {
                        milSubject = subject.SubjectFullName;
                    }
                }
            });
            //3rd semester section for mil subjects
            if (combinationSetting.ExcludeMilSubject) {
                if (milSubject.length > 0) {
                    showErrorMessage("MIL Subjects(" + milSubject + ") cannot be opted in 3rd sem");
                    return false;
                }
            }
            //end of 3rd sem section

            //Section: Total core subjects from previous semester
            if (combinationSetting.CoreCourseCount > 0) {
                if (listOfSubjectAssigned.length != combinationSetting.CoreCourseCount) {
                    showErrorMessage("Number of core subjects should be " + combinationSetting.CoreCourseCount);
                    //return false;
                }
            }
            //End core section

            //Compulsary subjects in a course
            //$(combinationSetting.CompulsarySubject).each(function (index, subject) {
            //    listOfSubjectAssigned.push(subject);
            //});
            //end compulsary section


            //generic/open elective subjects
            var totalGEopted = 0;
            for (var i = 0; i < combinationSetting.GenericElectiveCourseCount; i++) {
                var $SubjectOption = $($form.find(".jsGenericElectiveSubject select")[i]).find('option:selected');
                if (!isNullOrEmpty($SubjectOption.val()) && $SubjectOption.val().length > 0) {
                    listOfSubjectAssigned.push({ Subject_ID: $SubjectOption.val(), SubjectFullName: $SubjectOption.text(), subjectType: $SubjectOption.data('subjecttype') || "GenericElective" }); //3
                    totalGEopted++;
                }
            }
            if (combinationSetting.GenericElectiveCourseCount > 0 && totalGEopted <= 0) {
                showErrorMessage("Number of general elective subjects should be " + combinationSetting.GenericElectiveCourseCount);
                //return false;
            }
            //end section generic elective


            //skill Enhance subjects
            var totalSkillopted = 0;
            for (var i = 0; i < combinationSetting.SkillEnhancementCourseCount; i++) {
                var $SubjectOption = $($form.find(".jsSkillEnhanceSubject select")[i]).find('option:selected');
                if (!isNullOrEmpty($SubjectOption.val()) && $SubjectOption.val().length > 0) {
                    listOfSubjectAssigned.push({ Subject_ID: $SubjectOption.val(), SubjectFullName: $SubjectOption.text(), subjectType: 'SkillEnhancement' }); //3
                    totalSkillopted++;
                }
            }
            if (combinationSetting.SkillEnhancementCourseCount > 0 && totalSkillopted <= 0) {
                showErrorMessage("Number of Skill Enhance Subjects should be " + combinationSetting.SkillEnhancementCourseCount);
                //return false;
            }
            //end section skill enhance

            //Descipline Specific Elective subjects
            //var totalDSEopted = 0;
            //for (var i = 0; i < combinationSetting.DesciplineSpecificElectiveCourseCount; i++) {
            //    var $SubjectOption = $($form.find(".jsDesciplineSpecificElective select")[i]).find('option:selected');
            //    if ($SubjectOption.val().length > 0) {
            //        listOfSubjectAssigned.push({ Subject_ID: $SubjectOption.val(), SubjectFullName: $SubjectOption.text(), subjectType: 'DSE' });
            //        totalDSEopted++;
            //    }
            //}
            //if (combinationSetting.DesciplineSpecificElectiveCourseCount > 0 && totalDSEopted <= 0) {
            //    showErrorMessage("Number of Descipline Specific Elective subjects should be " + combinationSetting.DesciplineSpecificElectiveCourseCount);
            //    return false;
            //}
            //end section skill enhance


            //Descipline Specific Elective subjects
            var totalDCEopted = 0;
            for (var i = 0; i < combinationSetting.DesciplineCentricElectiveCourseCount; i++) {
                var $SubjectOption = $($form.find(".jsDesciplineCentricElective select")[i]).find('option:selected');
                if (!isNullOrEmpty($SubjectOption.val()) && $SubjectOption.val().length > 0) {
                    listOfSubjectAssigned.push({ Subject_ID: $SubjectOption.val(), SubjectFullName: $SubjectOption.text(), subjectType: 'DCE' });
                    totalDCEopted++;
                }
            }
            if (combinationSetting.DesciplineCentricElectiveCourseCount > 0 && totalDCEopted <= 0) {
                showErrorMessage("Number of Descipline Specific Elective subjects should be " + combinationSetting.DesciplineCentricElectiveCourseCount);
                //return false;
            }
            //end section skill enhance

            //Optional Core subjects
            var totalOCopted = 0;
            for (var i = 0; i < combinationSetting.OptionalCoreCourseCount; i++) {
                var $SubjectOption = $($form.find(".jsOptionalCoreCourse select")[i]).find('option:selected');
                if (!isNullOrEmpty($SubjectOption.val()) && $SubjectOption.val().length > 0) {
                    listOfSubjectAssigned.push({ Subject_ID: $SubjectOption.val(), SubjectFullName: $SubjectOption.text(), subjectType: 'OC' });
                    totalOCopted++;
                }
            }
            if (combinationSetting.OptionalCoreCourseCount > 0 && totalOCopted <= 0) {
                showErrorMessage("Number of Descipline Specific Elective subjects should be " + combinationSetting.OptionalCoreCourseCount);
                //return false;
            }
            //end section skill enhance

            if (combinationSetting.TotalCreditSet == true) {
                var totalcredit;//calculate subject credits
                if (totalcredit != combinationSetting.TotalCredit) {
                    showErrorMessage(`Total credit should be equal to ${combinationSetting.TotalCreditSet} credits.`);
                    //return false;
                }
            }
            var listOfAddionalSubjectAssigned = [];
            for (var i = 0; i < combinationSetting.AdditionalCourseCount; i++) {
                var $SubjectOption = $($form.find(".jsAdditionalCourse select")[i]).find('option:selected');
                if (!isNullOrEmpty($SubjectOption.val()) && $SubjectOption.val().length > 0) {
                    listOfAddionalSubjectAssigned.push({ Subject_ID: $SubjectOption.val(), SubjectFullName: $SubjectOption.text(), subjectType: 'OC' });
                }
            }
        }
        if (!$form.valid()) {
            return false;
        }
        postSubjects($form, listOfSubjectAssigned, listOfAddionalSubjectAssigned);
    });


}

function bindCombinationWizeScreenEvents() {

}

function bindSemester_I_Events() {
    getCombinationCount();
    $("#jsSaveUpdateCombination,#ClassRollNo").keypress(function (e) {
        if (e.which == "13") {
            $("#jsSaveUpdateCombination").trigger('click');
        }
    });
    $("#jsCourseDDL").change(function () {
        //getCombinationDDL();
        getCombinationCount();
    });

    $(document).on("change", '#jsCombinationDDL', function () {
        getCombinationCount();
    });

    $("#jsSaveUpdateCombination").click(function () {
        var $form = $('#Sem_I_CombinationForm');
        insertUpdateCombination($form);
    });


    $("#js-release-student").click(function () {
        var msg = '<h4>Are you sure you want to delete this student from your college ?</h4>';
        msg += '<h5>Once deleted this student, corresponding student has to reassign all its subject combination details</h5>';
        showConfirmationDialog(msg);
        $('#confirmationDialog').off('click.confirmation', 'button').on('click.confirmation', 'button', function () {
            var $btn = $(this);
            hideConfirmationDialog();
            if ($btn.data('response') == 'yes') {
                var _student_id = $("#Student_ID").val();
                var _printProgramme = $("#printProg option:selected").val();
                DeleteStudentCombinations(_student_id, _printProgramme);
            }
        });
    });

    function DeleteStudentCombinations(_student_id, _printProgramme) {
        var _url = getBaseUrlCollege() + "AssignCombination/ReleaseStudent";
        $.ajax({
            url: _url,
            type: "POST",
            datatype: "Json",
            async: false,
            data: { Student_ID: _student_id, printProgramme: _printProgramme },
            success: function (responseData) {
                if (responseData.IsSuccess) {
                    showSuccessMessage(responseData.NumberOfRecordsEffected + ' deleted successfully.');
                    $("#js-release-student").hide();
                    $("#jsEditLinkContainer").hide();
                    var fieldInput = $('#Form_RegistrationNumber');
                    var fldLength = fieldInput.val().length;
                    fieldInput.focus();
                    fieldInput[0].setSelectionRange(fldLength, fldLength);
                } else {
                    showErrorMessage(responseData.ErrorMessage);
                }
            },
            error: function (xhr, error, msg) {
                showErrorMessage(msg);
            }
        });
    }

    function insertUpdateCombination($form) {
        var _SelectedCombination = {};
        _SelectedCombination.PrintProgramme = $form.find('#PrintProgramme').val();
        _SelectedCombination.College_ID = $form.find('#College_ID').val();
        _SelectedCombination.Student_ID = $form.find('#Student_ID').val();
        _SelectedCombination.SelectedCombination_ID = $form.find('#SelectedCombination_ID').val();
        _SelectedCombination.Course_ID = $form.find('#jsCourseDDL option:selected').val();
        _SelectedCombination.CourseID = $form.find('#jsCourseDDL option:selected').text();
        _SelectedCombination.Combination_ID = $form.find('#jsCombinationDDL option:selected').val();
        _SelectedCombination.CombinationID = $form.find('#jsCombinationDDL option:selected').text();
        _SelectedCombination.CreatedBy = $form.find('#CreatedBy').val();
        _SelectedCombination.CreatedOn = $form.find('#CreatedOn').val();
        _SelectedCombination.UpdatedBy = $form.find('#UpdatedBy').val();
        _SelectedCombination.UpdatedOn = $form.find('#UpdatedOn').val();
        _SelectedCombination.Semester = $form.find('#Semester').val();

        var hasError = false;
        if (_SelectedCombination.Course_ID == "") {
            showErrorMessage("Please choose course."); return;
        }
        if (_SelectedCombination.Combination_ID == "") {
            showErrorMessage("Please choose combination."); return;
        }
        //if (hasError) {
        //    return;
        //}
        if ($("#jsSaveUpdateCombination").hasClass('disabled')) { return; }
        $("#jsSaveUpdateCombination").addClass('disabled');
        var _ClassRollNo = $("#ClassRollNo").val();
        var _OverrideExistingClassRollNo = $("#OverrideExistingClassRollNo").is(':checked');


        var _url = getBaseUrlCollege() + "AssignCombination/InsertUpdateCombination?ClassRollNo=" + _ClassRollNo + "&OverrideClassRollNumber=" + _OverrideExistingClassRollNo + "&fromAcceptExamFormScreen=" + $("#fromAcceptExamFormScreen").val();
        $.ajax({
            url: _url,
            type: "POST",
            contentType: 'application/json; charset=UTF-8',
            data: JSON.stringify(_SelectedCombination),
            success: function (responseData) {
                if (responseData.IsSuccess) {
                    showSuccessMessage(responseData.NumberOfRecordsEffected + " Record saved successfully.");
                    $("#jsEditLinkContainer").show();
                    $("#js-release-student").show();
                    //$("#Form_RegistrationNumber").focus();
                    var fieldInput = $('#Form_RegistrationNumber');
                    var fldLength = fieldInput.val().length;
                    fieldInput.focus();
                    fieldInput[0].setSelectionRange(fldLength, fldLength);

                }
                if (!isNullOrEmpty(responseData.ErrorMessage)) {
                    showErrorMessage(responseData.ErrorMessage);
                }
            },
            beforeSend: function () { showLoader(); },
            complete: function () {
                hideLoader();
                $("#jsSaveUpdateCombination").removeClass('disabled');
            },
            error: function (xhr, error, msg) {
                showErrorMessage(msg);
            }
        });
    }

    function getCombinationCount() {
        var Combination_ID = $("#jsCombinationDDL option:selected").val();
        if (isNullOrEmpty(Combination_ID) || Combination_ID == emptyGuid()) {
            $("#js-subject-comb-count").html("00");
            return;
        }
        var semester = $("#sem option:selected").val();
        var printProgramme = $("#printProg option:selected").val();
        var batch = $("#Batch").val();
        var _url = getBaseUrlCollege() + "AssignCombination/GetSubjectCombinationCount";
        $.ajax({
            url: _url,
            type: "POST",
            datatype: "Json",
            data: { Combination_ID: Combination_ID, Semester: semester, Batch: batch, printProgramme: printProgramme },
            success: function (data) {
                if (parseInt(data) > 0) {
                    $("#js-subject-comb-count").html(data);
                } else {
                    $("#js-subject-comb-count").html('00');
                }
            }
        });
    }


    //$('.js-courses').change(function () {
    //    var $select = $(this);
    //    var $tr = $select.closest('tr');
    //    fillSubjectCombinationDDL($tr);
    //    if ($select.val() == "") {
    //        setSubjectCombinationCount($tr, 00);
    //    } else {
    //        getSubjectCombinationCount($tr);
    //    }
    //    $tr.addClass('jsDirty');
    //});

    //$(document).on('change', '.js-subject-comb', function () {
    //    var $select = $(this);
    //    var $tr = $select.closest('tr');
    //    if ($select.val() == "" || $select.val() == emptyGuid()) {
    //        setCombinationStatus($tr, false);
    //        setSubjectCombinationCount($tr, 00);
    //    } else {
    //        setCombinationStatus($tr, true);
    //        getSubjectCombinationCount($tr);
    //    }
    //    $tr.addClass('jsDirty');
    //});

    //$(".js-comb-section tbody").on('change', 'input[type=checkbox]', function () {
    //    var $checkbox = $(this);
    //    var $tr = $checkbox.closest('tr');
    //    if ($checkbox.is(':checked'))
    //        setCombinationStatus($tr, true);
    //    else
    //        setCombinationStatus($tr, false);
    //});

    //function getCombinationDDL() {
    //    var $targetSelect = $("#jsCombinationDDL");
    //    clearDDOptions($targetSelect);
    //    fillDDLDefaultOption($targetSelect);
    //    clearDDOptions($targetSelect);
    //    var College_ID = $(".jsCollege").val();
    //    var Course_ID = $("#jsCourseDDL").find('option:selected').val();
    //    var semester = $("select#sem").find('option:selected').val();
    //    var _url = `/CUSrinagarAdminPanel/General/CombinationDDL?College_ID=${College_ID}&Course_ID=${Course_ID}&semester=${semester}`;
    //    $.ajax({
    //        url: _url,
    //        type: "POST",
    //        datatype: "Json",
    //        success: function (data) {
    //            fillDDLOptions($targetSelect, data);
    //            ChosenStyle();
    //            resizechosen($targetSelect);
    //        },
    //        error: function (xhr, error, msg) {
    //            fillDDLOptions($targetSelect, null);
    //            showErrorMessage(msg);
    //        }
    //    });
    //}



}


function bindCourseWiseScreenEvents() {
    //loadpagertableWithDefaultParams();
    resetMultiSelect();
    //$("#Programme").change(function () {
    //    var $select = $(this);
    //    var selectedValue = $select.find('option:selected').val();
    //    $select.closest('.js-pager-table').data('otherparam1', selectedValue);
    //    $(".js-exportToCSV").data('otherparam1', 'printProgramme=' + selectedValue);
    //});

    $('#jsToggleVerifyCombination').click(function () {

        var studentlist = getEntityList();
        var printProgramme = $("#Programme").find('option:selected').val();
        var _sem = $(".js-pager-table .jsSemesterDDL").find('option:selected').val();
        var _url = getBaseUrlCollege() + "AssignCombination/ToggleVerifyCombination?printProgramme=" + getPrintProgrammeFromProgramme(printProgramme) + "&Semester=" + _sem;
        var model = JSON.parse(JSON.stringify(studentlist));
        $.ajax({
            url: _url,
            type: "POST",
            contentType: 'application/json; charset=UTF-8',
            data: JSON.stringify(studentlist),
            dataType: "json",
            success: function (data) {
                if (data.IsSuccess)
                    showSuccessMessage(data.SuccessMessage);
                showErrorMessage(data.ErrorMessage, 10000);
                $(".js-search").click();
            },
            beforeSend: function () { showLoader(); },
            complete: function () {
                hideLoader();
            },
            error: function (xhr, error, msg) {
            }
        });
    });

    $('#jsShowBatchUpdateListItem').click(function () {
        var validateMessage = validateBatchFilters();
        if (!isNullOrEmpty(validateMessage)) {
            showErrorMessage(validateMessage);
            return;
        }
        $("#BatchUpdateCourseWiseDailog").modal('show');
    });

    $('#jsShowBatchUpdateNextSemsterCombListItem').click(function () {
        var validateMessage = validateBatchFilters();
        var $batchDialog = $("#BatchUpdateNextSemCombDailog");

        if (!isNullOrEmpty(validateMessage)) {
            showErrorMessage(validateMessage);
            return;
        }
        $batchDialog.find('.jsSemesterDDL').val('').trigger('chosen:updated');
        $batchDialog.find('.jsCombinationDDL').val('').trigger('chosen:updated');
        $("#BatchUpdateNextSemCombDailog").modal('show');
    });

    $('#jsBatchUpdateCourseWiseListBtn').click(function () {
        var $batchDialog = $("#BatchUpdateCourseWiseDailog");
        var validateMessage = validateBatchFilters();

        var selectedOperations = $batchDialog.find(".jsBatchChkBox:checked");
        if (selectedOperations.length == 0) {
            showAlertDialog('Please check the checbox for the operation to update...');
            return false;
        }

        var _OldCombination_ID = $(".js-pager-table .jsCombinationDDL").find('option:selected').val();
        var _NewCombination_ID = $batchDialog.find("#CombinationContainer select option:selected").val();
        if (isNullOrEmpty(_NewCombination_ID)) {
            showAlertDialog("choose new combination to be assigned");
            return;
        }

        //if (_NewCombination_ID == _OldCombination_ID) {
        //    showAlertDialog("Similar combinations chosen");
        //    return;
        //}

        $batchDialog.modal('hide');
        var studentlist = getEntityList();
        var msg = '<h4>Are you sure you want to update info of selected students</h4>';
        msg += '<h6>Number of students:  <b>' + studentlist.length + '</b></h6>';
        msg += '<h6>Semester:  <b>' + $("#Semester").find('option:selected').val() + '</b></h6>';
        msg += '<h6>Course:  <b>' + $("#Course").find('option:selected').text() + '</b></h6>';
        msg += '<h6>Old Combination: <b>' + (isNullOrEmpty($("#Combination").find('option:selected').val()) ? "Choosen student's combination" : $("#Combination").find('option:selected').text()) + '</b></h6>';
        msg += '<h6>New Combination:  <b>' + $batchDialog.find("#CombinationContainer select option:selected").text() + '</b></h6>';
        showConfirmationDialog(msg);
        $('#confirmationDialog').off('click.confirmation', 'button').on('click.confirmation', 'button', function () {
            var $btn = $(this);
            hideConfirmationDialog();
            if ($btn.data('response') == 'yes') {
                var BatchPersonalInfoUpdate = {};
                BatchPersonalInfoUpdate.Student_ID = studentlist;
                BatchPersonalInfoUpdate.Combination = { OldCombination_ID: _OldCombination_ID, NewCombination_ID: _NewCombination_ID, Semester: $('.js-pager-table').find(".jsSemesterDDL").find('option:selected').val() };
                var printProgramme = $("#Programme").find('option:selected').val();
                BatchUpdateSemesterCombination(BatchPersonalInfoUpdate, printProgramme);
            } else if ($btn.data('response') == 'no') {

            } else {

            }
        });
    });

    $('#jsBatchUpdateNextSemCombDailogBtn').click(function () {
        var $batchDialog = $("#BatchUpdateNextSemCombDailog");

        var selectedOperations = $batchDialog.find(".jsBatchChkBox:checked");
        if (selectedOperations.length != 2) {
            showAlertDialog('Please check the checbox for the operation to update...');
            return false;
        }

        var _PrevSem = $(".js-pager-table .jsSemesterDDL").find('option:selected').val();
        var _NextSemester = $batchDialog.find('.jsSemesterDDL').find('option:selected').val();
        var _SemesterBatch = $('#BatchDDL').find('option:selected').val();

        if (isNullOrEmpty(_NextSemester)) {
            showAlertDialog("choose semester");
            return;
        }

        if ((parseInt(_NextSemester) - parseInt(_PrevSem)) != 1 && (parseInt(_NextSemester) - parseInt(_PrevSem)) != 0) {
            showAlertDialog("Next semester value should be one or  greater than one by previous semester");
            return;
        }

        var _NewCombination_ID = $batchDialog.find(".jsCombinationDDL option:selected").val();
        if (isNullOrEmpty(_NewCombination_ID)) {
            showAlertDialog("choose new combination to be assigned");
            return;
        }

        var _OldCombination_ID = $(".js-pager-table .jsCombinationDDL").find('option:selected').val();
        if (_NewCombination_ID == _OldCombination_ID) {
            showAlertDialog("Similar combinations chosen");
            return;
        }

        $batchDialog.modal('hide');
        var studentlist = getEntityList();
        var msg = '<h4>Are you sure you want to update info of selected students</h4>';
        msg += '<h5>Number of students:  <b>' + studentlist.length + '</b></h6>';
        msg += '<h6>Update Semester ' + _PrevSem + ' to semester:  <b>' + _NextSemester + '</b></h6>';
        msg += '<h6>Course:  <b>' + $("#Course").find('option:selected').text() + '</b></h6>';
        msg += '<h6>Old Combination: <b>' + (isNullOrEmpty($(".js-pager-table .jsCombinationDDL").find('option:selected').val()) ? "Choosen student's combination" : $(".js-pager-table .jsCombinationDDL").find('option:selected').text()) + '</b></h6>';
        msg += '<h6>New Combination:  <b>' + $batchDialog.find(".jsCombinationDDL option:selected").text() + '</b></h6>';
        showConfirmationDialog(msg);
        $('#confirmationDialog').off('click.confirmation', 'button').on('click.confirmation', 'button', function () {
            var $btn = $(this);
            hideConfirmationDialog();
            if ($btn.data('response') == 'yes') {
                var BatchPersonalInfoUpdate = {};
                BatchPersonalInfoUpdate.Student_ID = studentlist;
                BatchPersonalInfoUpdate.Combination = { OldCombination_ID: _OldCombination_ID, NewCombination_ID: _NewCombination_ID, Semester: _NextSemester, SemesterBatch: _SemesterBatch };
                var printProgramme = $("#Programme").find('option:selected').val();
                BatchUpdateNextSemesterCombination(BatchPersonalInfoUpdate, printProgramme);
            } else if ($btn.data('response') == 'no') {

            } else {

            }
        });
    });
}

function bindDetailFormViewEvents() {

    $("#StudentSearchCombinationInfoForm").submit(function () {
        var semester = parseInt($("#sem").find('option:selected').val()) || 0;
        if (semester == 1) {
            $("#StudentSearchCombinationInfoForm").attr('action', '/CUCollegeAdminPanel/AssignCombination/ViewFormDetail');
        } else if (semester > 1) {
            $("#StudentSearchCombinationInfoForm").attr('action', '/CUCollegeAdminPanel/AssignCombination/CombinationRollNumber');
        }
    });

    //$("#sem").change(function () {


    //});


    //$(".js-search-form").submit(function () {
    //    showLoader();
    //    return true;
    //});

    //if ($("#frmStudentSubjectCombination").length > 0) {
    //    //setTimeout(function () { $('#SkillEnhanceSubject1').next(".chosen-container").focus(); },100); 
    //}

    //$('#SkillEnhanceSubject1').change(function () {
    //    var $select = $(this);
    //    var credit = parseInt($select.find('option:selected').data('credit')) || 4;
    //    //if ($("#SkillEnhanceSubject2 option:selected").val().length>0 && $("#SkillEnhanceSubject2 option[value='" + $select.find('option:selected').val() + "']").length > 0) {
    //    //    showErrorMessage("Duplicate Subjects");
    //    //    return false;
    //    //} 
    //    if (credit < 4) {
    //        //$('#SkillEnhanceSubject2').closest('tr').removeClass('hidden');
    //    } else {
    //        //$("#SkillEnhanceSubject2 option[value='']").attr('selected','selected');
    //        //$('#SkillEnhanceSubject2').closest('tr').addClass('hidden');
    //    }
    //});
    //$('#SkillEnhanceSubject2').change(function () {
    //    var $select = $(this);
    //    var credit = parseInt($select.find('option:selected').data('credit')) || 4;
    //    //if ($("#SkillEnhanceSubject1 option:selected").val().length > 0 && $("#SkillEnhanceSubject1 option[value='" + $select.find('option:selected').val() + "']").length > 0) {
    //    //    showErrorMessage("Duplicate Subjects");
    //    //    return false;
    //    //} 
    //    if (credit < 4) {
    //        //$('#SkillEnhanceSubject1').closest('tr').removeClass('hidden');
    //    } else {
    //        //$("#SkillEnhanceSubject1 option[value='']").attr('selected', 'selected');
    //        //$('#SkillEnhanceSubject1').closest('tr').addClass('hidden');
    //    }
    //});

    //$(".js-search-formdetail").click(function () {
    //    showLoader();
    //    $(".js-search-form").submit();
    //});



    //$(".js-checkAll").change(function () {
    //    var $checkbox = $(this);
    //    var tbody = $checkbox.closest('table').find('tbody');
    //    if ($checkbox.is(':checked')) {
    //        tbody.find('input[type="checkbox"]').attr('checked', 'checked').prop('checked', 'checked').trigger('change');
    //    } else {
    //        tbody.find('input[type="checkbox"]').attr('checked', false).prop('checked', false).trigger('change');
    //    }
    //});

    //$(".js-update-formDetails").click(function () {
    //    var $combSection = $(".js-comb-section tbody");
    //    //if ($(".js-update-formDetails").hasClass('disabled')) return;
    //    //$(".js-update-formDetails").addClass('disabled');
    //    var $form = $(this).closest('form');
    //    postCombinationDetails($form);
    //});



    //$('.js-comb-section tbody td:not(.js-draggable)').mousedown(function (event) {
    //    event.stopImmediatePropagation();
    //    return false;
    //});

    //$(".js-comb-section tbody").sortable();

    //$(".js-register-student").click(function () {

    //    if ($("#AcceptCollege_ID").val() != "" && $("#AcceptCollege_ID").val() != emptyGuid() && $('#LoggedInCollege_ID').val() != $("#AcceptCollege_ID").val()) {
    //        showErrorMessage("This student has already been registered in another college.");
    //        return false;
    //    }
    //    var $combSection = $(".js-comb-section tbody");
    //    //$(".js-update-formDetails").addClass('disabled'); // disabled combination button
    //    //if ($(".js-register-student").hasClass('disabled')) { $(".js-update-formDetails").removeClass('disabled'); return; }
    //    //$(".js-register-student").addClass('disabled'); // disabled register button        

    //    var selectedRowCount = $combSection.find("tr input:checked");
    //    if (selectedRowCount.length == 0) {
    //        showErrorMessage("Please choose atleast one subject combination");
    //        //$(".js-update-formDetails,.js-register-student").removeClass('disabled');
    //        return false;
    //    } else if (selectedRowCount.length > 1) {
    //        //showErrorMessage("Please choose atmost one subject combination");
    //        //$(".js-update-formDetails,.js-register-student").removeClass('disabled');
    //        //return false;
    //    }
    //    var courseid, combinationid, count, combination_id;
    //    $combSection.find("tr input:checked").each(function (index, checkbox) {
    //        var $tr = $(checkbox).closest('tr');
    //        courseid = $tr.find(".js-courses option:selected").text();
    //        combination_id = $tr.find(".js-subject-comb option:selected").val();
    //        combinationid = $tr.find(".js-subject-comb option:selected").text();
    //        //count = $tr.find('.js-subject-comb-count').html();
    //    });

    //    var msg = '<h5>Are you sure you want to save/update student info.</h5>';
    //    //msg += '<h6>Course: - <b>' + courseid + '</b></h6>';
    //    //msg += '<h6>Combination: - <small><b>' + combinationid + '</b></small></h6>';
    //    //msg += '<h6>No. of student added to this combination: - <b class="js-registered-std-count">' + 00 + '</b></h6>';
    //    showConfirmationDialog(msg);
    //    //getStudentRegisteredCount(combination_id);

    //    $('#confirmationDialog').off('click.confirmation', 'button').on('click.confirmation', 'button', function () {
    //        var $btn = $(this);
    //        hideConfirmationDialog();
    //        if ($btn.data('response') == 'yes') {
    //            postCombinationDetails($(".js-comb-section tbody"), true);
    //        } else if ($btn.data('response') == 'no') {
    //            //$(".js-update-formDetails,.js-register-student").removeClass('disabled');
    //        } else {
    //            //$(".js-update-formDetails,.js-register-student").removeClass('disabled');
    //        }
    //    });
    //});

    //$("#ClassRollNo").keypress(function (e) {
    //    if (e.which == "13") {
    //        $(".jsSaveStudentComb").trigger('click');
    //    }
    //});


    getSubjectCombinationsCount();
}


function getFormDetail(_formNumber) {
    var _url = getBaseUrlCollege() + "AssignCombination/ViewFormDetailPartial?formNumber=" + _formNumber;
    $.ajax({
        url: _url,
        data: { formNumber: _formNumber },
        type: 'Get',
        dataType: 'html',
        contentType: 'application/json; charset=UTF-8',
        success: function (html) {
            $(".js-form-detail").html(html);
        },
        beforeSend: function () { showLoader(); },
        complete: function () { hideLoader(); }
    });
}

//function fillSubjectCombinationDDL($tr) {
//    var course_id = $tr.find(".jsCourseDDL option:selected").val();
//    if (course_id == "" || course_id == emptyGuid()) {
//        setSubjectCombDDL($tr, null);
//        return;
//    }
//    var courseCode = $("#LoggedInCollegeID").val() + "-" + $tr.find(".js-courses option:selected").text();
//    var subjectCombinations = getLocalStorage(courseCode);
//    if (subjectCombinations != null && subjectCombinations != 'undefined' && subjectCombinations.length > 0) {
//        setSubjectCombDDL($tr, JSON.parse(subjectCombinations));
//    } else {
//        var _url = getBaseUrlCollege() + "AssignCombination/SubjectCombinationDDL";
//        $.ajax({
//            url: _url,
//            type: "POST",
//            datatype: "Json",
//            data: { course_id: course_id },
//            success: function (data) {
//                setLocalStorage(courseCode, JSON.stringify(data));
//                setSubjectCombDDL($tr, data);
//            },
//            error: function (xhr, error, msg) {
//                setLocalStorage(courseCode, JSON.stringify(data));
//                setSubjectCombDDL($tr, null);
//                showErrorMessage(msg);
//            }
//        });
//    }
//}


function setSubjectCombDDL($tr, data) {
    var $targetSubjectCombDDL = $('<select class = "form-control chosen-select js-subject-comb"></select>');
    $targetSubjectCombDDL.append('<option Value="">--   Select   --</option>');
    if (data != null) {
        $.each(data, function (index, item) {
            $targetSubjectCombDDL.append('<option value="' + item.Value + '">' + item.Text.trim() + '</option>');
        });
    }
    $tr.find('.js-subject-combination-holder').empty().html($targetSubjectCombDDL);
    ChosenStyle();
}

//function setCombinationStatus($tr, setStatus) {
//    $tr.find('.js-subject-combination-holder .chosen-container').removeClass('error');
//    if ($tr.find('.js-subject-comb').val() == "" || $tr.find('.js-subject-comb').val() == emptyGuid()) {
//        setStatus = false;
//        //$tr.find('.js-subject-combination-holder .chosen-container').addClass('error');
//    } else {
//        //$tr.find('.js-subject-combination-holder .chosen-container').removeClass('error');
//    }
//    if (setStatus) {
//        $tr.find('input[type="checkbox"]').attr('checked', 'checked').prop('checked', 'checked');
//        $tr.find('td.js-comb-status').html('<i class="ace-icon fa fa-check-square-o green fa-2x"></i>');
//    } else {
//        $tr.find('input[type="checkbox"]').attr('checked', false).prop('checked', false);
//        $tr.find('td.js-comb-status').html('<i class="ace-icon fa fa-times red fa-2x"></i>');
//    }
//}

function postCombinationDetails($combSection) {
    var SelectedCombinations = [];
    var _printProgramme = $("#printProgramme option:selected").val();
    var hasError = false;
    $combSection.find("tr.jsDirty").each(function (index, tr) {
        var $tr = $(tr);
        var _SelectedCombination = {};
        //if ($tr.find('input:checked').length > 0) {
        _SelectedCombination.College_ID = $tr.find('#CollegeID').val();
        _SelectedCombination.Student_ID = $('#Student_ID').val();
        _SelectedCombination.SelectedCombination_ID = $tr.find("input[id*=SelectedCombination_ID]").val();
        _SelectedCombination.Course_ID = $tr.find(".js-courses option:selected").val();
        _SelectedCombination.CourseID = $tr.find(".js-courses option:selected").text();
        _SelectedCombination.Combination_ID = $tr.find(".js-subject-comb option:selected").val();
        _SelectedCombination.CombinationID = $tr.find(".js-subject-comb option:selected").text();
        _SelectedCombination.CreatedBy = $tr.find("input[id*=CreatedBy]").val();
        _SelectedCombination.CreatedOn = $tr.find("input[id*=CreatedOn]").val();
        _SelectedCombination.UpdatedBy = $tr.find("input[id*=UpdatedBy]").val();
        _SelectedCombination.UpdatedOn = $tr.find("input[id*=UpdatedOn]").val();
        _SelectedCombination.Semester = $tr.find("input[id*=Semester]").val();
        _SelectedCombination.PreferenceOrder = $tr.find("input[id*=Preference]").val();;

        if (_SelectedCombination.Course_ID = "") {
            $tr.find('.js-courses').siblings('.chosen-container').addClass('error');
            hasError = true;
        }
        if (_SelectedCombination.Combination_ID == "") {
            $tr.find('.js-subject-combination-holder .chosen-container').addClass('error');
            hasError = true;
        }
        SelectedCombinations.push(_SelectedCombination);
        //}
    });
    if (hasError) {
        return;
    }
    if (SelectedCombinations.length == 0) {
        showErrorMessage("No change");
        return false;
    }

    if ($(".js-register-student").hasClass('disabled')) { return; }
    $(".js-update-formDetails,.js-register-student").addClass('disabled'); //disabled both buttons

    var _url = getBaseUrlCollege() + "AssignCombination/UpdateCombination?printProgramme=" + _printProgramme;
    $.ajax({
        url: _url,
        type: "POST",
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(SelectedCombinations),
        success: function (responseData) {
            if (responseData.IsSuccess) {
                showSuccessMessage(responseData.NumberOfRecordsEffected + " Record saved successfully.");
            } else {
                showErrorMessage(responseData.ErrorMessage);
            }
        },
        beforeSend: function () { showLoader(); },
        complete: function () {
            hideLoader();
            $(".js-update-formDetails,.js-register-student").removeClass('disabled'); //enable both buttons
            hideConfirmationDialog();
        },
        error: function (xhr, error, msg) {

            showErrorMessage(msg);
        }
    });
}




function postSubjects($form, listOfSubjectAssigned, listOfAddionalSubjectAssigned) {
    var SelectedCombinations = [];
    var _printProgramme = $("#printProg option:selected").val();
    var semester = $("#sem option:selected").val();
    var student_ID = $("#Student_ID").val();
    var ClassRollNumber = $("#ClassRollNo").val();
    var OverrideExisting = $("#OverrideExistingClassRollNo").is(':checked') ? "True" : "False";

    if ($("#jsSaveUpdateCombinationWithRollNumber").hasClass('disabled')) { return; }
    $("#jsSaveUpdateCombinationWithRollNumber").addClass('disabled');

    var _url = getBaseUrlCollege() + 'AssignCombination/PostSubjectCombinationRollNumber';// `AssignCombination/PostSubjectCombinationRollNumber?PrintProgramme=${_printProgramme}&Semester=${semester}&Student_ID=${student_ID}&ClassRollNo=${ClassRollNumber}&OverrideClassRollNumber=${OverrideExisting}&Form_RegistrationNumber=${$("#Form_RegistrationNumber").val()}&Batch=${$("#Batch").val()}`;
    $.ajax({
        url: _url,
        type: "POST",
        contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
        //data: JSON.stringify(listOfSubjectAssigned),
        data: jQuery.param({ Student_ID: student_ID, Semester: semester, PrintProgramme: _printProgramme, subjects: listOfSubjectAssigned, additionalSubject: listOfAddionalSubjectAssigned, ClassRollNo: ClassRollNumber, OverrideClassRollNumber: OverrideExisting, Form_RegistrationNumber: $("#Form_RegistrationNumber").val(), batch: $("#Batch").val(), fromAcceptExamFormScreen: $form.find("#fromAcceptExamFormScreen").val() }),
        success: function (responseData) {
            if (responseData.IsSuccess) {
                showSuccessMessage(responseData.SuccessMessage);
                $(".js-search-formnumber").focus();
                showErrorMessage(responseData.ErrorMessage);
            } else {
                showErrorMessage(responseData.ErrorMessage);
            }
        },
        beforeSend: function () { showLoader(); },
        complete: function () {
            hideLoader();
            $("#jsSaveUpdateCombinationWithRollNumber").removeClass('disabled');
            $("#Form_RegistrationNumber").focus();
            try { $('#Form_RegistrationNumber')[0].setSelectionRange(11, 18); } catch (e) {
                hideLoader();
            }
            //hideConfirmationDialog();
        },
        error: function (xhr, error, msg) {
            showErrorMessage(msg);
        }
    });
}

//function setSubjectCombinationCount($tr, count) {

//}


function getStudentRegisteredCount(SubjectCombination_ID) {
    if (SubjectCombination_ID == "" || SubjectCombination_ID == emptyGuid()) {
        $('#confirmationDialog').find(".js-registered-std-count").html('00');
    } else {
        var _url = getBaseUrlCollege() + "AssignCombination/GetStudentRegisteredCount";
        $.ajax({
            url: _url,
            type: "POST",
            datatype: "Json",
            async: false,
            data: { SubjectCombination_ID: SubjectCombination_ID },
            success: function (data) {
                $('#confirmationDialog').find(".js-registered-std-count").html(data)
            },
            error: function (xhr, error, msg) {
                //showErrorMessage(msg);
            }
        });
    }
}

function getSubjectCombinationsCount() {
    var $combSection = $(".js-comb-section tbody");
    $combSection.find('tr').each(function (index, tr) {
        var $tr = $(tr);
        if ($tr.find('input:checked').length > 0) {
            getSubjectCombinationCount($tr);
        }
    });
}

jQuery(function ($) {


    $(document).on('change', '.SearchDDL', function (e) {
        var CourseId = $("#Course").val();
        var Type = $(this).attr('data-ddltype');
        var ProgrammeId = $("#Programme").val();
        var Semester = $("#Semester").val();
        var childType = $(this).attr('data-ddlchildtype');
        var childSubType = $(this).attr('data-ddlSubchildtype');
        if (Semester == "" || Semester == undefined)
            Semester = 0

        if (Type != "" && Type != undefined && ProgrammeId != "" && ProgrammeId != undefined) {
            if (Type == "Combination") { setChildDDL(ProgrammeId, CourseId, "Subject", childType, childSubType, Semester); }
            setChildDDL(ProgrammeId, CourseId, Type, childType, childSubType, Semester);
        }
    });
    function setChildDDL(ProgrammeId, CourseId, Type, childType, childSubType, Semester) {
        $('#' + Type + "Div").html("<span style='color:#62a8d1'>...Please Wait..."); // Modify me
        var path = getBaseUrlCollege() + "AssignCombination/_GetChildDDL";
        $.ajax({
            url: path,
            data: { ProgrammeId: ProgrammeId, CourseId: CourseId, Type: Type, Semester: Semester, childType: childType, childSubType: childSubType },
            datatype: "json",
            contentType: "application/html; charset=utf-8",
            encode: true,
            type: "Get",
            cache: false,
            async: true,
            success: function (response) {
                var PartialView = response;
                var $container = $('#' + Type + "Div");
                $('#' + Type + "Div").html(PartialView); // Modify me
                if (Type == 'Combination') {
                    $("#CombinationContainer").html(PartialView);
                }
                ChosenStyle();
            }
        });
    }

});

function BatchUpdateSemesterCombination(BatchPersonalInfoUpdate, printProgramme) {
    var _url = getBaseUrlCollege() + "AssignCombination/BatchUpdateSemesterCombination?printProgramme=" + getPrintProgrammeFromProgramme(printProgramme);
    var model = JSON.parse(JSON.stringify(BatchPersonalInfoUpdate));
    $.ajax({
        url: _url,
        type: "POST",
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(BatchPersonalInfoUpdate),
        dataType: "json",
        success: function (data) {
            if (data.IsSuccess)
                showSuccessMessage(data.NumberOfRecordsEffected + " " + data.SuccessMessage);
            showErrorMessage(data.ErrorMessage, 10000);
            $(".js-search").click();
        },
        beforeSend: function () { showLoader(); },
        complete: function () {
            hideLoader();
        },
        error: function (xhr, error, msg) {
        }
    });
}

function BatchUpdateNextSemesterCombination(BatchPersonalInfoUpdate, printProgramme) {
    var _url = getBaseUrlCollege() + "AssignCombination/BatchUpdateNextSemesterCombination?printProgramme=" + getPrintProgrammeFromProgramme(printProgramme);
    var model = JSON.parse(JSON.stringify(BatchPersonalInfoUpdate));
    $.ajax({
        url: _url,
        type: "POST",
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(BatchPersonalInfoUpdate),
        dataType: "json",
        success: function (data) {
            if (data.IsSuccess)
                showSuccessMessage(data.NumberOfRecordsEffected + " " + data.SuccessMessage);
            showErrorMessage(data.ErrorMessage, 10000);
            $(".js-search").click();
        },
        beforeSend: function () { showLoader(); },
        complete: function () {
            hideLoader();
        },
        error: function (xhr, error, msg) {
        }
    });
}

function validateBatchFilters() {
    var entitylist = getEntityList();
    if (entitylist.length == 0)
        return 'Please choose atleast one record...';

    var _course_id = $(".js-pager-table .jsCourseDDL").find('option:selected').val();
    if (isNullOrEmpty(_course_id))
        return ("Please choose course");

    var _semester = $(".js-pager-table .jsSemesterDDL").find('option:selected').val();
    if (isNullOrEmpty(_semester))
        return ("Please choose semester");

    //var _OldCombination_ID = $(".js-pager-table .jsCombinationDDL").find('option:selected').val();
    //if (isNullOrEmpty(_OldCombination_ID) || _OldCombination_ID == emptyGuid())
    //    return ("Please choose students of a particular combination from combinatin dropdown list");

    $('[data-scope="batch-next-sem-row1"][data-column=Course_ID]').val(_course_id);
    $('[data-scope="batch-next-sem-row1"][data-column=Programme]').val($(".js-pager-table .jsProgrammeDDL").find('option:selected').val());
    $('[data-scope="batch-next-sem-row1"][data-column=Semester]').val(_semester);

    return null;
}