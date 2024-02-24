var _baseURL = null;
$(document).ready(function ($) {
    if (!isNullOrEmpty($("#collegebaseurl").val()) && $("#collegebaseurl").val().length > 0) {
        _baseURL = "CUCollegeAdminPanel";
    } else {
        _baseURL = "CUSrinagarAdminPanel";
    }

    $("#JSMakeActive").click(function () {
        var ActiveList = [];
        $("input[name='Combination_ID']:checked").each(function () {
            ActiveList.push($(this).attr("value"));
        });
        if (ActiveList.length != 0) {
            var ans = confirm('Are you sure you want to make selected combinations Active?');
            if (ans == true) {
                if ($(this).text() != "working...") {
                    //$(this).html("working...").prop("disabled", true);
                    //$("#JSMakeInActive").prop("disabled", true);
                    $.ajax({
                        url: "/" + _baseURL + "/Combination/MakeCombinationsActive",
                        type: "POST",
                        data: { Combination_IDs: ActiveList },
                        dataType: "json",
                        beforeSend: function () { showLoader(); },
                        success: function (response) {
                            showSuccessMessage(response);
                            loadpagertableWithDefaultParams();
                            // $("#JSMakeActive").prop("disabled", false).html('<i class="ace-icon fa fa-check"></i> Make Active');
                            // $("#JSMakeInActive").prop("disabled", false);
                            // $('input:checkbox').prop('checked', false);
                            // $('tbody tr').css('background-color', '');
                            // $(ActiveList).each(function (index, value) {
                            //    $("#" + value).html('<span class="label label-sm label-success">Active</span>');
                            //});
                            //$("#response").html("<div class='col-sm-1'></div><div class='alert alert-success col-sm-10'><button type='button' class='close' data-dismiss='alert'><i class='ace-icon fa fa-times'></i></button><strong><i class='ace-icon fa fa-times'></i> Awesome!</strong> "+response+"<br></div><div class='col-sm-1'></div>");
                        },
                        error: function (jqXHR, textStatus, errorThrown) {
                            //$("#JSMakeActive").prop("disabled", false).html('<i class="ace-icon fa fa-check"></i> Make Active');
                            //$("#JSMakeInActive").prop("disabled", false);
                            alert('status code: ' + jqXHR.status + '\n Please check your internet connection or refresh and try again.');
                        },
                        complete: function () {
                            hideLoader();
                        }
                    });
                }
            }
        } else {
            alert('No Combination has been selected.');
        }
    });

    $("#JSMakeInActive").click(function () {
        var ActiveList = [];
        $("input[name='Combination_ID']:checked").each(function () {
            ActiveList.push($(this).attr("value"));
        });
        if (ActiveList.length != 0) {
            var ans = confirm('Are you sure you want to make selected combinations In-Active?');
            if (ans == true) {
                if ($(this).text() != "working...") {
                    $(this).html("working...").prop("disabled", true);
                    $("#JSMakeActive").prop("disabled", true);
                    $.ajax({
                        url: "/" + _baseURL + "/Combination/MakeCombinationsInActive",
                        type: "POST",
                        data: { Combination_IDs: ActiveList },
                        dataType: "json",
                        success: function (response) {
                            showSuccessMessage(response);
                            loadpagertableWithDefaultParams();
                            //$("#JSMakeInActive").prop("disabled", false).html('<i class="ace-icon fa fa-bolt"></i> Make In-Active');
                            //$("#JSMakeActive").prop("disabled", false);
                            //$('input:checkbox').prop('checked', false);
                            //$('tbody tr').css('background-color', '');
                            //$(ActiveList).each(function (index, value) {
                            //    $("#" + value).html('<span class="label label-sm label-warning">In-Active</span>');
                            //});
                            //$("#response").html("<div class='col-sm-1'></div><div class='alert alert-success col-sm-10'><button type='button' class='close' data-dismiss='alert'><i class='ace-icon fa fa-times'></i></button><strong><i class='ace-icon fa fa-times'></i> Awesome!</strong> " + response +"<br></div><div class='col-sm-1'></div>");
                        },
                        error: function (jqXHR, textStatus, errorThrown) {
                            //$("#JSMakeInActive").prop("disabled", false).html('<i class="ace-icon fa fa-bolt"></i> Make In-Active');
                            //$("#JSMakeActive").prop("disabled", false);
                            alert('status code: ' + jqXHR.status + '\n Please check your internet connection or refresh and try again.');
                        }
                    });
                }
            }
        } else {
            alert('No Combination has been selected.');
        }
    });

    $(".jsmodel").click(function () {
        var combination_id = $(this).attr("data-combination_id");
        var modelid = $(this).attr("href");
        $.ajax({
            url: "/" + _baseURL + "/Combination/_UserActivity/" + combination_id,
            type: "POST",
            data: {},
            dataType: "html",
            async: true,
            contentType: "application/json",
            success: function (response) {
                $(modelid).find(".modal-body").empty().html(response);
            },
            error: function (jqXHR, textStatus, errorThrown) {
                alert('status code: ' + jqXHR.status + '\n Please check your internet connection or refresh and try again.');
            }
        });
    });

    $(document).on('click', '.jsDeleteMainItem', function () {
        var $tr = $(this).closest('tr');
        var msg = '<h4>Are you sure you want to delete this combination ?</h4>';
        showConfirmationDialog(msg);
        $('#confirmationDialog').off('click.confirmation', 'button').on('click.confirmation', 'button', function () {
            var $btn = $(this);
            hideConfirmationDialog();
            if ($btn.data('response') == 'yes') {
                var _ID = $tr.find(".jsRow_ID").val();
                DeleteCombination($tr, _ID);
            }
        });
    });

    //show row wize dailog
    $(document).on('click', '.jsShowReplaceSubjectDDL', function () {
        $('tr .jsSelectChk').removeAttr('checked').prop('checked', false);
        var $tr = $(this).closest('tr');
        $tr.find('.jsSelectChk').attr('checked', true).prop('checked', 'checked');
        //$tr.addClass('jsSelectedRow');
        var $batchDialog = $("#jsShowReplaceSubjectDDLDailog");
        var _oldCombination_ID = $tr.find('.jsEntity_ID').val();

        var _param = new Parameter();
        _param.Filters = [];
        _param.Filters.push({ Column: "Combination_ID", Operator: "EqualTo", Value: _oldCombination_ID, GroupOperation: "AND", TableAlias: "" });
        fillFindSubjectDDL(_param);

        $batchDialog.modal('show');
    });

    $(document).on('click', '.jsShowUpdateCombinationByIDsDialog', function () {
        $('tr.jsSelectedRow').removeClass('jsSelectedRow');
        var $tr = $(this).closest('tr');
        $tr.addClass('jsSelectedRow');
        var $batchDialog = $("#jsUpdateCombinationByIDsDialog");
        var subjectids = $tr.find('.jsSubjectGuids').val().split('|');
        $batchDialog.find('tbody').html('');
        for (var i = 0; i < subjectids.length + 3; i++) {
            var $copyRow = `<tr class="">
                                <td class="wd-35">
                                    <input class="ace checkbox checkbox-inline jsBatchChkBox" type="checkbox"><span class="lbl"></span>
                                </td>
                                <td class="wd-200">
                                    Subject - ${i + 1}
                                </td>
                                <td>
                                    <input type="text" id="SubjectID" name="SubjectID" class="form-control jsSubject_ID" value='${(isNullOrEmpty(subjectids[i]) ? '' : String(subjectids[i]))}' />                               
                                </td>
                            </tr>`;
            $batchDialog.find('tbody').append($copyRow);
        }
        $batchDialog.modal('show');
    });

    $("#jsUpdateCombinationByIDs").click(function () {
        var $tr = $('tr.jsSelectedRow');
        var $batchDialog = $("#jsUpdateCombinationByIDsDialog");
        var _ADMCombinationMaster = { Combination_ID: $tr.find('.jsRow_ID').val(), CombinationSubjects: '' };
        var subjectids = [];
        $batchDialog.find('.jsSubject_ID').each(function (index, element) {
            if (!isNullOrEmpty($(element).val())) {
                subjectids.push($(element).val().trim());
            }
        });
        subjectids = new Set(subjectids);
        subjectids.sort();
        for (var i = 0; i < subjectids.length; i++) {
            _ADMCombinationMaster.CombinationSubjects += subjectids[i] + '|';
        }
        _ADMCombinationMaster.CombinationSubjects = _ADMCombinationMaster.CombinationSubjects.slice(0, -1);
        var _url = getBaseUrlAdmin() + "Combination/BatchUpdateCombinationMasterByIDs";
        $.ajax({
            url: _url,
            type: "POST",
            contentType: 'application/json; charset=UTF-8',
            data: JSON.stringify(_ADMCombinationMaster),
            success: function (responseData) {
                if (responseData.IsSuccess) {
                    showSuccessMessage(responseData.SuccessMessage);
                }
                if (!isNullOrEmpty(responseData.ErrorMessage)) {
                    showErrorMessage(responseData.ErrorMessage);
                }
            },
            beforeSend: function () { showLoader(); },
            complete: function () {
                $batchDialog.modal('hide');
                hideLoader();
            },
            error: function (xhr, error, msg) {
                showErrorMessage(msg);
            }
        });

    });

    //show batch dialog
    $('#jsShowBatchReplaceSubjectDDL').click(function () {
        var validateMessage = validateCombinationFilters();
        if (!isNullOrEmpty(validateMessage)) {
            showErrorMessage(validateMessage);
            return;
        }
        var $batchDialog = $("#BatchUpdateNextSemCombDailog");
        var _param = new Parameter();
        _param.Filters = [];
        _param.Filters.push({ Column: "Semester", Operator: "EqualTo", Value: $("#SemesterDDL option:selected").val(), GroupOperation: "AND", TableAlias: "" });
        _param.Filters.push({ Column: "Combination_ID", Operator: "In", Value: getEntityList().toString(), GroupOperation: "AND", TableAlias: "" });
        fillFindSubjectDDL(_param);

        $("#jsShowReplaceSubjectDDLDailog").modal('show');
    });

    $("#jsBatchUpdateCombinationSubjectBtn").click(function () {
        debugger
        var $oldsubject = $("#FindSubject_ID option:selected");
        var $newsubject = $("#ReplaceBySubject_ID option:selected");
        var $batchs = $("#Batch option:selected");
        var $tr = $(this).closest('tr');
        var combinationids = "";
        //if ($tr !== null && $tr.length > 0) {
        //    combinationids = $tr.find('.jsEntity_ID').val();
        //} else {
        combinationids = getEntityList();
        //}

        if (isNullOrEmpty($oldsubject.val()) || isNullOrEmpty($newsubject.val())) {
            return;
        }
        var $modeldailog = $("#jsShowReplaceSubjectDDLDailog");
        $modeldailog.modal('hide');
        var msg = '<h5>Are you sure you want to replace subject in combination?</h5>';
        msg += `<h5>${$('tr.jsSelectedRow').find('.jsCombText').html()}</h5>`;
        msg += `<br/><h5>${$oldsubject.text()}</h5>`;
        msg += `With: <h5>${$newsubject.text()}</h5>`;
        showConfirmationDialog(msg);
        $('#confirmationDialog').off('click.confirmation', 'button').on('click.confirmation', 'button', function () {
            var $btn = $(this);
            hideConfirmationDialog();
            if ($btn.data('response') == 'yes') {
                var model = {};
                model.Combination_IDs = combinationids;
                model.FindSubject_ID = $oldsubject.val();
                model.ReplaceWithSubject_ID = $newsubject.val();
                model.ReplaceSubjectInResult = $modeldailog.find('#ReplaceSubjectInResult').is(':checked');
                model.InActiveFindedSubject_ID = $modeldailog.find('#InActiveFindedSubject_ID').is(':checked');
                model.RemoveFindedSubject = $modeldailog.find('#RemoveFindedSubject').is(':checked');
                model.ReplaceSubjectInSyllabus = $modeldailog.find('#ReplaceSubjectInSyllabus').is(':checked');
                model.ReplaceSubjectInTranscript = $modeldailog.find('#ReplaceSubjectInTranscript').is(':checked');
                BatchUpdateCombinationSubject(model);
            }
        });
    });

});

