
function Parameter() {
    this.Filters = [];
    this.PageInfo = { PageNumber: 1, PageSize: 15, DefaultOrderByColumn: "" };
    this.SortInfo = { ColumnName: "", OrderBy: 1 };
}

$(document).ready(function () {

    var $pagetable = $(".js-pager-table");
    bindPageTableEvents($pagetable);
    bindSearchevents($pagetable);
    bindPagerTableSaveAndValidationEvents();
    try {
        ChosenStyle();
    }
    catch (ex)
    { }
});

function rapidEntryRowAfterSuccess($tr) {
    $tr.removeClass('under-process jsDirty isCurrentRow');
    $tr.next('tr.jsCloneRow').remove();
    $tr.next('tr').find('.jsfocus').focus().select();

}//end row done function
function rapidEntryRowAfterError($tr) {
    if ($tr.next('tr.jsCloneRow').length > 0) {
        $tr.next('tr.jsCloneRow').removeClass('hidden jsCloneRow');
        $tr.remove();
    }
}//end row done function

function bindPagerTableSaveAndValidationEvents() {
    var $rapidEntrypagertables = $(".js-rapid-entry");
    if ($rapidEntrypagertables.length > 0) {
        $rapidEntrypagertables.each(function (index, pagetb) {
            var $rapidEntrypagertable = $(pagetb);

            //sets a row dirty if anything changes
            //since sometimes savefunction is called before change event .... so we are adding 
            //change event on keypress ....
            $rapidEntrypagertable.on('change', 'tr input', function (e) {
                var $tr = $(this).closest('tr');
                $tr.addClass('jsDirty');
                $tr.find('[name*=RecordState]').val('Dirty');
                return true;
            });

            //removes hidden row if no action is taken for that row
            $rapidEntrypagertable.on('focusout', 'tr input', function (e) {
                var $tr = $(this).closest('tr');
                if (!$tr.hasClass('under-process') && !$tr.hasClass('isCurrentRow')) {
                    if ($tr.next('tr.jsCloneRow').length > 0) {
                        $tr.next().removeClass('hidden jsCloneRow');
                        $tr.remove();
                    }
                }
                return true;
            });


            //creates a hidden row before anything changes
            $rapidEntrypagertable.on('focusin', 'tr input', function (e) {
                var $tr = $(this).closest('tr');
                $tr.addClass('jsDirty');
                $tr.find('[name*=RecordState]').val('Dirty');

                $tr.siblings(':not(.hidden)').find('.jsRowAction').hide();
                $tr.find('.jsRowAction').show();
                if ($tr.siblings('.under-process').length == 0 && !$tr.hasClass('isCurrentRow') && $tr.next('.jsCloneRow').length == 0) {
                    if ($tr.siblings('tr.jsCloneRow').length > 0) {
                        $tr.siblings('tr.jsCloneRow').removeClass('hidden jsCloneRow').prev().remove();
                    }
                }
                if ($tr.next('.jsCloneRow').length > 0) return true;
                var $cloneRow = $tr.clone(true);
                $cloneRow.addClass('hidden jsCloneRow').find('.jsRowAction').hide();
                $cloneRow.addClass('hidden jsCloneRow');
                $tr.siblings('tr.isCurrentRow').removeClass('isCurrentRow');
                $tr.addClass('isCurrentRow').after($cloneRow);
                return true;
            });



            //removes un comitted row
            $rapidEntrypagertable.on('click keypress', '.js-cancel-row', function (e) {
                if (e.type == "click" || e.which == "13") {
                    var $tr = $(this).closest('tr');
                    if (!$tr.hasClass('under-process') && $tr.next('tr.jsCloneRow').length > 0) {
                        $tr.next().removeClass('hidden jsCloneRow');
                        $tr.remove();
                    }
                }
            });

            $rapidEntrypagertable.on('click keypress', '.jsDeleteRow', function (e) {
                if (e.type == "click" || e.which == "13") {
                    var $tr = $(this).closest('tr');
                    showConfirmationDialog("Are you sure, you want to delete the record ? ");
                    $('#confirmationDialog').off('click.confirmation', 'button').on('click.confirmation', 'button', function () {
                        var $btn = $(this);
                        hideConfirmationDialog();
                        if ($btn.data('response') == 'yes') {
                            deleteRow($tr);
                        } else if ($btn.data('response') == 'no') {
                            ///
                        } else {
                            ///
                        }
                    });
                }
            });

            //sets a row is under process i.e. we r working actively on that row
            $rapidEntrypagertables.on('click keypress', 'tr', function (e) {
                if ($(e.target).hasClass('js-save-row') || e.which == "13") {
                    var $tr = $(this).closest('tr');
                    if (validateRow($tr) && !$tr.hasClass('under-process')) {
                        $tr.addClass('under-process');
                        showLoader();
                        let promise = new Promise((resolve, reject) => {
                            saveRapidEntryRow($tr, resolve, reject);
                        });
                        promise.then(
                            result => rapidEntryRowAfterSuccess($tr)
                        ).catch(
                            result => rapidEntryRowAfterError($tr)
                            ).finally(result => hideLoader());
                    }
                }
            });

        });
    }
}

