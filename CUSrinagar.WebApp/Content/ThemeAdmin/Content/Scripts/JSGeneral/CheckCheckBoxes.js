
function checkAll(topCheckAll, restCheckBoxName) {
    jQuery(document).ready(function ($) {
        if ($(topCheckAll).is(":checked")) {
            $("input[name='" + restCheckBoxName + "']").prop('checked', true);
            $('tbody tr').css('background-color', '#cfe9f1');
        } else {
            $("input[name='" + restCheckBoxName + "']").prop('checked', false);
            $('tbody tr').css('background-color', '');
        }
        TotalSelected(restCheckBoxName);
    });
}
function unCheckTop(topCheckBoxID, clickedCheckBoxName) {
    jQuery(document).ready(function ($) {
        if ($("input[name='" + clickedCheckBoxName + "']").length == $("input[name='" + clickedCheckBoxName + "']:checked").length) {
            $(topCheckBoxID).prop('checked', true);
        } else {
            $(topCheckBoxID).prop('checked', false);
        }
        HighLightSelectedTr(clickedCheckBoxName);
        TotalSelected(clickedCheckBoxName);
    });
}

function HighLightSelectedTr(clickedCheckBoxName) {
    jQuery(document).ready(function ($) {
        $("input[name='" + clickedCheckBoxName + "']").change(function () {
            if (this.checked) {
                $(this).closest('tr').css('background-color', '#cfe9f1');
            } else {
                $(this).closest('tr').css('background-color', '');
                $(this).closest('tr').find("table tr").css('background-color', '');
            }
        });
    });
}

function TotalSelected(clickedCheckBoxName) {
    jQuery(document).ready(function ($) {
        var TS = $("input[name='" + clickedCheckBoxName + "']:checked").length;
        if (Number(TS) != 0) {
            $(".TotalSelected").text(TS + " Selected");
            $("#errmsg").text("");
        }
        else {
            $(".TotalSelected").text('');
        }
    });
}