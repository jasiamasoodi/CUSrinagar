
//function uploadFile() {
//    $("#myframe").load(function () {
//        var frameData = $(this).contents().find("pre").text();
//        var successText = "\"success\"";
//        if (frameData == successText) {
//            $.ajax({
//                url: '/CUSrinagarAdminPanel/Notification/List',
//                contentType: 'application/html; charset=utf-8',
//                type: 'GET',
//                dataType: 'html'

//            })
//                .success(function (result) {
//                    alert("Notification Created Successfully");
//                    $('#pagecontent').html(result);
//                })
//        }
//        else if (frameData.indexOf("Error,") > -1 && frameData.indexOf("Error,") < 10) {
//            alert(frameData);
//            $("#Error1").hide();
//            $("#Error").hide();
//        }
//        else {
//            //              alert("Validation Error");
//            $('#pagecontent').html(JSON.parse(frameData));
//        }
//    });
    
//}

function MessageBox(response) {
    $("#responseMessage").html(response);
    var dialog = $("#dialog-message").removeClass('hide').dialog({
        modal: true,
        title_html: true,
        title: "License Limit",
        buttons: [
            {
                text: "OK",
                "class": "btn btn-primary btn-xs",
                click: function () {
                    $(this).dialog("close");
                    $(".green").click();

                }
            }
        ]
    });
}

var $form = $('#validation-form');
var file_input = $form.find('input[type=file]');
var upload_in_progress = false;
var count = 0;
var size = 0.0;
var sfiles = [];
jQuery(function ($) {
    var filetypes = _AllowedTypes;
    filetypes = filetypes.split(","); // ["\'"+filetypes.replace(/,/g, "\',\'")+"\'"];
    file_input.ace_file_input({
        hasFileList: true,
        multi: true,
        style: 'well',
        btn_choose: 'Select or drop files here',
        btn_change: null,
        droppable: true,
        thumbnail: 'large',
        can_reset: true,
        maxSize: _MaxUploadLimitInBytes,//100megabytes
        allowExt: filetypes,
        //  allowMime: ["image/jpg", "image/jpeg", "image/png", "image/gif", "application/mspowerpoint", "application/powerpoint", "application/x-mspowerpoint"],
        before_remove: function () {
            if (upload_in_progress)
                return false;//if we are in the middle of uploading a file, don't allow resetting file input
            return true;
        },

        preview_error: function (filename, code) {
            //code = 1 means file load error
            //code = 2 image load error (possibly file is not an image)
            //code = 3 preview failed
        }
    })
    file_input.on('file.error.ace', function (ev, info) {
        var filetype = _AllowedTypes;
        filetype = filetype.replace("*", ""); // ["\'"+filetypes.replace(/,/g, "\',\'")+"\'"];
        filetype = filetype.replace("*", "");
        filetype = filetype.replace(/,/g, ", ");
        if (info.error_count['ext'] || info.error_count['mime']) alert('Invalid file type! Allowed file types are: ' + filetype + '  ');
        if (info.error_count['size']) alert('Invalid file size! Maximum size is ' + _MaxUploadLimitInMB + ' MB.');

        //you can reset previous selection on error
        //ev.preventDefault();
        //file_input.ace_file_input('reset_input');
    });
    file_input.on('change', function () {
        $(this).each(function () {
            var field_name = $(this).attr('name');
            var files = $(this).data('ace_input_files');
            if (files && files.length > 0) {
                for (var f = 0; f < files.length; f++) {
                    size += files[f].size;
                }
            }
            sfiles.push(files[f]);
        });

        if (sfiles && sfiles.length > 0) {
            if (parseFloat($("#SizeLimit").val()) > 0) {
                if ((parseFloat(size)) > parseFloat($("#AllowedSizeInBytes").val())) {
                    MessageBox("<div class='alert alert-block alert-danger'>You are allowed to upload total documents of size " + parseFloat($("#AllowedSizeInMb").val()).toPrecision(2) + " (MB).Please increase your license storage.</div>")
                    file_input.ace_file_input('reset_input');
                }
                else {
                    for (var f = 0; f < sfiles.length; f++) {
                        $("#Files").push(sfiles[f]);
                    }
                }
            }
            else {
                for (var f = 0; f < sfiles.length; f++) {
                    $("#Files").push(sfiles[f]);
                }
            }

        }

    });
   
});

