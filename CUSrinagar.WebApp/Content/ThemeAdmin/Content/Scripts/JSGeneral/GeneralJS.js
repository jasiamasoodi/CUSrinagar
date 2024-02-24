function charsOnly(e) {
    var charCode;
    if (window.event)         // IE
    { charCode = e.keyCode; }

    else if (e.which)            // Netscape/Firefox/Opera
    { charCode = e.which; }
    if (charCode == undefined || charCode == 45 || charCode == 46 || charCode == 8 || charCode == 9 || charCode == 38 || charCode == 32 || (charCode >= 97 && charCode <= 122) || (charCode >= 65 && charCode <= 90))
        return true;
    else
        return false;
}
function numbersOnly(e) {
    var charCode;
    if (window.event)         // IE
    { charCode = e.keyCode }

    else if (e.which)            // Netscape/Firefox/Opera
    { charCode = e.which }
    if (charCode == undefined || charCode == 8 || charCode == 9 || (charCode >= 48 && charCode <= 57))
        return true;
    else
        return false;
}
function IntegersOnly(e) {
    var charCode;
    if (window.event)         // IE
    { charCode = e.keyCode }

    else if (e.which)            // Netscape/Firefox/Opera
    { charCode = e.which }
    if (charCode == undefined || charCode == 45 || charCode == 8 || charCode == 9 || (charCode >= 48 && charCode <= 57))
        return true;
    else
        return false;
}
function charno(e) {
    var charCode;
    if (window.event)         // IE
    { charCode = e.keyCode }

    else if (e.which)            // Netscape/Firefox/Opera
    { charCode = e.which }
    if (charCode == undefined || charCode == 8 || charCode == 93 || charCode == 125 || charCode == 9 || (charCode >= 45 && charCode <= 57) || charCode == 32 || (charCode >= 97 && charCode <= 123) || (charCode >= 65 && charCode <= 91))
        return true;
    else
        return false;
}
//alphabets - . / & \ ' ,() and numbers space del bkspace left arrow right arrow
function forAddressOnly(e) {
    var charCode;
    if (window.event)         // IE
    { charCode = e.keyCode }

    else if (e.which)            // Netscape/Firefox/Opera
    { charCode = e.which }
    if (charCode == undefined || charCode == 95 || charCode == 8 || charCode == 9 || (charCode >= 44 && charCode <= 57) || (charCode >= 97 && charCode <= 122) || (charCode >= 65 && charCode <= 90) || (charCode >= 48 && charCode <= 57) || charCode == 40 || charCode == 41 || charCode == 92 || charCode == 34 || charCode == 32 || charCode == 38 || e.keyCode == 46 || charCode == 39 || charCode == 9)
        return true;
    return false;
}
// chars and - only
function forNameOnly(e) {
    var charCode;
    if (window.event)         // IE
    { charCode = e.keyCode }

    else if (e.which)            // Netscape/Firefox/Opera
    { charCode = e.which }
    if (charCode == undefined || charCode == undefined || charCode == 8 || charCode == 9 || (charCode >= 97 && charCode <= 122) || (charCode >= 65 && charCode <= 90) || charCode == 45 || charCode == 32 || charCode == 9 || charCode == 45 || charCode == 46)
        return true;
    return false;
}
//Admission RollNo chars and nos -/.
function forAdmissionRollNo(e) {
    var charCode;
    if (window.event)         // IE
    { charCode = e.keyCode }

    else if (e.which)            // Netscape/Firefox/Opera
    { charCode = e.which }
    if (charCode == undefined || charCode == 8 || charCode == 9 || (charCode >= 97 && charCode <= 122) || (charCode >= 65 && charCode <= 90) || (charCode >= 48 && charCode <= 57) || charCode == 47 || charCode == 45 || charCode == 46 || charCode == 9 || charCode == 92)
        return true;
    return false;
}
// chars and .- only
function forMsgNameOnly(e) {
    var charCode;
    if (window.event)         // IE
    { charCode = e.keyCode }

    else if (e.which)            // Netscape/Firefox/Opera
    { charCode = e.which }
    if (charCode == undefined || charCode == 46 || charCode == 8 || charCode == 9 || (charCode >= 97 && charCode <= 122) || (charCode >= 65 && charCode <= 90) || charCode == 45 || charCode == 32 || charCode == 9)
        return true;
    return false;
}