function loadpagertable($pagertable) {
    if (!$pagertable)
        $pagertable = $('.js-pager-table').first();

    var _url = $pagertable.data('url');
    if (!_url) {
        _url = $pagertable.find(".js-table-content").data('url');
        $pagertable.data('url', _url);
    }
    var _otherObject = $pagertable.data('otherobject');
    if (!_otherObject) {
        _otherObject = {}
        $pagertable.data('otherobject', _otherObject);
    }

    getpagertable($pagertable);
}
function loadpagertableWithDefaultParams($pagertable) {
    if (!$pagertable)
        $pagertable = $('.js-pager-table').first();
    if (!fillPagerTableParams($pagertable)) return;

    var _url = $pagertable.data('url');
    if (!_url) {
        _url = $pagertable.find(".js-table-content").data('url');
        $pagertable.data('url', _url);
    }
    //var _otherObject = $pagertable.data('otherobject');
    //if (!_otherObject) {
    //    _otherObject = {}
    //    $pagertable.data('otherobject', _otherObject);
    //}

    getpagertable($pagertable);
}

function renderNewRows($pagertable, $table) {
    var _param = getpagertableparameter($pagertable);
    var currentpage = _param.PageInfo.PageNumber;
    var previousPage = currentpage - 1;
    var $newRows = $table.find('tbody tr');
    if ($newRows.length > 0) {
        $pagertable.find("tr[data-pagegroup=" + previousPage + "]").addClass('hidden');
        $pagertable.find('tr:last').after($newRows);
        scrollPager($pagertable);
        enabledisablepreviousandnextbutton($pagertable);
    } else {
        $pagertable.find('.jsNext').addClass('disabled');
        $pagertable.data('allrecordsfetch', true);
        _param.PageInfo.PageNumber -= 1;
    }
}

function enabledisablepreviousandnextbutton($pagertable) {
    var _param = getpagertableparameter($pagertable);
    var _PageSize = $pagertable.find(".js-pager-table-size option:selected").val() || _param.PageInfo.PageSize;
    var $visibleRows = $pagertable.find('tbody tr:not(.hidden)');
    if (_PageSize == "-1" || $visibleRows.length < parseInt(_PageSize)) {
        $pagertable.find('.jsNext').addClass('disabled');
        $pagertable.data('allrecordsfetch', true);//client sort
    }
    else {
        $pagertable.find('.jsNext').removeClass('disabled');
    }
    (parseInt(_param.PageInfo.PageNumber) || 0) > 1 ? $pagertable.find('.jsPrev').removeClass('disabled') : $pagertable.find('.jsPrev').addClass('disabled');

    //if (_param.PageInfo.PageNumber != 1) {
    //    $pagertable.find('.jsPrev').removeClass('disabled');
    //}        
    //else {
    //    $pagertable.find('.jsPrev').addClass('disabled');
    //} 
    ((parseInt(_param.PageInfo.PageNumber) || 0) > 1 || $pagertable.find('tbody tr').length > 0) ? $(".jsPageNumber").html("Page " + _param.PageInfo.PageNumber) : $(".jsPageNumber").html("Page 0");
}

