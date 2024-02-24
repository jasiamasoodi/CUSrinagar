$(document).ready(function () {

    if ($("#PostSubjectCombinationRollNumber").length > 0) {
        bindCombinationSectionEvents();
    } else if ($("#ExaminationForm").length > 0) {
        bindExaminationFormSectionEvents();
    } else if ($("#BacklogExaminationForm").length > 0) {
        BacklogExaminationFormSectionEvents();
    }

});

function BacklogExaminationFormSectionEvents() {
    //BacklogExaminationForm
    $(".jsMainForm").validate({
        rules: {
            'ReAppearSubjectFullName': {
                required: true,
                minlength: 1
            }
        },
        messages: {
            ReAppearSubjectFullName: "Please select at least one subject"
        }
    });

    $(".js-reExamSubject").change(function () {
        setTotalFee();
    });

    $(".jsMainForm").submit(function () {
        var $form = $(this);

        if ($(".js-reExamSubject:checked:not([disabled])").length === 0) {
            showErrorMessage("Please choose atleast one subject."); return false;
        }
        if ($form.valid()) {
            if (confirm('You are applying for division improvement Examination Form.\nAre you sure, you want to proceed?')) {
                $("#MobileNumber").val($("#MobileNumber").cleanVal());
                showWaitingDialog('Please wait ... while we redirect you to the payment gateway.');
                return true;
            }
        }
        return false;
    });
}

function bindCombinationSectionEvents() {

    if ($(".jsOptionCoreCourseCheckbox").length > 0) {
        $(".jsOptionCoreCourseCheckbox").each(function () {
            var $element = $(this);
            var value = $element.closest('.jsOptionalCoreCourse').find('select').find('option:selected').val();
            if (isNullOrEmpty(value)) {
                $element.closest('.jsOptionalCoreCourse').find('select').find('option').removeAttr('selected');
                $element.closest('.jsOptionalCoreCourse').find('select').find('option [value=""]').attr('selected', 'selected');
                $element.closest('.jsOptionalCoreCourse').find('select').val('');
                $element.closest('.jsOptionalCoreCourse').find('select').addClass('hidden');
                $element.closest('.jsOptionalCoreCourse').find('.chosen-container').addClass('hidden');
            } else {
                $element.attr('checked', 'checked').prop('checked', 'checked');
                $element.closest('.jsOptionalCoreCourse').find('select').removeClass('hidden');
                $element.closest('.jsOptionalCoreCourse').find('.chosen-container').removeClass('hidden');
            }
        });
    }


    $(".jsOptionCoreCourseCheckbox").change(function () {
        var $element = $(this);
        if ($element.is(':checked')) {
            $element.closest('.jsOptionalCoreCourse').find('select').removeClass('hidden');
            $element.closest('.jsOptionalCoreCourse').find('.chosen-container').removeClass('hidden');
        } else {
            $element.closest('.jsOptionalCoreCourse').find('select').find('option').removeAttr('selected');
            $element.closest('.jsOptionalCoreCourse').find('select').find('option [value=""]').attr('selected', 'selected');
            $element.closest('.jsOptionalCoreCourse').find('select').val('');
            $element.closest('.jsOptionalCoreCourse').find('select').addClass('hidden');
            $element.closest('.jsOptionalCoreCourse').find('.chosen-container').addClass('hidden');
        }
        return false;
    });

    $(".jsCoreSubject").change(function () {
        var $checkbox = $(this);
        var $section = $(this).closest('.jsCoreSubjectSection');
        if ($checkbox.is(':checked')) {
            $section.find('input.jsCoreSubject_ID').val($checkbox.val());
        } else {
            $section.find('input.jsCoreSubject_ID').val('');
        }
    });

    $("#PostSubjectCombinationRollNumber").submit(function (event) {
        $(".jsSubjectSection").each(function (index, section) {
            var $section = $(section);
            var $checkbox = $section.find('input[type=checkbox]');
            if ($checkbox.is(':checked')) {
                $section.find('input.jsSubject_ID').val($checkbox.val());
            } else {
                $section.find('input.jsSubject_ID').val('');
            }
        });
        return true;

        var $form = $("#CombinationForm");
        var combinationSetting = JSON.parse($("#CombinationSetting").val());
        var listOfSubjectAssigned = [];
        var milSubject = "";

        $(".jsCoreSubjectSection").each(function (index, section) {
            var $section = $(section);
            if ($section.find('input[type=checkbox]:checked').length > 0) {
                var subject = { Subject_ID: $section.find('.jsCoreSubject_ID').val(), subjectType: 'Core', SubjectFullName: $section.find('.jsCoreSubjectID').val() }; //1
                listOfSubjectAssigned.push(subject);
                var $subject = `<input type='hidden' name='Subject' value='${subject.Subject_ID}' />`;
                $form.append($subject);
                if (subject.SubjectFullName.toLowerCase().indexOf('mil') >= 0) {
                    milSubject = subject.SubjectFullName;
                }
            }
        });

        //3rd semester section for mil subjects
        if (milSubject.length > 0 && combinationSetting.Semester == 3) {
            showErrorMessage("MIL Subjects(" + milSubject + ") cannot be opted in 3rd sem");
            return false;
        }
        //end of 3rd sem section

        //Section: Total core subjects from previous semester
        if (combinationSetting.CoreCourseCount > 0) {
            if (listOfSubjectAssigned.length != combinationSetting.CoreCourseCount) {
                showErrorMessage("Number of core subjects should be " + combinationSetting.CoreCourseCount);
                return false;
            }
        }
        //End core section

        //Compulsary subjects in a course
        $(combinationSetting.CompulsarySubject).each(function (index, subject) {
            listOfSubjectAssigned.push(subject);
        });
        //end compulsary section


        //generic/open elective subjects
        var totalGEopted = 0;
        for (var i = 0; i < combinationSetting.GenericElectiveCourseCount; i++) {
            var $SubjectOption = $($form.find(".jsGenericElectiveSubject select")[i]).find('option:selected');
            if ($SubjectOption.val().length > 0) {
                listOfSubjectAssigned.push({ Subject_ID: $SubjectOption.val(), SubjectFullName: $SubjectOption.text(), subjectType: $SubjectOption.data('subjecttype') || "GenericElective" }); //3
                totalGEopted++;
            }
        }
        if (combinationSetting.GenericElectiveCourseCount > 0 && totalGEopted <= 0) {
            showErrorMessage("Number of general elective subjects should be " + combinationSetting.GenericElectiveCourseCount);
            return false;
        }
        //end section generic elective


        //skill Enhance subjects
        var totalSkillopted = 0;
        for (var i = 0; i < combinationSetting.SkillEnhancementCourseCount; i++) {
            var $SubjectOption = $($form.find(".jsSkillEnhanceSubject select")[i]).find('option:selected');
            if ($SubjectOption.val().length > 0) {
                listOfSubjectAssigned.push({ Subject_ID: $SubjectOption.val(), SubjectFullName: $SubjectOption.text(), subjectType: 'SkillEnhancement' }); //3
                totalSkillopted++;
            }
        }
        if (combinationSetting.SkillEnhancementCourseCount > 0 && totalSkillopted <= 0) {
            showErrorMessage("Number of Skill Enhance Subjects should be " + combinationSetting.SkillEnhancementCourseCount);
            return false;
        }
        //end section skill enhance

        //Descipline Specific Elective subjects
        var totalDSEopted = 0;
        for (var i = 0; i < combinationSetting.DesciplineSpecificElectiveCourseCount; i++) {
            var $SubjectOption = $($form.find(".jsDesciplineSpecificElective select")[i]).find('option:selected');
            if ($SubjectOption.val().length > 0) {
                listOfSubjectAssigned.push({ Subject_ID: $SubjectOption.val(), SubjectFullName: $SubjectOption.text(), subjectType: 'DSE' });
                totalDSEopted++;
            }
        }
        if (combinationSetting.DesciplineSpecificElectiveCourseCount > 0 && totalDSEopted <= 0) {
            showErrorMessage("Number of Discipline Specific Elective subjects should be " + combinationSetting.DesciplineSpecificElectiveCourseCount);
            return false;
        }
        //end section skill enhance


        //Descipline Specific Elective subjects
        var totalDCEopted = 0;
        for (var i = 0; i < combinationSetting.DesciplineCentricElectiveCourseCount; i++) {
            var $SubjectOption = $($form.find(".jsDesciplineCentricElective select")[i]).find('option:selected');
            if ($SubjectOption.val().length > 0) {
                listOfSubjectAssigned.push({ Subject_ID: $SubjectOption.val(), SubjectFullName: $SubjectOption.text(), subjectType: 'DCE' });
                totalDCEopted++;
            }
        }
        if (combinationSetting.DesciplineCentricElectiveCourseCount > 0 && totalDCEopted <= 0) {
            showErrorMessage("Number of Descipline Specific Elective subjects should be " + combinationSetting.DesciplineCentricElectiveCourseCount);
            return false;
        }
        //end section skill enhance

        if (combinationSetting.TotalCreditSet == true) {
            var totalcredit;//calculate subject credits
            if (totalcredit != combinationSetting.TotalCredit) {
                showErrorMessage(`Total credit should be equal to ${combinationSetting.TotalCreditSet} credits.`);
                return false;
            }
        }

        if (!$("#frmStudentSubjectCombination").valid()) {
            return false;
        }
    });

}

