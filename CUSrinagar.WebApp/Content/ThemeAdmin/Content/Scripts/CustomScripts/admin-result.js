$(document).ready(function () {

    if ($("#SaveEditSingleStudentResult").length > 0) {
        SaveEditSingleStudentResult();
    } else {
        loadGrid();

        $("#viewby-college").change(function () {
            var college = $("#viewby-college").val();
            var filter = { Column: "college", Operator: "eq", Data: college };
            fillGrid(filter);
        });
    }

});

function SaveEditSingleStudentResult() {

    $(".jsSetResultNotification").change(function () {
        var $tr = $(this).closest('tr');
        if ($(this).is(':checked')) {
            $tr.find('[name*=ResultNotification_ID]').val($tr.find('[name*=ResultSubjectNotification_ID]').val());
        } else {
            $tr.find('[name*=ResultNotification_ID]').val('');
        }
    });
    $(".jsSetExamForm_ID").change(function () {
        var $tr = $(this).closest('tr');
        if ($(this).is(':checked')) {
            $tr.find('[name*=ExamForm_ID]').val($tr.find('[name*=ResultSubjectExamFormID]').val());
        } else {
            $tr.find('[name*=ExamForm_ID]').val('');
        }
    });

    $(".jsSubjectResult").on('change', 'input', function () {
        var $tr = $(this).closest('tr');
        var $recordState = $tr.find('[name*=RecordState]');
        if ($tr.find('.js_ID').val() != emptyGuid()) {
            $recordState.val('Dirty');
        }

    });
}

function fillGrid(filter) {
    $(".jsSingleContent").hide();
    $(".jsTableContent").show();
    var customFilters = { groupOp: "AND", Filters: [] };
    customFilters.Filters.push(filter);
    var grid = $("#grid-table");
    grid[0].p.search = customFilters.Filters.length > 0;
    $.extend(grid[0].p.postData, { customFilters: JSON.stringify(customFilters) });
    grid.trigger("reloadGrid", [{ page: 1 }]);
}
function getStudentResult(type, value) {

    $(".jsTableContent").hide();
    $(".jsSingleContent").show();
    value = 'CUS-17-SPC-10001';
    $.ajax({
        url: '/Result/ResultPartial?type=' + type + '&searchvalue=' + value + '',
        type: 'Get',
        dataType: 'html',
        contentType: 'application/json; charset=UTF-8',
        success: function (html) {
            $(".jsSingleContent").html(html);
        }
    });
}