function getpagertable($pagertable, callback) {
    $.ajax({
        url: $pagertable.data('url'),
        type: 'POST',
        data: jQuery.param({ parameter: getpagertableparameter($pagertable), otherParam1: getOtherParam($pagertable, "otherparam1") }),
        contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
        success: function (html) {
            $('.jsNext').removeClass("jsdisable");
            var $table = setpaginginfo($pagertable, $(html)); //parse table info after rendering ... will seem fast to user
            if (callback == 'renderNewRows') {
                window['renderNewRows']($pagertable, $table);
                afterAppendingRows($pagertable);
                return;
            }
            $pagertable.find('.js-table-content').html($table);
            enabledisablepreviousandnextbutton($pagertable);
            afterAppendingRows($pagertable);

            setSortInfo($pagertable, $table);
            //var _aftergridload = $pagertable.find('[data-aftergridload]').attr('data-aftergridload');
            //if (_aftergridload != null && _aftergridload != undefined && _aftergridload.length > 0) {
            //    window[_aftergridload]($pagertable);
            //}
        },
        error: function (xhr, error, message) {
            console.log(error + ': ' + message);
        },
        beforeSend: function () { showLoader(); $(".js-search,.fa-search-btn").prop("disabled", true); },
        complete: function () {
            hideLoader();
            $(".js-search,.fa-search-btn").prop("disabled", false);
            //try { ShowORHideSubmit(); }
            //catch (e){ }
        }
    });
}

function afterAppendingRows($pagertable) {
    var _afterAppendingRows = $pagertable.find('[data-afterappendingrows]').attr('data-afterappendingrows');
    if (_afterAppendingRows != null && _afterAppendingRows != undefined && _afterAppendingRows.length > 0) {
        window[_afterAppendingRows]($pagertable);
    }
}

function setpaginginfo($pagertable, $table) {
    var _param = getpagertableparameter($pagertable);
    var PageInfo = _param.PageInfo;
    var $trs = $table.find('tbody tr');
    $trs.attr('data-pagegroup', PageInfo.PageNumber);
    if ($trs.find('.js-sno').length > 0) {
        $trs.find('.js-sno').each(function (index, td) {
            var sno = ((PageInfo.PageNumber - 1) * PageInfo.PageSize) + (index + 1);
            $(td).html(sno);
        });
    }
    return $table;
}

