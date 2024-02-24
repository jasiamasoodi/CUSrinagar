

function saveRapidEntryRow($tr, resolve, reject) {
    var _model = createModelFromForm($tr);
    var _url = getBaseUrlAdmin() + "Result/PostRevaluationListItem";
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