function BatchUpdateCombinationSubject(model) {
    var _url = getBaseUrlAdmin() + "Combination/BatchUpdateCombinationMaster";
    $.ajax({
        url: _url,
        type: "POST",
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(model),
        success: function (responseData) {
            if (responseData.IsSuccess) {
                showSuccessMessage(responseData.SuccessMessage);
            }
            if (!isNullOrEmpty(responseData.ErrorMessage)) {
                showErrorMessage(responseData.ErrorMessage);
            }
        },
        beforeSend: function () { showLoader(); },
        complete: function () {
            hideLoader();
        },
        error: function (xhr, error, msg) {
            showErrorMessage(msg);
        }
    });
}

function validateCombinationFilters() {
    var entitylist = getEntityList();
    if (entitylist.length == 0)
        return 'Please choose atleast one record...';

    //var _course_id = $(".js-pager-table #CourseDDL").find('option:selected').val();
    //if (isNullOrEmpty(_course_id))
    //    return ("Please choose course");

    var _semester = $(".js-pager-table #SemesterDDL").find('option:selected').val();
    if (isNullOrEmpty(_semester))
        return ("Please choose semester");

    //var _OldCombination_ID = $(".js-pager-table .jsCombinationDDL").find('option:selected').val();
    //if (isNullOrEmpty(_OldCombination_ID) || _OldCombination_ID == emptyGuid())
    //    return ("Please choose students of a particular combination from combinatin dropdown list");

    //$('[data-scope="batch-next-sem-row1"][data-column=Course_ID]').val(_course_id);
    //$('[data-scope="batch-next-sem-row1"][data-column=Programme]').val($(".js-pager-table .jsProgrammeDDL").find('option:selected').val());
    //$('[data-scope="batch-next-sem-row1"][data-column=Semester]').val(_semester);

    return null;
}

