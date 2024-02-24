/*----------------------------------------------------------------------
                    Forms having input type="file"
------------------------------------------------------------------------*/
function valFile(id, minSizeorg, maxSizeorg, KbMb, exts, erMsg) {
    jQuery(document).ready(function ($) {
        if ($(id).val() != '') {
            maxSize = maxSizeorg * 1024;// bytes to kb
            minSize = minSizeorg * 1024;
            var file = $(id)[0].files[0];
            var fileName = file.name;
            var fileExt = fileName.split('.').pop().toLowerCase();
            var fileSize = file.size;
            exts = exts.toLowerCase();
            $(erMsg).text("");
            $("#modelError").empty();
            if (exts.search(fileExt) != -1) {//not found returns -1
                // check if maxsize in mb what to display and if maxsize in kb what to display
                var disSizeAs = maxSizeorg;
                if (KbMb.toLowerCase() == "mb") {
                    disSizeAs = maxSizeorg / 1024;
                }
                if (fileSize <= minSize) {
                    if (maxSize != 0)
                        $(erMsg).text("Too small(Allowed " + minSizeorg + "KB - " + disSizeAs + KbMb + ")");
                    else
                        $(erMsg).text("Too small(atleast  " + minSizeorg + "KB)");
                    $(id).val('');
                    $(id).focus();
                    previewFile(id);
                    previewFileEdit(id);
                    return false;
                }
                else {
                    if (maxSize != 0 && fileSize >= maxSize) { //0 means not required
                        $(erMsg).text("Too big(Allowed " + minSizeorg + "KB - " + disSizeAs + KbMb + ")");
                        $(id).val('');
                        $(id).focus();
                        previewFile(id);
                        previewFileEdit(id);
                        return false;
                    } else {
                        previewFile(id);
                        previewFileEdit(id);
                        return true;
                    }
                }//2nd if
            }
            else {
                $(erMsg).text("Invalid file type");
                $(id).val('');
                $(id).focus();
                previewFile(id);
                previewFileEdit(id);
                return false;//ext not allowed
            }
        }//top if
        else {
            return false;
        }
    });
}

function previewFile(id) {
    jQuery(document).ready(function ($) {
        try {
            var imgPath = $(id)[0].value;
            if ($.trim(imgPath) != "") {
                if (typeof (FileReader) != "undefined") {
                    var reader = new FileReader();
                    reader.onload = function (e) {
                        $("#div-preview").slideDown(100);
                        $("#img-preview").attr('src', e.target.result);
                    }
                    reader.readAsDataURL($(id)[0].files[0]);
                } else {
                    $("#div-preview").slideDown(100);
                    $("#img-preview").attr('src', '').attr('alt', 'Preview not supported by browser');
                }
            } else {
                $("#div-preview").slideUp(100);
                $("#img-preview").attr('src', '');
            }
        } catch (err) { }
    });
}