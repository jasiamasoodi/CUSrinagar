var AppRoles = {
    University: 1,
    College: 2,
    College_AssistantProfessor: 3,
    University_Registrar: 4,
    University_OfficeAssistant: 5,
    College_Clerk: 6,
    Student: 7,
    College_Principal: 8,
    University_Dean: 9,
    University_DeputyController: 10,
    University_Evaluator: 11,
    University_GrievanceFeedback: 14,
};
var PrintProgramme = {
    UG: 1,
    PG: 2,
    IH: 3,
    BED: 5
};
var Programme = {
    UG: 1,
    PG: 2,
    IG: 3,
    HS: 4,
    Engineering: 5,
    Professional: 6
};
var SubjectType = {
    None: 0,
    Core: 1,
    SEC: 3,
    GE: 4,
    MIL: 5,
    AE: 6,
    OE: 7,
    DSE: 8,
    FirstSemesterExclusion: 9,
    DCE: 10,
    OC: 11,
    Practical_Lab: 12,
};
var ExaminationCourseCategory = {
    UG: 1,
    BED: 2,
    PG: 3,
    MED: 4,
    IH: 5,
    ENG: 6
}

function GetPrintProgrammeForExaminationCourseCategory(courseCategory) {
    var programme = PrintProgramme.UG;
    courseCategory = parseInt(courseCategory);
    switch (courseCategory) {
        case ExaminationCourseCategory.UG:
            programme = PrintProgramme.UG;
            break;
        case ExaminationCourseCategory.BED:
            programme = PrintProgramme.UG;
            break;
        case ExaminationCourseCategory.PG:
            programme = PrintProgramme.PG;
            break;
        case ExaminationCourseCategory.MED:
            programme = PrintProgramme.PG;
            break;
        case ExaminationCourseCategory.IH:
            programme = PrintProgramme.IH;
            break;
        case ExaminationCourseCategory.ENG:
            programme = PrintProgramme.IH;
            break;
        default:
            break;
    }
    return programme;
}

function printAll() {
    $('.jsPrintTable').closest('.jsPrintTabContent').removeClass('hidden-print');
    $('.collapse').each(function (index, element) {
        if (!$(element).hasClass('in')) {
            $(element).collapse('show');
        }
    });
    $('.panel-heading').click();
    setTimeout(function () {
        window.print();
    }, 1000);
}

$(document).ready(function () {

    $(document).on('click', '.jsPrintTable', function () {
        $('.jsPrintTable').closest('.jsPrintTabContent').addClass('hidden-print');
        var $currentTable = $(this);
        $currentTable.closest('.jsPrintTabContent').removeClass('hidden-print');
        window.print();
    });





    $(document).on('click', '.jsWindowBack', function () {
        goBack();
    });
    $(document).on('click', '.copy-to-clipboard', function () {
        var $element = $(this).closest('tr').find('.jsEntity_ID');
        var $textbox = `<input type='text' value='${$element.val()}' id='tempCopyValueElement' />`;
        $(this).closest('tr').append($textbox);
        var $copiedElement = $(this).closest('tr').find("#tempCopyValueElement");
        $copiedElement.focus();
        $copiedElement.select();
        document.execCommand('copy');
        console.log($element.val());
        setTimeout(function () {
            $("#tempCopyValueElement").remove();
        }, 100);
    });

    $(".close").click(function () {
        $(this).closest("div.alert").hide();
        //$("#successMessage,#errorMessage").alert("close");
    });
    //$("#successMessage,#errorMessage").on('closed.bs.alert', function () {
    //    alert('The alert message is now closed.');
    //});

    bindStyleEvents();
    QuickLaunch();
    setTimeout(setActiveInActiveTabs, 500);
});

function goBack() {
    window.history.back();
}

function hideLoader() {
    $(".loader").hide().remove();
}
function showLoader() {
    var $loader = $(`<div class="loader" style="display:none;"></div>`);
    $('body').append($loader);
    $loader.show();
    //$(".loader").show();
}
function showWaitingDialog(message) {
    if (isNullOrEmpty(message)) { message = 'Please wait...'; }
    var $waitingDailog = $(`<div class="waitingDialog"><img src="/Content/ThemeAdmin/Content/Libraries/images/loading-bars2.gif" /><br/><p>` + message + `</p></div>`);
    $('body').append($waitingDailog);
    $waitingDailog.show();
}
function hideWaitingDialog() {
    $(".waitingDialog").hide().remove();
}
function getBaseUrlWebApp() {
    return $("#webbaseurl").val().replace("controller/action", "");
}
function getBaseUrlCollege() {
    return $("#collegebaseurl").val().replace("controller/action", "");
}
function getBaseUrlAdmin() {
    return $("#adminbaseurl").val().replace("controller/action", "");
}
function getBaseUrlStudentZone() {
    return $("#studentbaseurl").val().replace("controller/action", "");
}
function emptyGuid() {
    return "00000000-0000-0000-0000-000000000000";
}