function bindExaminationFormSectionEvents() {

    $("[id*=Section]").removeClass("active");
    $("#ExaminationFormSection").addClass("active");


    $("#ExaminationForm").submit(function () {
        var $form = $("#ExaminationForm");
        if ($form.valid()) {
            $("#MobileNumber").val($("#MobileNumber").cleanVal());
            showWaitingDialog('Please wait ... while we redirect you to the payment page.');
            return true;
        }
        return false;
    });
}


function setTotalFee() {
    var ARGExamFormDownloadable = JSON.parse($("#ARGExamFormDownloadable").val());
    var totalFee = parseInt($("#TotalRegularFee").val()) || 0;
    var backlogfee = 0;
    $(".js-reExamSubject:checked").each(function (index, element) {
        var $element = $(element);
        if (index <= 1) {
            backlogfee = (ARGExamFormDownloadable.MinimumFee);
        }
        else {
            backlogfee += (ARGExamFormDownloadable.FeePerSubject);
        }
    });
    if ($(".js-reExamSubject:checked").length > 0) { 
        totalFee = backlogfee + ARGExamFormDownloadable.OtherCharges + ARGExamFormDownloadable.ExaminationFund + ARGExamFormDownloadable.ITComponent;
    }
    else {
        totalFee += backlogfee;
    }
    if (totalFee > 0) totalFee = totalFee;

    $("#jsTotalFee").html(totalFee.toFixed(0));
    totalFee = totalFee + (parseInt($("#LateFeeAmount").val()) || 0) - (parseInt($("#AmountAlreadyPaid").val()) || 0);
    $("#jsAmountToBePaid").html(totalFee.toFixed(0));
}