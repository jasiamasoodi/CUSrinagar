$(document).ready(function () {
    $('form').submit(function () {
        var $form = $(this);

        if (!$form.valid()) {
            return false;
        }


    });

    $("[name*=File]").change(function () {
        var $tr = $(this).closest('.jsFileContainer');
        var $recordState = $tr.find('[name*=RecordState]');
        $recordState.val('Dirty');
    });


    //$('#btn-make-payment').click(function () {
    //    var $btn = $(this);
    //    var msg = '<h4>Are you sure you want to go payment?</h4>';
    //    msg += '<h3 class=" text-danger">------------------------------</h3>';
    //    showConfirmationDialog(msg);
    //    $('#confirmationDialog').off('click.confirmation', 'button').on('click.confirmation', 'button', function () {
    //        window.location= ($btn.attr('href'));
    //    });
    //});


    //var numFiles = 0;

    //$("[name*=File]").blur(function (event) {
    //    var target = event.target || event.srcElement;
    //    console.log(event);
    //    console.log(target.value);
    //    console.log(target.files);
    //    console.log(target.files.length);
    //    //if (target.value.length == 0) {
    //    //    console.log("Suspect Cancel was hit, no files selected.");
            
    //    //} else {
    //    //    console.log("File selected: ", target.value);
    //    //    numFiles = target.files.length;
    //    //}
    //});


});