function showSuccessMessage(message, delay) {
    if (isNullOrEmpty(message)) return;
    var $section = $("#SuccessMessageContainer");
    if ($section.length == 0) {
        $section = $(`<div class="alert alert-success" id="SuccessMessageContainer" style="display:none;">
                        <a href="#" class="close">&times;</a>
                        <strong>Success!&nbsp;</strong><span> Record saved successfully.</span>
                    </div>`);
        $(".main-content .page-content").prepend($section);
        $section.on('click', '.close', function () { $(this).closest('div.alert').hide(); });
    }
    $section.find('span').html(message);
    window.scrollTo(0, 0);
    delay = delay || 10000;
    $("#SuccessMessageContainer").fadeIn(1000).delay(delay).fadeOut(2000);
    //no need to remove section
}

function showErrorMessage(message, delay) {
    if (isNullOrEmpty(message)) return;
    var $section = $("#ErrorMessageContainer");
    if ($section.length == 0) {
        $section = $(` <div class="alert alert-danger" id="ErrorMessageContainer" style="display: none;">
            <a href="#" class="close">&times;</a>
            <strong>Information!&nbsp;</strong><span> ${message}.</span>
        </div>`);
        $(".main-content .page-content").prepend($section);
        $section.on('click', '.close', function () { $(this).closest('div.alert').hide(); });
    }
    $section.find('span').html(message);
    window.scrollTo(0, 0);
    delay = delay || 10000;
    $("#ErrorMessageContainer").fadeIn(1000).delay(delay).fadeOut(1000);
}

function showSuccessAlertMessage(message, delay) {
    if (isNullOrEmpty(message)) return;
    $("#SuccessAlertMessage").remove();
    var $section = $(`<div class="modal success-popup green" id="SuccessAlertMessage"  role="dialog" style="opacity:0.9">
            <div class="modal-dialog modal-sm" role="document">
                <div class="modal-content" style="border-radius:10px">
                    <div class="modal-header  text-center">               
                        <h4 class="modal-title">Success</h4>
                    </div>
                    <div class="modal-body text-center">
                        <p class="text-center"><i class="fa fa-check-square-o fa-5x" aria-hidden="true"></i></p>
                        <p class="lead"><b class="message">Record Saved Successfully</b></p>
                    </div>
                </div>
            </div>
        </div>`);
    $section.find('.message').html(message);
    $("body").prepend($section);
    delay = delay || 1000;
    $("#SuccessAlertMessage").fadeIn(delay).fadeOut(delay, function () { $("#SuccessAlertMessage").remove(); });
    //no need to remove section
}
function showAlertDialog(message) {
    if (isNullOrEmpty(message)) return;
    var $section = $("#AlertDialog");
    if ($section.length == 0) {
        $section = $(`<div class="modal fade" id="AlertDialog" tabindex="-1" role="dialog"  aria-hidden="true" style="z-index:1051;">
                    <div class="modal-dialog" role="document">
                        <div class="modal-content">
                            <div class="modal-header">
                                <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                                    <span aria-hidden="true">&times;</span>
                                </button>
                                <h4 class="modal-title">Alert</h4>
                            </div>
                            <div class="modal-body">
                                <span class='message'>Please choose atleast one row to update</span>
                            </div>
                            <div class="modal-footer">
                                <button type="button" class="btn btn-primary" data-dismiss="modal">OK</button>
                            </div>
                        </div>
                    </div>
                </div>`);
        $("body").prepend($section);
    }
    $section.find('.message').html(message);
    $("#AlertDialog").modal('show');
}


/**
* set localstorage name/value pair
* in case value is object then stringify it first like using JSON.stringify()
* @param {string} name
* @param {string} value
*/
function setLocalStorage(name, value) {
    localStorage.setItem(name, value);
}

function getLocalStorage(name) {
    return localStorage.getItem(name);
}

function removeLocalStorage(name) {
    localStorage.removeItem(name);
}


