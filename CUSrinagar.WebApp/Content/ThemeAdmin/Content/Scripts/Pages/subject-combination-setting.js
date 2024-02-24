$(document).ready(function () {

    //$("#jsNewRow .jsSemesterDDL").trigger('change');

    $("tr").on('change', ':input', function () {
        var $tr = $(this).closest('tr');
        var $recordState = $tr.find('[name*=RecordState]');
        if ($tr.find('.js_ID').val() != emptyGuid()) {
            $recordState.val('Dirty');
        }
    });


    $(".jsDeleteItem").click(function () {
        var $tr = $(this).closest('tr');
        var msg = '<h4>Are you sure you want to delete this row ?</h4>';
        //msg += '<h5>Once deleted this student, corresponding student has to reassign all its subject combination details</h5>';
        showConfirmationDialog(msg);
        $('#confirmationDialog').off('click.confirmation', 'button').on('click.confirmation', 'button', function () {
            var $btn = $(this);
            hideConfirmationDialog();
            if ($btn.data('response') == 'yes') {
                var jsMaimBaseItem_ID = $(".jsMaimBaseItem_ID").val();
                var _ID = $tr.find(".js_ID").val();
                var _url = getBaseUrlAdmin() + `CombinationSetting/DeleteSubjectCombinationSetting?id=${_ID}&MainBaseItem_ID=` + jsMaimBaseItem_ID;
                window.location = _url;
            }
        });
    });


    $(document).on('click', '.jsDeleteMainItem', function () {
        var $tr = $(this).closest('tr');
        var msg = '<h4>Are you sure you want to delete this row ?</h4>';
        //msg += '<h5>Once deleted this student, corresponding student has to reassign all its subject combination details</h5>';
        showConfirmationDialog(msg);
        $('#confirmationDialog').off('click.confirmation', 'button').on('click.confirmation', 'button', function () {
            var $btn = $(this);
            hideConfirmationDialog();
            if ($btn.data('response') == 'yes') {
                var _ID = $tr.find(".jsRow_ID").val();
                //var baseSubject_ID = $tr.find("[name*=_ID]").val();
                //var _batch = $tr.find("[name*=Batch]").val();
                //var _forSemester = $tr.find("[name*=ForSemester]").val();
                //var _url = getBaseUrlAdmin() + `CombinationSetting/DeleteBaseSubjectCombinationSetting?baseSubject_ID=${baseSubject_ID}&Batch=` + _batch;
                //window.location = _url;
                DeleteBaseSubjectCombinationSetting($tr, _ID);

            }
        });

    });


});

function DeleteBaseSubjectCombinationSetting($tr, _ID) {
    var _url = getBaseUrlAdmin() + `CombinationSetting/DeleteBaseSubjectCombinationSetting`;
    $.ajax({
        url: _url,
        type: "POST",
        datatype: "Json",
        async: false,
        data: { id: _ID},
        success: function (responseData) {
            if (responseData.IsSuccess) {
                showSuccessMessage(responseData.NumberOfRecordsEffected + ' deleted successfully.');
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