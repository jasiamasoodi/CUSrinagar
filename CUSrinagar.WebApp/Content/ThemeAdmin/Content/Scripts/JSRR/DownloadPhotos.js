$(document).ready(function ($) {
    $(".JSDownloadBtn").prop("disabled", false);
    $("#ProgrammeId").val('');

    function urlToPromise(url) {
        return new Promise(function (resolve, reject) {
            JSZipUtils.getBinaryContent(url, function (err, data) {
                if (err) {
                    reject(err);
                } else {
                    resolve(data);
                }
            });
        });
    }

    $(".JSDownloadBtn").click(function () {
        $(this).empty().html('<i class="fa fa-spinner"></i>&nbsp;Working...').prop("disabled", true);
        var fileName = $(".PhotoFileName").attr("data-string");
        var zip = new JSZip();
        var img = zip.folder(fileName);

        var ImageName = "";
        var dataURL = "";

        $('#JSPhotos img').each(function () {
            dataURL = $(this).attr("src");
            ImageName = $(this).attr("data-FormNo") + ".jpg";
            img.file(ImageName, urlToPromise(dataURL), { base64: true });

            ImageName = "";
            dataURL = "";
        });
        zip.generateAsync({ type: "blob" }).then(function (content) {
            saveAs(content, fileName + ".zip");
        });

        $(this).empty().html('<span class="quadraText"><i class="fa fa-download"></i></span>Download as ZIP').prop("disabled", false);
    });

    $("#frmDownloadPhotos").submit(function () {
        var year = $("input[name='Year']").val();
        var ProgrammeId = $("select[name='ProgrammeId']").val();
        var CourseID = $("select[name='CourseID']").val();
        var Semester = $("select[name='Semester']").val();
        if (year !== "" && ProgrammeId !== "" && CourseID !== "" && Semester !== "") {
            $(".JSBtnSearchPhotos").empty().html('<i class="fa fa-eye"></i>&nbsp;Working...').prop("disabled", true);
            return true;
        } else {
            alert("All fields are required.");
        }
        return false;
    });
    $("#Jspleasewait").css("display", "none");
});