function showConfirmationDialog(bodyMessage) {

    if (isNullOrEmpty(bodyMessage)) return;
    var $section = $("#confirmationDialog");
    if ($section.length == 0) {
        $section = $(`    <div class="modal" id="confirmationDialog" tabindex="-1" role="dialog" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                        <span aria-hidden="true">&times;</span>
                    </button>
                    <h4 class="modal-title">Confirmation</h4>                   
                </div>
                <div class="modal-body padding-0-10">
                    Confirmation ?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"  data-response="no">No</button>
                    <button type="button" class="btn btn-primary" data-response="yes">Yes</button>
                </div>
            </div>
        </div>
    </div>`);
        $("body").prepend($section);
    }
    $section.find('.modal-body').html(bodyMessage);
    $('#confirmationDialog').modal('show');
    $('#confirmationDialog').find('[data-response]').focus();

}
function hideConfirmationDialog() {
    $('#confirmationDialog').modal('hide');

}

function showProgressBar(bodyMessage) {
    var $section = $("#progressBar");
    if ($section.length == 0) {
        $section = $(`<div class="modal" id="progressBar" tabindex="-1" role="dialog" aria-hidden="true" style="top:40%">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content">                
                <div class="modal-body padding-0-10">
                    <br/>
                    <div class="progress pos-rel" data-percent="50%">
						<div class="progress-bar" style="width:50%;"></div>
					</div>
                </div>
            </div>
        </div>
    </div>`);
        $("body").prepend($section);
    }
    $section.find('.modal-body').html(bodyMessage);
    $('#progressBar').modal('show');
    //$('#confirmationDialog').find('[data-response]').focus();
}
function setProgressBar(_value) {
    $("#progressBar").find('.progress-bar').css('width', _value);
    $("#progressBar").find('.progress').attr('data-percent', _value);
}
function hideProgressBar() {
    $('#progressBar').modal('hide');

}

function bindStyleEvents() {
    $(".immediate-notify").on('mouseenter ', 'marquee', function () {
        this.stop();
    }).on('mouseleave ', 'marquee', function () {
        this.start();
    }).on('mouseenter ', 'marquee span', function () {
        return;
        $(".js-active-popover").removeClass("js-active-popover");
        var $span = $(this).addClass('js-active-popover');
        var $marquee = $span.closest('marquee');
        var $new_span = $('<span title="' + $span.data('title') + '" data-content="' + $span.data('content') + '" data-toggle="popover" data-placement="bottom"  id="data-toggle-ref"></span>')
        $marquee.after($new_span);
        $('[data-toggle="popover"]').popover();
        $new_span.trigger('click');
        setTimeout(function () {
            var $span = $(".js-active-popover");
            if ($span.position().left - ($span.width() / 2) < 0) { //left
                $(".popover").css('left', $span.closest('marquee').position().left + ($(".popover").width() / 2));
            } else if ($span.position().left + ($span.width() / 2) > $span.closest('marquee').width()) { //right
                $(".popover").css('left', $span.closest('marquee').width() - ($(".popover").width() / 2));
            } else {
                $(".popover").css('left', $span.position().left + ($span.width() / 2));
            }
        }, 50);
    }).on('mouseleave ', 'marquee span', function () {
        return;
        $("#data-toggle-ref").remove();
        var $marquee = $(this).closest('marquee');
        $marquee.siblings(".popover").remove();
    });
}


function isNullOrEmpty(data) {
    if (!data || data == null || data == "null" || data == undefined || data == "undefined" || String(data).trim().length == 0 || $.isEmptyObject(data)) {
        return true;
    }
    return false;
}

function getUserRoles() {
    var roles = $("#UserRoles").val();
    return JSON.parse(roles) || [];
}


//Cascading Dropdown List start

function getCourseDDL() {
    var _college_ID = $("#CollegeDDL").find('option:selected').val();
    var _printProgramme = $("#PrintProgrammeDDL option:selected").val();
    var _programme = $("#ProgrammeDDL option:selected").val();

    if (_programme === undefined || _programme.length == 0) { _programme = $("#ProgramDDL option:selected").val(); }

    $('.js-pager-table').data('otherparam1', _printProgramme);
    $('.js-exportToCSV').data('otherparam1', 'printProgramme=' + _printProgramme);

    var $targetSelect = $("#CourseDDL");
    clearDDOptions($targetSelect);
    //fillDDLDefaultOption($targetSelect);

    var _url = "/CUSrinagarAdminPanel/General/GetCourseList";
    $.ajax({
        url: _url,
        type: "POST",
        datatype: "Json",
        data: { College_ID: _college_ID, printProgramme: _printProgramme, programme: _programme },
        success: function (data) {
            fillDDLOptions($targetSelect, data);
            ChosenStyle();
            resizechosen($targetSelect);


            //fillDDLDefaultOption($targetSelect);
            //if (data != null) {
            //    $.each(data, function (index, item) {
            //        $targetSelect.append('<option value="' + item.Value + '">' + item.Text.trim() + '</option>');
            //    });
            //}
            //resizechosen($targetSelect);
            $targetSelect.trigger('change');
        },
        error: function (xhr, error, msg) {
            fillDDLOptions($targetSelect, null);
            showErrorMessage(msg);
        }
    });
}