function setSortInfo($pagertable, $table) {
    var _param = getpagertableparameter($pagertable);
    var PageInfo = _param.PageInfo;
    var $tds = $table.find('thead [data-property]');
    $tds.addClass('sort');
    var column = '', sorttype = '';
    if ($tds.length > 0) {
        if (!isNullOrEmpty(_param.SortInfo) && _param.SortInfo.ColumnName.length > 0) {
            column = _param.SortInfo.ColumnName;
            sorttype = _param.SortInfo.OrderBy;
        } else if (!isNullOrEmpty(_param.PageInfo) && _param.PageInfo.DefaultOrderByColumn.length > 0) {
            column = _param.PageInfo.DefaultOrderByColumn;
            sorttype = 0;

        }
        if (sorttype == 0) {
            $tds.siblings(`[data-property=${column}]`).addClass('asc').removeClass('sort');
        } else if (sorttype == 1) {
            $tds.siblings(`[data-property=${column}]`).addClass('desc').removeClass('sort');
        }

    }
}
function bindPageTableEvents($pagetable) {

    $($pagetable).each(function (index, pagetb) {
        var $pagetb = $(pagetb);

        $pagetb.on('click', '.jsPrev', function (e) {
            e.preventDefault();
            if ($(this).hasClass('disabled')) { e.preventDefault(); return; }
            var $pagertable = $(this).closest('.js-pager-table');
            getpagertablepreviouspage($pagertable);
        });

        $pagetb.on('click', '.jsNext', function (e) {
            e.preventDefault();
            if ($(this).hasClass('disabled') || $(this).hasClass("jsdisable")) { e.preventDefault(); return; }
            $(this).addClass("jsdisable");
            var $pagertable = $(this).closest('.js-pager-table');
            getpagertablenextpage($pagertable);
        });

        $pagetb.on('change', '.js-pager-table-size', function () {
            var _PageSize = $(this).val();
            var $pagertable = $(this).closest('.js-pager-table');
            var _param = getpagertableparameter($pagertable);

            _param.PageInfo.PageSize = _PageSize || 15;
            _param.PageInfo.PageNumber = 1;
            getpagertable($pagertable);
        });

        $pagetb.on('click', 'th[data-property]', function () {
            var $th = $(this);
            var _field = $th.attr('data-property');
            var typeis = $th.attr('data-isalphanumeric');
            var $pagertable = $th.closest('.js-pager-table');
            var _param = getpagertableparameter($pagertable);

            if (_param.SortInfo.ColumnName == _field) {
                _param.SortInfo.OrderBy == 1 ? _param.SortInfo.OrderBy = 0 : _param.SortInfo.OrderBy = 1;
            } else {
                _param.SortInfo.IsAlphaNumeric = typeis;
                _param.SortInfo.ColumnName = _field;
                _param.SortInfo.OrderBy == 1;
            }
            _param.PageInfo.PageNumber = 1;
            getpagertable($pagertable);
        });

        $pagetb.on('click', '.js-search,.fa-search-btn', function () {
            var $pagertable = $(this).closest('.js-pager-table');
            filtergrid($pagertable);
        });
        $pagetb.on('keypress', '.js-search,.fa-search-btn', function () {
            if (event.which == 13) {
                $(".js-search").trigger('click');
            }
        });
        $pagetb.on('keypress', '.jsfilterelement', function () {
            if (event.which == 13) {
                $(".js-search").trigger('click');
            }
        });
        $pagetb.on('click', '.jsSelectAllChk', function () {
            var $checkbox = $(this);
            var $table = $checkbox.closest('table');
            if ($checkbox.is(':checked')) {
                $table.find('tbody tr:not(.hidden) .jsSelectChk').prop('checked', 'checked').attr('checked', 'checked').trigger('change');
            } else {
                $table.find('tbody tr:not(.hidden) .jsSelectChk').removeProp('checked').removeAttr('checked').trigger('change');
            }
            return true;
        });

        $pagetb.on('change', 'select.jsChangeFilterColumn', function () {
            var $select = $(this);
            var selectValue = $select.find('option:selected').val();
            var $filter = $select.closest('label').siblings('.jsfilterelement');
            $filter.data('data-column', selectValue);
            $filter.data('column', selectValue);
            $filter.attr('placeholder', selectValue);
        });



    });

    $('[data-action="table-filter"]').each(function (index, element) {
        $(element).on('keyup', function (e) {
            $('.filterTable_no_results').remove();
            var element = $(this);
            var search = element.val().toLowerCase();
            var target = element.attr('data-filters');
            target = $(target);
            var rows = target.find('tbody tr');
            if (search == '') {
                rows.show();
            } else {
                rows.each(function () {
                    var $this = $(this);
                    $this.text().toLowerCase().indexOf(search) === -1 ? $this.hide() : $this.show();
                })
                if (target.find('tbody tr:visible').length === 0) {
                    var col_count = target.find('tr:eq(2)').find('td').length;
                    var no_results = $('<tr class="filterTable_no_results"><td colspan="' + col_count + '">No results found</td></tr>');
                    target.find('tbody').append(no_results);
                }
            }
        });
    });
    $(".js-pager-table").on('click', '.panel-heading span.filter-btn', function (e) {
        var $this = $(this),
            $panel = $this.parents('.panel');
        $panel.find('.panel-body').slideToggle();
        if ($this.css('display') != 'none') {
            $panel.find('.panel-body input').first().focus();
        }

    });
    $('[data-toggle="tooltip"]').tooltip();



    $('.js-exportToCSV').click(function () {
        var $element = $(this);
        var $pagertable = $element.closest('.js-pager-table');
        if (!fillPagerTableParams($pagertable)) return;

        var _param = getpagertableparameter($pagertable);

        var pageParam = JSON.parse(JSON.stringify(_param));
        //pageParam.Filters = [];
        //pageParam.PageInfo = { PageNumber: -1, PageSize: -1, DefaultOrderByColumn: $pagertable.find('[data-DefaultOrderByColumn]').attr('data-DefaultOrderByColumn') };
        pageParam.PageInfo.PageNumber = pageParam.PageInfo.PageSize = -1;
        //_param.SortInfo = _param.SortInfo;
        //var isValidated = true;
        //$pagertable.find('.jsfilterelement').each(function () {
        //    var colName = $(this).data("column");
        //    var operator = $(this).data("operator");
        //    var _tableAlias = $(this).data('tablealias') || '';
        //    var _IsSibbling = $(this).data("issibbling") || false;
        //    var _groupOperation = $(this).data("groupoperation") || "AND";
        //    var value = $(this).val();
        //    if (value && value != "") {
        //        pageParam.Filters.push({ Column: colName, Operator: operator, Value: value.trim(), GroupOperation: _groupOperation, IsSibling: _IsSibbling, TableAlias: _tableAlias });
        //        if ($(this).hasClass("required")) {
        //            $("#" + colName + "Validator").html("");
        //        }
        //    }
        //    else {
        //        if ($(this).hasClass("required")) {
        //            isValidated = false;
        //            $("#" + colName + "Validator").html("Required");
        //        }
        //    }
        //});
        //if (!isValidated)
        //{ return false; }
        //eport to excel
        var _url = $element.data('url');
        if (_url.length == 0) {
            showErrorMessage("Missing action url");
            return;
        }
        var _otherparam = $element.data('otherparam1');
        if (!isNullOrEmpty(_otherparam) && _otherparam.length > 0) {
            _url += "?" + _otherparam;
        }
        var _otherparam2 = $element.data('otherparam2');
        if (!isNullOrEmpty(_otherparam2) && _otherparam2.length > 0) {
            //var _o_p = _otherparam2.split(';');
            //for (var i = 0; i < _o_p.length; i++) {

            //}
            //_o_p.each(function () { });
            _url += "&" + _otherparam2;
        }

        var $customform = createFormFromModel(pageParam, _url);
        var otherParams = getOtherParam($pagertable, "otherparam1");
        if (!isNullOrEmpty(otherParams) && otherParams.length > 0) {
            createFormFromModel({ otherParam1: JSON.stringify(otherParams) }, null, $customform);
        }
        var $token = $('[name*=RequestVerificationToken]');
        $customform.append($token);
        $('body').append($customform);
        $customform.submit();
        $customform.remove();
    });
}

