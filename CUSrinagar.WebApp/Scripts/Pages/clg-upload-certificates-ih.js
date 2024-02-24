$(document).ready(function () {
    $("[name*=VerificationStatus]").change(function () {
        var $tr = $(this).closest('tr');
        var $recordState = $tr.find('[name*=RecordState]');
        $recordState.val('Dirty');
        validatePaymentOption();
    });

    $('.jsShowPreview').click(function () {
        var $img = $(this);
        var $batchDialog = $("#PreviewPopUp");
        $("#img-preview").attr('src',$img.attr('src'));
        $batchDialog.modal('show');
    });




});

function validatePaymentOption() {
    var showPayment = true;
    $("[name*=VerificationStatus]").each(function (index,element) {
        if ($(element).find('option:selected').val() != "2" && showPayment) showPayment = false;
    });
    if (showPayment) {
        $("#AllowPayment").attr('disabled',false).removeAttr('disabled');
    } else {
        $("#AllowPayment").attr('disabled','disabled');
    }
}