function getSubjectDDL() {
    var $targetSelect = $("#SubjectDDL");
    clearDDOptions($targetSelect);
    fillDDLDefaultOption($targetSelect);

    var _param = new Parameter();
    _param.Filters = [];

    var printProgmme = $("#PrintProgrammeDDL").find('option:selected').val();
    if (!isNullOrEmpty(printProgmme))
        _param.Filters.push({ Column: "PrintProgramme", Operator: "EqualTo", Value: printProgmme, GroupOperation: "AND", TableAlias: "ADMCourseMaster" });

    var programme = $("#ProgramDDL").find('option:selected').val();
    if (!isNullOrEmpty(programme))
        _param.Filters.push({ Column: "Programme", Operator: "EqualTo", Value: programme, GroupOperation: "AND", TableAlias: "ADMCourseMaster" });

    var course_id = $("#CourseDDL").find('option:selected').val();
    if (!isNullOrEmpty(course_id))
        _param.Filters.push({ Column: "Course_ID", Operator: "EqualTo", Value: course_id, GroupOperation: "AND", TableAlias: "ADMSubjectMaster" });

    var semester = $("#SemesterDDL").find('option:selected').val();
    if (!isNullOrEmpty(semester))
        _param.Filters.push({ Column: "Semester", Operator: "EqualTo", Value: semester, GroupOperation: "AND", TableAlias: "ADMSubjectMaster" });

    _param.SortInfo = { ColumnName: "SubjectFullName" };
    _param.PageInfo = { PageNumber: -1, PageSize: -1, DefaultOrderByColumn: "SubjectFullName" };
    _param.SortInfo = { ColumnName: "", OrderBy: 1 };

    clearDDOptions($targetSelect);
    var _url = "/CUSrinagarAdminPanel/General/SubjectDDL";
    $.ajax({
        url: _url,
        type: "POST",
        datatype: "Json",
        data: _param,
        success: function (data) {
            fillDDLOptions($targetSelect, data);
            ChosenStyle();
            resizechosen($targetSelect);
        },
        error: function (xhr, error, msg) {
            fillDDLOptions($targetSelect, null);
            showErrorMessage(msg);
        }
    });
}

function getSubjectWithTypeDDL() {
    var $targetSelect = $("#SubjectDDL");
    clearDDOptions($targetSelect);
    fillDDLDefaultOption($targetSelect);

    var _param = new Parameter();
    _param.Filters = [];

    var printProgmme = $("#PrintProgrammeDDL").find('option:selected').val();
    if (!isNullOrEmpty(printProgmme))
        _param.Filters.push({ Column: "PrintProgramme", Operator: "EqualTo", Value: printProgmme, GroupOperation: "AND", TableAlias: "ADMCourseMaster" });

    var programme = $("#ProgramDDL").find('option:selected').val();
    if (!isNullOrEmpty(programme))
        _param.Filters.push({ Column: "Programme", Operator: "EqualTo", Value: programme, GroupOperation: "AND", TableAlias: "ADMCourseMaster" });

    var course_id = $("#CourseDDL").find('option:selected').val();
    if (!isNullOrEmpty(course_id))
        _param.Filters.push({ Column: "Course_ID", Operator: "EqualTo", Value: course_id, GroupOperation: "AND", TableAlias: "ADMSubjectMaster" });

    var semester = $("#SemesterDDL").find('option:selected').val();
    if (!isNullOrEmpty(semester))
        _param.Filters.push({ Column: "Semester", Operator: "EqualTo", Value: semester, GroupOperation: "AND", TableAlias: "ADMSubjectMaster" });

    _param.SortInfo = { ColumnName: "SubjectFullName" };
    _param.PageInfo = { PageNumber: -1, PageSize: -1, DefaultOrderByColumn: "SubjectFullName" };
    _param.SortInfo = { ColumnName: "", OrderBy: 1 };

    clearDDOptions($targetSelect);
    var _url = "/CUSrinagarAdminPanel/General/SubjectDDLWithDetail";
    $.ajax({
        url: _url,
        type: "POST",
        datatype: "Json",
        data: _param,
        success: function (data) {
            fillDDLOptions($targetSelect, data);
            ChosenStyle();
            resizechosen($targetSelect);
        },
        error: function (xhr, error, msg) {
            fillDDLOptions($targetSelect, null);
            showErrorMessage(msg);
        }
    });
}