function DeleteCombination($tr, _ID) {
    var _url = "/" + _baseURL + "/Combination/Delete/";
    $.ajax({
        url: _url,
        type: "POST",
        datatype: "Json",
        async: false,
        data: { id: _ID },
        success: function (responseData) {
            if (responseData.IsSuccess) {
                showSuccessMessage(responseData.NumberOfRecordsEffected + ' row deleted successfully.');
                $tr.remove();
            } else {
                showErrorMessage(responseData.ErrorMessage);
            }
        },
        error: function (xhr, error, msg) {
            showErrorMessage(msg);
        }
    });
}




function fillFindSubjectDDL(_param) {
    var $targetSelect = $(`#FindSubject_ID`);
    clearDDOptions($targetSelect);

    _param.SortInfo = { ColumnName: "SubjectFullName" };
    _param.PageInfo = { PageNumber: -1, PageSize: -1, DefaultOrderByColumn: "CourseFullName,SubjectFullName" };
    var _url = '/CUSrinagarAdminPanel/General/VWSCSubjectDDLWithDetail';
    $.ajax({
        url: _url,
        type: "POST",
        datatype: "Json",
        data: _param,
        beforeSend: function () { showLoader(); },
        success: function (data) {
            fillDDLOptions($targetSelect, data);
            ChosenStyle();
            resizechosen($targetSelect);
        },
        error: function (xhr, error, msg) {
            fillDDLOptions($targetSelect, null);
            showErrorMessage(msg);
        },
        complete: function () {
            hideLoader();
        }
    });
}