function forCollegeNameOnly(e) {
    var charCode;
    if (window.event)         // IE
    { charCode = e.keyCode }

    else if (e.which)            // Netscape/Firefox/Opera
    { charCode = e.which }
    if (charCode == 47) {
        return false;
    } else {
        if (charCode == undefined || charCode == 95 || charCode == 8 || charCode == 9 || (charCode >= 44 && charCode <= 57) || (charCode >= 97 && charCode <= 122) || (charCode >= 65 && charCode <= 90) || (charCode >= 48 && charCode <= 57) || charCode == 40 || charCode == 41 || charCode == 32 || charCode == 38 || charCode == 46 || charCode == 39 || charCode == 9)
            return true;
        else
            return false;
    }
}

//dynamic urls chars and nos -_.
function forDynamicUrl(e) {
    var charCode;
    if (window.event)         // IE
    { charCode = e.keyCode }

    else if (e.which)            // Netscape/Firefox/Opera
    { charCode = e.which }
    if (charCode == undefined || charCode == 8 || charCode == 9 || (charCode >= 97 && charCode <= 122) || (charCode >= 65 && charCode <= 90) || (charCode >= 48 && charCode <= 57) || charCode == 45 || charCode == 46 || charCode == 9 || charCode == 95)
        return true;
    return false;
}

function floatNumbersOnly(e) {
    var charCode;
    if (window.event)         // IE
    { charCode = e.keyCode }

    else if (e.which)            // Netscape/Firefox/Opera
    { charCode = e.which }
    if (charCode == undefined || charCode == 46 || charCode == 8 || charCode == 9 || (charCode >= 48 && charCode <= 57))
        return true;
    else
        return false;
}

function dateOnly(e) {
    var charCode;
    if (window.event)         // IE
    { charCode = e.keyCode }

    else if (e.which)            // Netscape/Firefox/Opera
    { charCode = e.which }
    if (charCode == undefined || charCode == 45 || charCode == 8 || charCode == 9 || (charCode >= 48 && charCode <= 57))
        return true;
    else
        return false;
}

$(document).ready(function ($) {
    $("input,textarea").keypress(function (e) {
        if ($(this).hasClass("allowHTML"))
            return true;

        var charCode;
        if (window.event)         // IE
        { charCode = e.keyCode }

        else if (e.which)// Netscape/Firefox/Opera
        { charCode = e.which }
        if (charCode == 60 || charCode == 62)
            return false;
        else
            return true;
    });

    function DateMasking() {
        if ($('.dateDDMMYYY').length > 0) {
            $('.dateDDMMYYY').mask('D0-M0-0Y00', {
                translation: {
                    'D': { pattern: /[0-3]/, optional: true },
                    'M': { pattern: /[0-2]/, optional: true },
                    'Y': { pattern: /[0-9]/ }
                }
            });
        }
    }
    DateMasking();
});



function valFile(id, minSizeorg, maxSizeorg, KbMb, exts, erMsg) {
    $(document).ready(function ($) {
        try {
            showLoader();
            if ($(id).val() != '') {
                maxSize = maxSizeorg * 1024;// bytes to kb
                minSize = minSizeorg * 1024;
                var file = $(id)[0].files[0];
                var fileName = String(file.name);
                var fileExt = fileName.split('.').pop().toLowerCase();
                var fileSize = file.size;
                exts = exts.toLowerCase();
                var $erMsg = $(erMsg);
                if ($(`${id}-error`).length > 0) {
                    $erMsg = $(`${id}-error`).addClass('hidden');
                }
                $erMsg.text("");
                $("#modelError").empty();
                if (exts.search(fileExt) != -1) {//not found returns -1
                    // check if maxsize in mb what to display and if maxsize in kb what to display
                    var disSizeAs = maxSizeorg;
                    if (KbMb.toLowerCase() == "mb") {
                        disSizeAs = maxSizeorg / 1024;
                    }
                   
                    if (fileSize <= minSize) {
                        if (maxSize != 0)
                            $erMsg.text("Image size is too small (Allowed " + minSizeorg + "KB - " + disSizeAs + KbMb + ")").removeClass('hidden');
                        else
                            $erMsg.text("Image size is too small (atleast  " + minSizeorg + "KB)").removeClass('hidden');
                        $(id).val('');
                        $(id).focus().addClass('error');
                        previewFile(id);
                        return false;
                    }
                    else {
                        if (maxSize != 0 && fileSize >= maxSize) { //0 means not required
                            $erMsg.text("Image size exceedes (Allowed " + minSizeorg + "KB - " + disSizeAs + KbMb + ")").removeClass('hidden');
                            $(id).val('');
                            $(id).focus().addClass('error');
                            previewFile(id);
                            return false;
                        } else {
                            previewFile(id);
                            return true;
                        }
                    }//2nd if
                }
                else {
                    $erMsg.text("Invalid file type").html("Invalid file type");
                    $(id).val('');
                    $(id).focus();
                    previewFile(id);
                    return false;//ext not allowed
                }
            }//top if
            else {
                return false;
            }
        } catch (e){
            return false;
        } finally {
            hideLoader();
        }
    });
}

