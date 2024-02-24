var grid_selector = "#grid-table";
var pager_selector = "#grid-pager";
var form = "";

CreateNotificationJQGrid();

function CreateNotificationJQGrid() {

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


        jQuery(grid_selector).jqGrid({
            url: NotificationPath,
            datatype: 'json',
            mtype: 'POST',
            autoencode:true,
            colNames: ['NotificationId', 'Title', 'Description', 'Start Date', 'End Date', 'Full Description', 'Link', ''],
            colModel: [

                { name: 'Notification_ID', hidden: true, index: 'id', sorttype: "int", key: true },

                {
                    name: 'Title', index: 'Title'

                },
                {
                    name: 'Description', index: 'Description', hidden: true

                },
                {
                    name: 'StartDate', index: 'StartDate', formatter: 'date', formatoptions: { /*srcformat: 'm/d/Y',*/ newformat: 'd/m/Y' }

                },
                {
                    name: 'EndDate', index: 'EndDate', formatter: 'date', formatoptions: { /*srcformat: 'm/d/Y',*/ newformat: 'd/m/Y' }

                },
                {
                    name: 'FullDescription', index: 'FullDescription',
                    formatter: function DescrFormatter(cellvalue, options, rowobj) {
                         if (rowobj.FullDescription != null && rowobj.FullDescription.length >250) {
                            return rowobj.FullDescription.substring(0,250); 
                        }
                        return  rowobj.FullDescription; 
                    }

                },
                {
                    name: 'Link', index: 'Link',
                    formatter: function LinkFormatter(cellvalue, options, rowobj) {
                        if (rowobj.Link!=null)
                        {
                            return "<a download title='Download' href='" + rowobj.Link + "'> Download </a>";
                        }
                        return "";
                    }


                },

                { name: 'Actions', hidden: true, index: 'id', align: 'center', width: 80, sortable: false, search: false, editable: false, viewable: true },

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
                var button = "";
                button += " <a class='ace-icon fa fa-edit' title='Edit' href='/CUSrinagarAdminPanel/Notification/Edit?id=" + jsondata.Notification_ID + "'></a>";
                $(this).setCell(id, "Actions", button);
            },
            toppager: false,
            multiselect: false,
            //multikey: "ctrlKey",
            multiboxonly: false,
            sortname: 'Notification_ID',
            loadComplete: function () {
                var table = this;
                setTimeout(function () {
                    updatePagerIcons(table);
                    enableTooltips(table);

                }, 0);


            },
            caption: "<i class='menu-icon fa fa-list'></i> Notification List",
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
                url: NotificationPath,
                caption: "Notification Search",
                recreateForm: true,
                afterShowSearch: function (e) {
                    form = $(e[0]);
                    form.closest('.ui-jqdialog').find('.ui-jqdialog-title').wrap('<div class="widget-header" />')
                    style_search_form(form);

                },
                afterRedraw: function () {
                    style_search_filters($(this));
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
    });
}

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






jQuery(function ($) {


    //Hiding search icon from grid bottom
    $('#search_grid-table').css({ 'visibility': 'hidden' });

});
