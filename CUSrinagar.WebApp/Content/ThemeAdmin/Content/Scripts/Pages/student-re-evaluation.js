$(document).ready(function () {
    $(".jsMainForm").validate({
        rules: {
            subjectrow: {
                required: true,
                minlength: 1
            }
        },
        messages: {
            subjectrow: "Please select atleast one subject"
        }
    });

    //$(".jsSubmitReEvaluationForm").on('click keypress', function (e) {
    //    if (e.type == "click" && $(e.target).is('button') || e.which == "13") {
    //        var $form = $(this).closest('form');
    //        if (!$form.hasClass('under-process')) {
    //            var $form = $(this).closest('form');
    //            postRevationForm($form);
    //        }
    //    }
    //});

    $('.jsMainForm').submit(function () {
        var $form = $(this);
        if (!$form.valid()) return false;
        if ($form.find('table tbody tr input[type=checkbox]:checked').length === 0) {
            showErrorMessage('Please choose at least one subject for Re-Evaluation.'); return false;
        }
    });
    $('.js-chk-fee').on('click', function () {

        var $container = $(this).closest('table');
        var totalFee = getTotalFee($container);
        $(".jsTotalFee").html(totalFee);
    });

});

function getTotalFee($container) {
    var totalFee = 0;
    var feePerSubject = 0;
    if ($("#FeeAmountXerox").length > 0) {
        feePerSubject = parseInt($("#FeeAmountXerox").val()) || 100;
        $container.find('tr input[type=checkbox]:checked').each(function (index, element) {
            totalFee += feePerSubject;

        });
        return totalFee;
    } else if ($("#FeeAmountReEvaluation").length > 0) {
        feePerSubject = parseInt($("#FeeAmountReEvaluation").val());
        $container.find('tr input[type=checkbox]:checked').each(function (index, element) {
            totalFee += feePerSubject;
        });
        return totalFee;
    }
    return 0;
}

//function postRevationForm($form) {
//    if ($form.find('table tbody tr input[type=checkbox]:checked').length == 0) {
//        showErrorMessage('Please choose at least one subject for Re-Evaluation.'); return;
//    }
//    $('#RevConfirmationDialog').modal('show');
//    $('#RevConfirmationDialog').off('click.confirmation', 'button').on('click.confirmation', 'button', function () {
//        var $btn = $(this);
//        if ($btn.data('response') == 'yes') {
//            if (!$form.valid()) { return; };
//            var revaltionFormObject = getReEvaluationFormObject($form);
//            revaltionFormObject.Email = $("#Email").val();
//            revaltionFormObject.MobileNumber = $("#MobileNumber").cleanVal();
//            revaltionFormObject.Semester = $("#ApplyForSemester").val();
//            var $customform = createFormFromModel(revaltionFormObject, $form.attr('action'));
//            var $token = $form.find('[name*=RequestVerificationToken]');
//            $customform.append($token);
//            $('body').append($customform);
//            $('#RevConfirmationDialog').modal('hide');
//            showWaitingDialog('Please wait ... while we redirect you to the payment page.');
//            $form.addClass('under-process');
//            $customform.submit();
//        } else if ($btn.data('response') == 'no') {
//            $('#RevConfirmationDialog').modal('hide');
//        } else {
//            $('#RevConfirmationDialog').modal('hide');
//        }
//    });
//}

//function getReEvaluationFormObject($form) {
//    var revaltionFormObject = {};
//    revaltionFormObject._ID = emptyGuid();
//    revaltionFormObject.Student_ID = $form.find("input[id*=Student_ID]").val();
//    revaltionFormObject.FeeAmount = getTotalFee($form); //.find("input[id*=FeeAmount]").val();
//    revaltionFormObject.FormStatus = $form.find("input[id*=FormStatus]").val();
//    revaltionFormObject.FormNumber = $form.find("input[id*=FormNumber]").val();
//    revaltionFormObject.FormType = $form.find("input[id*=FormType]").val();
//    revaltionFormObject.SubjectsForEvaluation = [];
//    $form.find('table tbody tr').each(function (index, tr) {
//        var $tr = $(tr);
//        if ($tr.find('input[type=checkbox]:checked').length > 0) {
//            var revalution = {};
//            revalution.ReEvaluationSubject_ID = $tr.find(".jsReEvaluationSubject_ID").val();
//            revalution.Student_ID = $tr.find(".jsStudent_ID").val();
//            revalution.Subject_ID = $tr.find(".jsSubject_ID").val();
//            revalution.OptForReEvaluation = $tr.find(".js-chk-fee:checked[value=ReEvaluation]").length>0; //&& $tr.find(".js-chk-fee:checked").val() == "ReEvaluation";
//            revalution.OptForXerox = $tr.find(".js-chk-fee:checked[value=Xerox]").length > 0;//&& $tr.find(".js-chk-fee:checked").val() == "Xerox";
//            revaltionFormObject.SubjectsForEvaluation.push(revalution);
//        }
//    });
//    return revaltionFormObject;
//}