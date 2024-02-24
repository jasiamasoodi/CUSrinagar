jQuery(function ($) {

    $(document).on('change', '.jsCollegeDDL', function (e) {
        var college_ID = $(this).val();

        var path = chilDDLPathProgramme;
        $(".jsDDLProgrammes").html("<span style='color:#62a8d1'>...Please Wait...");
        $.ajax({
            url: path,
            data: { College_ID: college_ID },
            datatype: "json",
            contentType: "application/json; charset=utf-8",
            encode: true,
            type: "GET",
            cache: false,
            async: true,
            success: function (response) {
                var PartialView = response;
                $(".jsDDLProgrammes").html(PartialView);
                ChosenStyle();
            }
        });
    });

    $(document).on('change', '.jsProgrammeDDL', function (e) {

        var college_ID = $(".jsCollegeDDL").val();
        var Prog = $(this).find(":selected").val();

        var path = chilDDLPathCourse;
        if (Prog != "" && Prog != undefined) {
            $(".jsDDLCourse").html("<span style='color:#62a8d1'>...Please Wait...");
            $.ajax({
                url: path,
                data: { College_ID: college_ID, programme: Prog },
                datatype: "json",
                contentType: "application/json; charset=utf-8",
                encode: true,
                type: "GET",
                cache: false,
                async: true,
                success: function (response) {
                    var PartialView = response;
                    $(".jsDDLCourse").html(PartialView);
                    ChosenStyle();
                }
            });
        }
    });

});