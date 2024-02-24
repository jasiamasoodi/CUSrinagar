window.oncontextmenu = function () {
    return false;
}

$(document).ready(function ($) {
    $("body").css("background-color", "lightgray");
    $("#CaptchaInputText").attr("maxlength", "10");
    $("input[type='text']").attr("autocomplete", "off");

    //status or job form
    $(document).on('submit', '#StdCrtRegistration', function (e) {
        if ($(this).valid()) {
            var ans = confirm("Please review your details again before final submission.\nNo change is allowed after final submission of the Form.");
            if (ans) {
                $("#jsfinalSubmit").val("Submitting...").prop("disabled", true);
                $("#Jspleasewait").fadeIn();
                return true;
            } else {
                $("#jsfinalSubmit").prop("disabled", false).val("Final Submit & Make Payment");
                $("#Jspleasewait").hide();
                return false;
            }
        } else {
            $("#jsfinalSubmit").prop("disabled", false).val("Final Submit & Make Payment");
            $("#Jspleasewait").hide();
            return false;
        }
    });

    $("body").on("change", ".jsvalminm", function () {
        var maxMarks = $(this).closest("tr").find(".jsvalmm").val();
        var minMarks = $(this).val();
        if (Number(maxMarks) < Number(minMarks)) {
            $(this).val('');
            alert("Min. Marks Obtained should be less or equal to Max. Marks");
        }
    });

    $("body").on("change", ".jsvalmm", function () {
        var minMarks = $(this).closest("tr").find(".jsvalminm").val();
        var maxMarks = $(this).val();
        if (Number(maxMarks) < Number(minMarks)) {
            $(this).closest("tr").find(".jsvalminm").val('');
            alert("Max. Marks should be greater or equal to Min. Marks");
        }
    });


    $("body").on("change", "input[type='text']", function () {
        $(this).val($(this).val().toUpperCase());
    });

    $("#JSAddPQItem").click(function () {
        var addItemIndex = Number($("#AddItemIndex").val());
        if ($(".JsTableBody tr").length >= 11) {
            alert("Only 10 items are allowed.");
            return;
        }
        var tableTrToAppend = `'<tr id="item_${addItemIndex}">
                                    <th style="padding-left:20px">
                                        <span class="required" aria-required="true">
                                            <span class="field-validation-valid" data-valmsg-for="PrevQualifications[${addItemIndex}].ExamName" data-valmsg-replace="true"></span>
                                        </span>
                                        <input class="form-control width100 input-validation-error" data-val="true" data-val-maxlength="Max 150 chars" data-val-maxlength-max="150" data-val-minlength="Max 3 chars" data-val-minlength-min="3" data-val-required=" Required" id="PrevQualifications_${addItemIndex}__ExamName" maxlength="140" name="PrevQualifications[${addItemIndex}].ExamName" onkeypress="return forAddressOnly(event);" title="Exam Name" type="text" value="" autocomplete="off" aria-required="true" aria-describedby="PrevQualifications_${addItemIndex}__ExamName-error" aria-invalid="true"></th>
                                    <td>
                                        <span class="required" aria-required="true">
                                            <span class="field-validation-valid" data-valmsg-for="PrevQualifications[${addItemIndex}].Stream" data-valmsg-replace="true"></span>
                                        </span>
                                        <input class="form-control width100" data-val="true" data-val-maxlength="Max 150 chars" data-val-maxlength-max="150" data-val-minlength="Max 2 chars" data-val-minlength-min="2" data-val-required=" Required" id="PrevQualifications_${addItemIndex}__Stream" maxlength="150" name="PrevQualifications[${addItemIndex}].Stream" onkeypress="return forAddressOnly(event);" placeholder="e.g ARTS" title="e.g GENERAL, ARTS, MEDICAL" type="text" value="" autocomplete="off">
                                    </td>
                                    <td>
                                        <span class="required" aria-required="true">
                                            <span class="field-validation-valid" data-valmsg-for="PrevQualifications[${addItemIndex}].YearOfPassing" data-valmsg-replace="true"></span>
                                        </span>
                                        <input class="form-control width100 valid" data-val="true" data-val-number="The field YearOfPassing must be a number." data-val-range=" Invalid" data-val-range-max="9999" data-val-range-min="1950" data-val-regex="Invalid" data-val-required=" Required" id="PrevQualifications_${addItemIndex}__YearOfPassing" maxlength="4" name="PrevQualifications[${addItemIndex}].YearOfPassing" onkeypress="return numbersOnly(event);" title="Year Of Passing" type="text" value="" autocomplete="off" aria-required="true" aria-describedby="PrevQualifications_${addItemIndex}__YearOfPassing-error" aria-invalid="false">
                                    </td>
                                    <td>
                                        <span class="required" aria-required="true">
                                            <span class="field-validation-valid" data-valmsg-for="PrevQualifications[${addItemIndex}].MaxMarks" data-valmsg-replace="true"></span>
                                        </span>
                                        <input class="form-control width100 jsvalmm" data-val="true" data-val-number="The field Max Marks must be a number." data-val-regex="Invalid" data-val-required=" Required" id="PrevQualifications_${addItemIndex}__MaxMarks" maxlength="4" name="PrevQualifications[${addItemIndex}].MaxMarks" onkeypress="return numbersOnly(event);" title="Maximum Marks" type="text" value="0" autocomplete="off">
                                    </td>

                                    <td>
                                        <span class="required" aria-required="true">
                                            <span class="field-validation-valid" data-valmsg-for="PrevQualifications[${addItemIndex}].MarksObt" data-valmsg-replace="true"></span>
                                        </span>
                                        <input class="form-control width100 jsvalminm" data-val="true" data-val-number="The field Marks Obt must be a number." data-val-range=" Invalid" data-val-range-max="9999" data-val-range-min="1" data-val-required=" Required" id="PrevQualifications_${addItemIndex}__MarksObt" maxlength="7" name="PrevQualifications[${addItemIndex}].MarksObt" onkeypress="return floatNumbersOnly(event);" title="Marks Obtained" type="text" value="0" autocomplete="off">
                                    </td>
                                    <td>
                                        <span class="required" aria-required="true">
                                            <span class="field-validation-valid" data-valmsg-for="PrevQualifications[${addItemIndex}].ExamBody" data-valmsg-replace="true"></span>
                                        </span>
                                        <select class="form-control width100" data-val="true" data-val-required=" Required" id="PrevQualifications_${addItemIndex}__ExamBody" name="PrevQualifications[${addItemIndex}].ExamBody"><option value=""></option>
                                        <option value="JKBOSE">JK BOSE</option>
                                        <option value="CBSE">CBSE</option>
                                        <option value="University Of Kashmir">University Of Kashmir</option>
                                        <option value="University Of Jammu">University Of Jammu</option>
                                        <option value="Central University Of Kashmir">Central University Of Kashmir</option>
                                        <option value="Central University Of Jammu">Central University Of Jammu</option>
                                        <option value="Cluster University Srinagar">Cluster University Srinagar</option>
                                        <option value="Central University Jammu">Cluster University Jammu</option>
                                        <option value="SKUAST">SKUAST</option>
                                        <option value="IGNOU">IGNOU</option>
                                        <option value="IUST Awantipora">IUST Awantipora</option>
                                        <option value="OTHER">Other</option>
                                        </select>
                                    </td> 
                                    <th class="text-center">
                                            <a href="javascript:void(0);" class="JSRemovePQItem" data-ids="item_${addItemIndex}"><i class="fa fa-trash"></i> Remove</a>
                                    </th>
                                </tr>'`;
        $(".JsTableBody").append(tableTrToAppend);
        $("#AddItemIndex").val(addItemIndex + 1);

        //Remove current form validation information
        $("#StdCrtRegistration").removeData("validator").removeData("unobtrusiveValidation");
        //Parse the form again
        $.validator.unobtrusive.parse("#StdCrtRegistration");
    });

    $("body").on("click", ".JSRemovePQItem", function () {
        var idToRemove = $(this).attr("data-ids");
        $("#" + idToRemove).remove();
    });

});