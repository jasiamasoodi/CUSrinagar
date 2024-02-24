jQuery(function ($) {


    $(document).on('change', '.SearchDDL', function (e) {
        var id = $(this).val();
        var path = chilDDLPath;
        $.ajax({
            url: path,
            data: { CollegeID: id },
            datatype: "json",
            contentType: "application/json; charset=utf-8",
            encode: true,
            type: "Get",
            cache: false,
            async: false,
            success: function (response) {
                var PartialView = response;
                $('#Course').html(PartialView); // Modify me

                ChosenStyle();
            }
        });

    });

    $("#jsPrintCombination").click(function () {
        $("#CombinationSet").removeAttr('style');
        window.print();

    });

});