function getpagertablenextpage($pagertable) {
    var _param = getpagertableparameter($pagertable);
    var _currentpage = _param.PageInfo.PageNumber;
    var _nextPage = _currentpage + 1;
    var $tbody = $pagertable.find('table tbody');
    var $nextrows = $tbody.find("tr[data-pagegroup=" + _nextPage + "]");
    _param.PageInfo.PageNumber += 1;
    if ($nextrows.length > 0) {
        scrollPager($pagertable);
        $tbody.find("tr[data-pagegroup=" + _currentpage + "]").addClass('hidden');
        $nextrows.removeClass('hidden');
        enabledisablepreviousandnextbutton($pagertable);
        $('.jsNext').removeClass("jsdisable");

    } else {
        getpagertable($pagertable, 'renderNewRows');

    }
    SetPageSize(_param.PageInfo.PageNumber);
}
function SetPageSize(pageNumber) {
    $(".jsPageNumber").html("Page " + pageNumber);
}
function getpagertablepreviouspage($pagertable) {
    var _param = getpagertableparameter($pagertable);
    var _currentpage = _param.PageInfo.PageNumber;
    var _previousPage = _currentpage - 1;
    var $tbody = $pagertable.find('table tbody');
    var $previousrows = $tbody.find("tr[data-pagegroup=" + _previousPage + "]");
    if ($previousrows.length > 0) {
        scrollPager($pagertable);
        $tbody.find("tr[data-pagegroup=" + _currentpage + "]").addClass('hidden');
        $previousrows.removeClass('hidden');
    }
    _param.PageInfo.PageNumber -= 1;
    if (_param.PageInfo.PageNumber == 1)
        $pagertable.find('.jsPrev').addClass('disabled');
    $pagertable.find('.jsNext').removeClass('disabled');
    SetPageSize(_param.PageInfo.PageNumber);

}