function loadGrid() {

    var grid_selector = "#grid-table";
    var pager_selector = "#grid-pager";
    var form = "";
    height: '100%';
    jQuery(function ($) {
        var popup = '#popup';
        if ($(popup).length == 0) {
            $('body').append("<div id='popup'></div>");
        }
        $('#popup').hide();
        //resize to fit page size
        $(window).on('resize.jqGrid', function () {

            setTimeout(function () {
                var shrinkToFit = $(window).width() > 800 ? true : false;
                $(grid_selector).setGridParam({ shrinkToFit: shrinkToFit });
                $(grid_selector).jqGrid('setGridWidth', $(".page-content").width());
            }, 150);
        })
        //resize on sidebar collapse/expand
        var parent_column = $(grid_selector).closest('[class*="col-"]');
        $(document).on('settings.ace.jqGrid', function (ev, event_name, collapsed) {
            if (event_name === 'sidebar_collapsed' || event_name === 'main_container_fixed') {
                $(grid_selector).jqGrid('setGridWidth', parent_column.width());
            }
        });
        var rowid = 0;
        var _filters = null;
        var _search = false;
        var _postData = {};
        if (_filters == null || _filters == '' || _filters == undefined) {
            _search = false;
        }
        else {
            _search = true;
            _postData = { filters: _filters };
        }
        jQuery(grid_selector).jqGrid({
            url: "/CUSrinagarAdminPanel/Result/List",
            datatype: 'json',
            mtype: 'GET',
            colNames: ['Name', 'Father Name', 'Registration No', 'Roll No', 'Action'],
            colModel: [
                { name: 'Name' },
                { name: 'FathersName' },
                { name: 'RegistrationNumber' },
                { name: 'RollNumber' },
                { name: 'Actions', index: 'id', align: 'center', width: 80, sortable: false, search: false, editable: false, viewable: true }
            ],
            viewrecords: true,
            rowNum: 15,
            rowList: [15, 25, 50, 100, 150],
            pager: pager_selector,
            altRows: true,
            multiselect: false,
            gridview: false,
            postData: _postData,
            search: _search,

            afterInsertRow: function (id, currentData, jsondata) {
                //var button = "";
                // button += " <a class='ace-icon fa fa-edit' title='Edit' href='/CUSrinagarAdminPanel/Syllabus/Edit?id=" + jsondata.SyllabusId + "'></a>";
                // $(this).setCell(id, "Actions", button);
            },
            toppager: false,
            multiselect: false,
            //multikey: "ctrlKey",
            multiboxonly: false,
            sortname: 'Name',
            loadComplete: function () {
                var table = this;
                //  var totalrows = $("#grid-table").getGridParam("reccount");
                //if (totalrows <= 0)
                //{$("#exportToExcelButton").hide(); }
                //else
                //{$("#exportToExcelButton").show(); }
                setTimeout(function () {
                    updatePagerIcons(table);
                    enableTooltips(table);

                }, 0);


            },
            caption: "<i class='menu-icon fa fa-list'></i> Syllabus List",
            subGrid: false,

            onSelectRow: function (id) {
                rowid = id;
            },

            //Setting height 100%, so no vertical scroll-bar
            height: '100%',
            width: $(".page-content").width(),
            shrinkToFit: $(window).width() > 800 ? true : false,
        });
        var rowId = 0;


        $(window).triggerHandler('resize.jqGrid');//trigger window resize to make the grid get the correct size
        jQuery(grid_selector).jqGrid('navGrid', pager_selector,
            { 	//navbar options
                edit: false,
                add: false,
                del: false,
                search: true,
                searchicon: 'ace-icon fa fa-search blue',
                refresh: false,
                view: false,
                cloneToTop: true,
            },
            {},
            {},
            {},
            {
                url: "/CUSrinagarAdminPanel/Result/List",
                caption: "Result Search",
                recreateForm: true,
                afterShowSearch: function (e) {
                    form = $(e[0]);
                    form.closest('.ui-jqdialog').find('.ui-jqdialog-title').wrap('<div class="widget-header" />')
                    style_search_form(form);

                },
                afterRedraw: function () {
                    style_search_filters($(this));
                },
                onSearch: function () {
                    var grid = $(grid_selector);
                    if (grid != null) {
                        parseJQGridPostData(grid[0]);
                    }
                },
                ignoreCase: true,
                multipleSearch: true,
                multipleGroup: false,
                /**
                showQuery: false
                 */
            },
            {}
        );



        function style_search_filters(form) {
            form.find('.delete-rule').val('X');
            form.find('.add-rule').addClass('btn btn-xs btn-primary');
            form.find('.add-group').addClass('btn btn-xs btn-success');
            form.find('.delete-group').addClass('btn btn-xs btn-danger');
        }
        function style_search_form(form) {
            dialog = form.closest('.ui-jqdialog');
            var buttons = dialog.find('.EditTable')
            buttons.find('.EditButton a[id*="_reset"]').addClass('btn btn-sm btn-info').find('.ui-icon').attr('class', 'ace-icon fa fa-retweet');
            buttons.find('.EditButton a[id*="_query"]').addClass('btn btn-sm btn-inverse').find('.ui-icon').attr('class', 'ace-icon fa fa-comment-o');
            buttons.find('.EditButton a[id*="_search"]').addClass('btn btn-sm btn-purple').find('.ui-icon').attr('class', 'ace-icon fa fa-search');
        }


        function updatePagerIcons(table) {
            var replacement =
                {
                    'ui-icon-seek-first': 'ace-icon fa fa-angle-double-left bigger-140',
                    'ui-icon-seek-prev': 'ace-icon fa fa-angle-left bigger-140',
                    'ui-icon-seek-next': 'ace-icon fa fa-angle-right bigger-140',
                    'ui-icon-seek-end': 'ace-icon fa fa-angle-double-right bigger-140'
                };
            $('.ui-pg-table:not(.navtable) > tbody > tr > .ui-pg-button > .ui-icon').each(function () {
                var icon = $(this);
                var $class = $.trim(icon.attr('class').replace('ui-icon', ''));

                if ($class in replacement) icon.attr('class', 'ui-icon ' + replacement[$class]);
            })
        }
        function enableTooltips(table) {
            $('.navtable .ui-pg-button').tooltip({ container: 'body' });
            $(table).find('.ui-pg-div').tooltip({ container: 'body' });
        }

    });




    jQuery(function ($) {

        //Hiding search icon from grid bottom
        $('#search_grid-table').css({ 'visibility': 'hidden' });

    });

}