function previewFile(id) {
    try {
        var imgPath = $(id)[0].value;
        var $preview = $("#img-preview");
        if ($(`${id}-img-preview`).length > 0) {
            $preview = $(`${id}-img-preview`);
            $preview.closest('.jsPreviewSection').removeClass('hidden');
        }
        if ($.trim(imgPath) != "") {
            if (typeof (FileReader) != "undefined") {
                var reader = new FileReader();
                reader.onload = function (e) {
                    $("#div-preview").slideDown(100);
                    $preview.attr('src', e.target.result);
                }
                reader.readAsDataURL($(id)[0].files[0]);
            } else {
                $("#div-preview").slideDown(100);
                $preview.attr('src', '').attr('alt', 'Preview not supported by browser');
            }
        } else {
            $("#div-preview").slideUp(100);
            $preview.attr('src', '');
        }
    } catch (err) {
        //nothing
    }
}


function valFileCertificates(id, minSizeorg, maxSizeorg, KbMb, exts, erMsg,PreviewDiv) {
    $(document).ready(function ($) {
        try {
            showLoader();
            if ($(id).val() != '') {
                maxSize = maxSizeorg * 1024;// bytes to kb
                minSize = minSizeorg * 1024;
                var file = $(id)[0].files[0];
                var fileName = String(file.name);
                var fileExt = fileName.split('.').pop().toLowerCase();
                var fileSize = file.size;
                exts = exts.toLowerCase();
                var $erMsg = $(erMsg);
                if ($(`${id}-error`).length > 0) {
                    $erMsg = $(`${id}-error`).addClass('hidden');
                }
                $erMsg.text("");
                $("#modelError").empty();
                if (exts.search(fileExt) != -1) {//not found returns -1
                    // check if maxsize in mb what to display and if maxsize in kb what to display
                    var disSizeAs = maxSizeorg;
                    if (KbMb.toLowerCase() == "mb") {
                        disSizeAs = maxSizeorg / 1024;
                    }

                    if (fileSize <= minSize) {
                        if (maxSize != 0)
                            $erMsg.text("Image size is too small (Allowed " + minSizeorg + "KB - " + disSizeAs + KbMb + ")").removeClass('hidden');
                        else
                            $erMsg.text("Image size is too small (atleast  " + minSizeorg + "KB)").removeClass('hidden');
                        $(id).val('');
                        $(id).focus().addClass('error');
                        previewFileC(id, PreviewDiv);
                        return false;
                    }
                    else {
                        if (maxSize != 0 && fileSize >= maxSize) { //0 means not required
                            $erMsg.text("Image size exceedes (Allowed " + minSizeorg + "KB - " + disSizeAs + KbMb + ")").removeClass('hidden');
                            $(id).val('');
                            $(id).focus().addClass('error');
                            previewFileC(id, PreviewDiv);
                            return false;
                        } else {
                            previewFileC(id, PreviewDiv);
                            return true;
                        }
                    }//2nd if
                }
                else {
                    $erMsg.text("Invalid file type").html("Invalid file type");
                    $(id).val('');
                    $(id).focus();
                    previewFileC(id, PreviewDiv);
                    return false;//ext not allowed
                }
            }//top if
            else {
                return false;
            }
        } catch (e) {
            return false;
        } finally {
            hideLoader();
        }
    });
}

function previewFileC(id,previewDiv) {
    try {
        var imgPath = $(id)[0].value;
        var $preview = $(previewDiv);
        if ($(`${id}-img-preview`).length > 0) {
            $preview = $(`${id}-img-preview`);
            $preview.closest('.jsPreviewSection').removeClass('hidden');
        }
        if ($.trim(imgPath) != "") {
            if (typeof (FileReader) != "undefined") {
                var reader = new FileReader();
                reader.onload = function (e) {
                    $("#div-preview").slideDown(100);
                    $preview.attr('src', e.target.result);
                }
                reader.readAsDataURL($(id)[0].files[0]);
            } else {
                $("#div-preview").slideDown(100);
                $preview.attr('src', '').attr('alt', 'Preview not supported by browser');
            }
        } else {
            $("#div-preview").slideUp(100);
            $preview.attr('src', '');
        }
    } catch (err) {
        //nothing
    }
}