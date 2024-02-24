$("#addQuestion").click(function () {
    var subject_id = $("#Subject option:selected").val();
    var exam_id = Exam_Id;
    if (subject_id.trim() == "" || exam_id.trim() == "") {
        $("#Subjectlbl").html("Subject Required");
        return;
    }
    $("#Subjectlbl").html("");
    window.open(
        "/CuSrinagarAdminPanel/InternalAdmin/AddQuestions?Exam_Id=" + exam_id + "&Subject_Id=" + subject_id,
        '_blank'
    );

})

$(".add").click(function () {
    var count = $(this).data("count");
    count++;
    var div = `<div class="form-group" id="formgroup_` + count + `">
                                        <label class="col-sm-3 control-label no-padding-right">
                                            <span class="red bigger-110">*</span> <label for="Option`+ count + `">Option ` + (count + 1) + `</label>

                                        </label>
                                        <div class="col-sm-6 ">
                                            <input class="width -100" id="option_`+ count + `__OptionName" maxlength="150" name="option[` + count + `].OptionName" type="text" value>
                                            <input id="option_`+ count + `__IsCorrect" name="option[` + count + `].IsCorrect" type="radio" value="False"> Is Answer
                                        </div>
                                        <div class="col-sm-2">
                                            <span class="field-validation-valid label label-sm label-danger arrowed-in-right arrowed" data-valmsg-for="option[`+ count + `].OptionName" data-valmsg-replace="true"></span>

                                        </div>
                                        <div class="col-sm-1">
                                              <button  type="button" class="remove alert-danger " data-count="` + count + `" id="Del_` + count + `">-</button>

                                         </div>
                                    </div>`;
    $(div).insertBefore(".Count");
    $(".Count").data("count", count);
    $("#Add").data("count", count);

})
$(document).on('click', '.remove', function () {
    var count = $(this).data("count");
    $("#option_" + count + "__OptionName").val("");
    $("#formgroup_" + count).addClass("hidden");
});
$(document).on('change', 'input[type=radio]', function () {
    // When any radio button on the page is selected,
    // then deselect all other radio buttons.
    $(this).val(true);
    $('input[type=radio]:checked').not(this).prop('checked', false);
    $('input[type=radio]:checked').not(this).val(false);
})
