$(document).on('click', '#ForwardBtn', function () {

    if (confirm("Are you sure?") === true) {
        var form = $("#Forward-Form");
        var url = form.attr('action');
        $.ajax({
            type: "POST",
            url: url,
            data: form.serialize(), // serializes the form's elements.
            success: function (result) {
                if (result === "Success") {
                    alert('Forwarded Successfully');
                    var $pagertable = $(".js-pager-table");
                    loadpagertable();
                }
                else
                    alert(result);
            },
            error: function (result) {
                alert('Some error occured');
            }
        });
    } else {
        return;
    }
});

$(document).on('click', '#jsShowPopUp', function (event) {
    $("#Form_Id").val($(this).data("formid"));
    var currentRow =$("."+ $(this).data("formid"));
    $("#FulName").text(currentRow.find('input[id*=FullName]').val()); // get current row 2nd TD
    $("#CusRegNo").text(currentRow.find('input[id*=CUSRegistrationNo]').val()); // get
    $("#FathrName").text(currentRow.find('input[id*=FathersName]').val()); // get current row 2nd TD
    $("#MothrName").text(currentRow.find('input[id*=MothersName]').val()); // get
    $("#OldColg").text(currentRow.find('input[id*=CollegeFullName]').val()); // get
    $("#NewCollegeName").text(currentRow.find('input[id*=NewCollege]').val()); // get current row 2nd TD
    $("#Remark").text(currentRow.find('input[id*=Remarks]').val()); // get
    $("#FType").text(currentRow.find('input[id*=FormType]').val()); // get
});

$(document).on('click', '#submitremarks', function (event) {
    event.preventDefault();
    $("#REMARKSBYCOLLEGEError").html("");
    $("#AcceptRejectError").html("");
    var remarks = $("#REMARKSBYCOLLEGE").val();
    var accrej = $("#AcceptReject").val();
    var $form = $("#submit-form");
    if (remarks.trim() === "" || remarks === undefined) {
        $("#REMARKSBYCOLLEGEError").html("Required");
        return;
    }


    if ($('input[name=AcceptReject]:checked').length) {
        $form.submit();
        return true; // allow whatever action would normally happen to continue
    }
    else {
        $("#AcceptRejectError").html("Required");
        return;
    }

});