function getpagertableparameter($pagertable, resetParam) {
    var _param = $pagertable.data('parameter');
    if (!_param || resetParam == true) {
        _param = new Parameter();
        _param.PageInfo.PageSize = $pagertable.find(".js-pager-table-size option:selected").val() || _param.PageInfo.PageSize;
        $pagertable.data('parameter', _param);
    }
    if (!_param.PageInfo.DefaultOrderByColumn || _param.PageInfo.DefaultOrderByColumn == "")
        _param.PageInfo.DefaultOrderByColumn = $pagertable.find('[data-DefaultOrderByColumn]').attr('data-DefaultOrderByColumn');
    return _param;
}
function getOtherParam($pagertable, paramName) {
    var _otherparam = $pagertable.data(paramName);
    return _otherparam;
}

function pagertableclientsort($th) {
    //pager table client sort will be implemented on a table where we have all rows rendered ....
    var $pagertable = $th.closest('.js-pager-table');
    var _param = getpagertableparameter($pagertable);// $pagertable.data('parameter');
    var $table = $th.closest('table');
    var $sortedtable = $('<table><tr class="temprow"></tr></table>');
    var $tds = $($table.find('tbody td').filter(function (i, td) { return $(td).index() == $th.index() })).addClass('jsdirty');
    var complexity = $tds.length * $tds.length, complexitycount = 0;
    while ($tds.hasClass('jsdirty')) {
        var bubbleRowIndex = 0;
        $tds.each(function (index, element) { if ($(element).hasClass('jsdirty')) { bubbleRowIndex = index; return false; } });
        //for (var i = 1; i < $tds.siblings('.jsdirty').andSelf().length; i++) {
        for (var i = 1; i < $tds.length; i++) {
            if ($($tds[bubbleRowIndex]).html().toLowerCase() < $($tds[i]).html().toLowerCase() && _param.SortInfo.OrderBy == 1 && $($tds[i]).hasClass('jsdirty')) {
                bubbleRowIndex = i;
            } else if ($($tds[bubbleRowIndex]).html().toLowerCase() > $($tds[i]).html().toLowerCase() && _param.SortInfo.OrderBy == 0 && $($tds[i]).hasClass('jsdirty')) {
                bubbleRowIndex = i;
            }
        }
        $sortedtable.find('tr:last').append($($tds[bubbleRowIndex]).removeClass('jsdirty').closest('tr').clone());
        if (++complexitycount > complexity) {
            return false;
        }
    }
    console.log(complexitycount);
    $table.find('tbody').html($sortedtable.find('tr:not(.temprow)'));

    //reset page group info not handled ...
    resetPageGroupInfo($pagertable);
}

function bindSearchevents($pagetable) {
    //$(".fa-search-btn").click(function () {
    //    filtergrid($pagetable);
    //});

}
function filtergrid($pagetable) {
    if (!fillPagerTableParams($pagetable)) return;
    loadpagertable($pagetable);
    $('.js-pager-table-size option:selected').prop("selected", false); // remove the selected Item
}

