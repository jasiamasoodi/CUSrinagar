window.oncontextmenu = function () {
    return false;
}

function blockBrowser() {
    var BrowserAgent = navigator.userAgent + "";
    if (BrowserAgent.indexOf("UBrowser/") >= 0 || BrowserAgent.indexOf("UCBrowser/") >= 0) {
        location.href = "/Error/BrowserBlocked";
    }
}
blockBrowser();

$(document).ready(function ($) {
    $("#frmSZone").submit(function (e) {
        if ($(this).valid()) {
            $("#jsStudentZone").prop("disabled", true).html('<i class="fa fa-download"></i> Working on it...');
            return true;
        }
        $("#jsStudentZone").prop("disabled", false).html('<i class="fa fa-twitter"></i>Sign In');
        return false;
    });

    $.widget("custom.catcomplete", $.ui.autocomplete, {
        _create: function () {
            this._super();
            this.widget().menu("option", "items", "> :not(.ui-autocomplete-category)");
        },
        _renderMenu: function (ul, items) {
            var that = this,
                currentCategory = "";
            $.each(items, function (index, item) {
                var li;
                if (item.category !== currentCategory) {
                    ul.append("<li class='ui-autocomplete-category'>" + item.category + "</li>");
                    currentCategory = item.category;
                }
                li = that._renderItemData(ul, item);
                if (item.category) {
                    li.attr("aria-label", item.category + " : " + item.label);
                }
            });
            $("#ui-id-1").css("max-width", "91%");
        }
    });



    StudentListAutoComplete();
    function StudentListAutoComplete() {
        var _url = "/Account/GetStudentsForLogin";
        $.ajax({
            url: _url,
            type: "POST",
            datatype: "Json",
            success: function (data) {
                var list = [];
                if (data.length > 0) {
                    for (i = 0; i < data.length; i++) {
                        var obj = { category: data[i].CollegeFullName, label: data[i].CourseFullName, Course_ID: data[i].Course_ID, College_ID: data[i].College_ID };
                        list.push(obj);
                    }
                    $("#studentListAutoComplete").catcomplete({
                        delay: 0,
                        source: list,
                        autoFocus: true,
                        select: function (event, ui) {

                            var _url = "/Account/GetParticularStudentForLogin?College_ID=" + ui.item.College_ID + "&Course_ID=" + ui.item.Course_ID;

                            if ($("#SearchBatch").length > 0) {
                                _url += "&Batch=" + $("#SearchBatch").val();
                            }

                            $.ajax({
                                url: _url,
                                type: "POST",
                                datatype: "Json",
                                success: function (data) {
                                    var arr = data.Text.split('|');

                                    $('#EnteredDOB').val(data.Value);
                                    $('#FormNo').val(arr[0]);
                                    $(`#PrintProgrammeOption option[value=${arr[1]}]`).attr("selected", "selected");
                                }
                            });
                        }
                    });
                }
            }
        });
    }
});