//cascading Dropdown list end



function setActiveInActiveTabs() {

    try {
        var url = window.location.pathname + window.location.search;
        if (url.indexOf('Panel/') < 0 && url.indexOf('CUStudentZone/') < 0) return;
        var _url = `a[href*="${url}"]`;
        if (_url.length === 0) { _url = `a[href*="${window.location.pathname}"]`; }
        var $listToActive = $(_url);
        $(".nav.nav-list li").removeClass('open active');
        //$("li.ac").removeClass('open active');
        if ($listToActive.length > 0) {
            $listToActive.closest('li').addClass('active');
            if ($listToActive.closest('.submenu').closest('li').length > 0) {
                $listToActive.closest('.submenu').closest('li').addClass('active open');
                if ($listToActive.closest('.submenu').closest("li").closest('.submenu').length > 0)
                    $listToActive.closest('.submenu').closest("li").closest('.submenu').closest("li").find("a[href^='#']").click();
            }
        }

    } catch (err) {
        console.log(err);
    }
}

try {
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
                    if (item.category === undefined) {
                        ul.append("<li class='ui-autocomplete-category'>Other</li>");
                    } else {
                        ul.append("<li class='ui-autocomplete-category'>" + item.category + "</li>");
                    }
                    currentCategory = item.category;
                }
                li = that._renderItemData(ul, item);
                if (item.category) {
                    li.attr("aria-label", item.category + " : " + item.label);
                    li.empty().html(`<a style="color:black;" href="${item.Url}">${item.label}</a>`);
                }
            });

            $("#ui-id-1").css("max-width", "91%");
        }
    });
} catch (err) {
    console.log(err);
}


function QuickLaunch() {
    try {
        var list = [];
        var MenuItems = $(".nav li a:not([href^='#'])");
        if (MenuItems.length > 0) {
            var itemURL;
            var UrlSplit;
            MenuItems.each(function () {
                itemURL = $(this).attr('href');
                UrlSplit = itemURL.split('/');
                var obj = { category: UrlSplit[2], label: $.trim($(this).text()), Url: itemURL };
                list.push(obj);
            });

            $("#quickLaunch").catcomplete({
                delay: 0,
                source: list,
                autoFocus: true,
                select: function (event, ui) {
                    window.location.href = ui.item.Url;
                }
            });
        }
    } catch (err) { }
}



function getPrintProgrammeFromProgramme(programme) {
    //var _SubjectType = $("#SubjectType").val();
    programme = Object.keys(Programme).filter(function (key, index) { if (Programme[key] == programme) return key; })[0] || "";
    programme = Programme[programme];
    var printProgramme = PrintProgramme.UG;
    switch (programme) {
        case Programme.HS:
        case Programme.IG:
        case Programme.Professional:
        case Programme.Engineering:
            printProgramme = PrintProgramme.IH;
            break;
        case Programme.UG:
            printProgramme = PrintProgramme.UG;
            break;
        case Programme.PG:
            printProgramme = PrintProgramme.PG;
            break;
        default:
            printProgramme = PrintProgramme.UG;
            break;
    }
    return printProgramme;

}

function parseLocalDate(value) {
    var _array = value.split('-');
    var _year = _array[0];
    var _month = _array[1];
    var _day = _array[2].split('T')[0];
    var _hour = _array[2].split('T')[1].split(':')[0];
    var _minute = _array[2].split('T')[1].split(':')[1];
    var _datetime = _month + '/' + _day + '/' + _year + ' ' + _hour + ':' + _minute;
    return _datetime;
}

function SearchBySubmit(url) {
    var val = false;
    var $pagertable = $(".js-pager-table");
    $pagertable.find('.jsfilterelement').each(function () {
        var colName = $(this).data("column");
        if (colName == "CreatedOn") {
            colName = $(this).attr('id');
        }
        var value = $(this).val() || $(this).data('value');
        if (value && value != "") {
            url += colName + "=" + value + "&";
            if ($(this).hasClass("required")) {
                $("#" + colName + "Validator").html("");
            }
        }
        else {
            if ($(this).hasClass("required")) {
                isValidated = false;
                $("#" + colName + "Validator").html("Required");
            }
        }
        val = true;
    });
    if (val)
        window.open(url.substring(0, url.length - 1), '_blank');
}