function scrollPager($pagertable) {
    var pagerTopPosition = $pagertable.offset().top;
    $('html, body').animate({ scrollTop: (pagerTopPosition) }, '2000');
}


function resetPageGroupInfo($pagertable) {
    var _param = getpagertableparameter($pagertable);
    var $trs = $pagertable.find('tbody tr')
    var pagegroup = 1;
    var _pageGroupSize = parseFloat(_param.PageInfo.PageSize);
    $trs.each(function (index, tr) {
        ++index;
        $(tr).attr('data-pagegroup', pagegroup);
        if ((index % _pageGroupSize) == 0 && index > 0)
            pagegroup++;
    });
    $trs.addClass('hidden').find(':lt(15)').removeClass('hidden');
    $pagertable.find('tbody tr[data-pagegroup=1]').removeClass('hidden');
    _param.PageInfo.pageNumber = 1;
    enabledisablepreviousandnextbutton($pagertable);
    SetPageSize(_param.PageInfo.PageNumber);
}


function createFormFromModel(object, url, $form) {
    if (isNullOrEmpty($form) || $form.length == 0) {
        $('form.jsTempPostForm').remove();
        $form = $('<form class="jsTempPostForm"></form>');
        $form.attr('method', 'post').attr('action', url);
    }
    for (var property in object) {
        if (object.hasOwnProperty(property)) {
            var propertyValueTop = object[property];
            if (propertyValueTop != null && typeof (propertyValueTop) == "object") {
                if (propertyValueTop.length > 0) {
                    for (var i = 0; i < propertyValueTop.length; i++) {
                        for (var propertySub in propertyValueTop[i]) {
                            if (propertyValueTop[i].hasOwnProperty(propertySub)) {
                                var propertyValue = propertyValueTop[i][propertySub];
                                var inputname = property + "[" + i + "]" + "." + propertySub;
                                $form.append("<input type=hidden name='" + inputname + "' value='" + propertyValue + "' />");
                            }
                        }
                    }
                } else {
                    for (var propertySub in propertyValueTop) {
                        if (propertyValueTop.hasOwnProperty(propertySub)) {
                            var propertyValue = propertyValueTop[propertySub];
                            var inputname = property + "." + propertySub;
                            $form.append("<input type=hidden name='" + inputname + "' value='" + propertyValue + "' />");
                        }
                    }
                }
            }
            else {
                $form.append("<input type=hidden name='" + property + "' value='" + propertyValueTop + "' />");
            }
        }
    }
    return $form;
}



function fillPagerTableParams($pagertable) {
    if (!$pagertable)
        $pagertable = $('.js-pager-table').first();

    var _param = getpagertableparameter($pagertable);
    _param.Filters = [];
    if (_param.PageInfo === null || isNullOrEmpty(_param.PageInfo.DefaultOrderByColumn))
        _param.PageInfo = { PageNumber: 1, PageSize: 15, DefaultOrderByColumn: $pagertable.find('[data-DefaultOrderByColumn]').attr('data-DefaultOrderByColumn') };
    if (_param.SortInfo === null || isNullOrEmpty(_param.SortInfo.ColumnName))
        _param.SortInfo = { ColumnName: "", OrderBy: 1 };

    var isValidated = true;
    $pagertable.find('.jsfilterelement').each(function (index, element) {
        var $element = $(this);
        var colNames = $element.data("column");
        var operator = $element.data("operator");
        var _GroupOperation = $element.data('groupoperation') || 'AND';
        var _tableAlias = $element.data('tablealias') || '';
        var _IsSibling = $element.data("issibling") || false;
        var value = "";
        if ($element.is('select')) {
            if ($element.hasClass('multiselect')) {
                var _ids = "";
                $element.find('option:selected').each(function (index, option) {
                    if ($(option).val().trim().length > 0) {
                        _ids += "" + $(option).val().trim() + ",";
                    }
                });
                value = _ids.slice(0, -1)
            } else {
                value = $element.find('option:selected').val();
            }

        } else if ($element.is('checkbox')) {
            value = $element.is('checked');
        } else if ($element.is('radio')) {
            value = $element.val();
        } else {
            value = $element.val();
        }
        if (value && value != "") {
            var columns = colNames.split(",");
            for (var i = 0; i < columns.length; i++) {
                // _param.Filters.push({ Column: columns[i], Operator: operator, Value: value.trim(), GroupOperation: _GroupOperation, IsSibling: _IsSibling, TableAlias: _tableAlias });
                var filter = { Column: columns[i], Operator: operator, Value: value.trim(), GroupOperation: (i == 0 ? _GroupOperation : "OR"), IsSibling: (i == 0 ? _IsSibling : "true"), TableAlias: _tableAlias };
                if (!isNullOrEmpty(filter.Operator) && filter.Operator.length > 0 && filter.Operator.toLowerCase() === "null") {
                    filter = { Column: columns[i], Operator: (value.trim() === '1' ? 'ISNotNULL' : "ISNULL"), GroupOperation: (i == 0 ? _GroupOperation : "OR"), IsSibling: (i == 0 ? _IsSibling : "true"), TableAlias: _tableAlias };
                }
                _param.Filters.push(filter);
            }
            if ($(this).hasClass("required")) {
                $("#" + colNames + "Validator").html("");
            }
        }
        else {
            if ($(this).hasClass("required")) {
                isValidated = false;
                $("#" + colNames + "Validator").html("Required");
            }
        }
    });
    if (!isValidated) {
        return false;
    }
    return true;
}

