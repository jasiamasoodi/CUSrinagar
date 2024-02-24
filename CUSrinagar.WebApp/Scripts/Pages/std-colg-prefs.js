$(document).ready(function () {

    $("[name=Course_ID]").change(function (e) {
        var $element = $(this);
        var course_id = $element.val() || $element.find('option:selected').val();

        fillCollegeDDL(course_id);
    });



    $('form').keypress(function (e) {
        if (e === 'Enter' || e.charCode === 13 || e.which === 13) {
            $('#jsSavePref').click();
        }
    });
    $('#jsSavePref').click(function () {
        var $form = $('#CollegePreference');
        if (!$form.valid()) {
            return false;
        }
        //var hasFilledOne = false;
        //$(".jsCollegeCode").each(function () {
        //    if (!isNullOrEmpty($(this).val()) && hasFilledOne == false)
        //        hasFilledOne = true;
        //});
        //if (!hasFilledOne) {
        //    showErrorMessage('Please fill any one preference');
        //    return false;
        //}
        var hasFilledAll = true;
        $(".jsCollegeCode").each(function () {
            if (isNullOrEmpty($(this).val()) && hasFilledAll == true)
                hasFilledAll = false;
        });
        if (!hasFilledAll) {
            showErrorMessage('Please fill all preference');
            return false;
        }

        var _alreadySelectOptions = [];
        $(".jsCollegeCode").each(function (index, select) {
            var $select = $(select);
            if (!isNullOrEmpty($select.find('option:selected').val()) && $select.find('option:selected').val().length > 0) {
                _alreadySelectOptions.push($select.find('option:selected').val());
            }
        });

        if (new Set(_alreadySelectOptions).size !== _alreadySelectOptions.length) {
            showErrorMessage('Duplicate preferences choosen. Please dont choose similar preferences');
            return false;
        }

        var msg = '<h4>Are you sure to save college preferences ?</h4>';
        msg += '<h3 class=" text-danger">After that preferences cannot be edit</h3>';
        showConfirmationDialog(msg);
        $('#confirmationDialog').off('click.confirmation', 'button').on('click.confirmation', 'button', function () {
            var $btn = $(this);
            hideConfirmationDialog();
            if ($btn.data('response') == 'yes') {
                $('#CollegePreference').submit();
            }
        });
        return false;

    });

    $(document).on('change', '.jsCollegeCode', function () {
        var selectPosition = parseInt($(this).data('position'));
        var $listOfSelects = $(".jsCollegeCode");
        var _alreadySelectOptions = [];
        $listOfSelects.each(function (index, select) {
            var $select = $(select);
            if (!isNullOrEmpty($select.find('option:selected').val())) {
                if (_alreadySelectOptions.indexOf($select.find('option:selected').val()) >= 0) {
                    $select.find('option').first().prop('selected', 'selected');
                }
                _alreadySelectOptions.push($select.find('option:selected').val());
            }
        });
        $listOfSelects.each(function (mainIndex, mainSelect) {
            var $mainSelect = $(mainSelect);
            _alreadySelectOptions = [];
            $listOfSelects.each(function (index, _select) {
                var $select = $(_select);
                if (!isNullOrEmpty($select.find('option:selected').val()) && index < mainIndex) {
                    _alreadySelectOptions.push($select.find('option:selected').val());
                }
            });
            $mainSelect.find('option').prop('disabled', false);
            if (_alreadySelectOptions.length > 0) {
                $mainSelect.find('option').each(function (_index, option) {
                    $option = $(option);
                    if ($option.val().length > 0) {
                        if (_alreadySelectOptions.indexOf($option.val()) >= 0) {
                            $option.prop('disabled', true);
                        }
                    }
                });
            }
        });
    });

});


function fillCollegeDDL(_Course_ID) {
    if (isNullOrEmpty(_Course_ID)) { fillDDL(null); return; }

    var _url = "/CUSrinagarAdminPanel/General/GetCollegeList";
    $.ajax({
        url: _url,
        type: "POST",
        datatype: "Json",
        data: { Course_ID: _Course_ID, printProgramme: null, programme: null },
        beforeSend: function () { showLoader(); },
        success: function (data) {
            var gender = $("#Gender").val();
            if (data !== null && data.length > 0) {
                data = data.filter(function (val, index) {
                    return gender.toLowerCase() !== "female" && val.Text.toLowerCase().indexOf('women') > 0 ? null : val;
                });

                //remove college
                var removeCols = $("#CRmv").val().toLowerCase().split("|");

                data = data.filter(function (val, index) {
                    if (removeCols.indexOf($.trim(val.Value.toLowerCase())) == -1) {
                        return val;
                    } else {
                        return null;
                    }
                });
            }
            fillDDL(data);
        },
        error: function (xhr, error, msg) {
            fillDDL(ddlLists);
        },
        complete: function () {
            hideLoader();
        }
    });
}

function fillDDL(data) {
    var $targetSelects = $(`.jsCollegeCode`);
    $targetSelects.each(function (index, element) {
        var $targetSelect = $(element);
        clearDDOptions($targetSelect);
        if (index < data.length) {
            $('.lbl_' + index).removeClass('hidden');
            $targetSelect.removeClass('hidden');
            
            fillDDLOptions($targetSelect, data);
            //ChosenStyle();
            //resizechosen($targetSelect);
        } else {
            $('.lbl_' + index).addClass('hidden');
            $targetSelect.addClass('hidden');
        }
    });
}