function getEntityList($pagertable) {
    if (!$pagertable)
        $pagertable = $('.js-pager-table').first();
    var EntityList = [];
    var $checkboxes = $pagertable.find('.jsSelectChk:checked');
    $checkboxes.each(function (index, checkbox) {
        var $tr = $(checkbox).closest('tr');
        var entity_id = $tr.find('.jsEntity_ID').val();
        if (!isNullOrEmpty(entity_id)) {
            EntityList.push(entity_id);
        }
    });
    return EntityList;
}


function createModelFromForm($form) {
    if (isNullOrEmpty($form) || $form.length == 0) {
        return null;
    }
    var model = {};
    $form.find(':input').each(function (index, element) {
        var $element = $(element);
        if (!isNullOrEmpty($element.attr('name')) && $element.attr('name').split('.').length > 0) {
            var name = $element.attr('name').split('.').length == 1 ? $element.attr('name').split('.')[0] : $element.attr('name').split('.')[1];
            var value = $element.val();
            model[name] = value;
        }
    });
    return model;
}

//[Violation] 'readystatechange' handler took 119484ms
//RapidEntry: 1[Intervention] Slow network is detected.See https://www.chromestatus.com/feature/5636954674692096 for more details. Fallback font will be used while loading: http://localhost:51473/Content/ThemeAdmin/Content/Libraries/font-awesome/4.7.0/fonts/fontawesome-webfont.woff2?v=4.7.0
//24[Violation] Added non- passive event listener to a scroll- blocking < some > event.Consider marking event handler as 'passive' to make the page more responsive.See < URL >
//    jquery - 1.11.3.min.js:4[Violation] Added non- passive event listener to a scroll- blocking 'mousewheel' event.Consider marking event handler as 'passive' to make the page more responsive.See https://www.chromestatus.com/feature/5745543795965952
//jquery - 1.11.3.min.js:4[Violation] Added non- passive event listener to 

function bindModelPropertiesToForm($form, model) {
    if (isNullOrEmpty($form) || $form.length == 0) {
        return null;
    }
    $form.find(':input').each(function (index, element) {
        var $element = $(element);
        if (!isNullOrEmpty($element.attr('name')) && $element.attr('name').split('.').length > 0) {
            var name = $element.attr('name').split('.').length == 1 ? $element.attr('name').split('.')[0] : $element.attr('name').split('.')[1];
            var value = model[name] == null ? "" : model[name];
            $element.val(String(value));
        }
    });
    return $form;
}


