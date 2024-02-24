jQuery(function ($) {
    Badges();
    //------------------- Create New file ---------------
    $("#frmCreateNewFile").submit(function () {
        if ($(this).valid()) {
            var _FileNo = $("#frmCreateNewFile #FileNo").val();
            var _Subject = $("#frmCreateNewFile #Subject").val();
            var _Section = $("#frmCreateNewFile #Section").val();
            var _Description = $("#frmCreateNewFile #Description").val();
            $(".JSCreateFile").text('Working...').prop("disabled", true);

            $.ajax({
                url: "/CUSrinagarAdminPanel/FileTracking/CreateNewFile",
                type: "POST",
                async: true,
                data: JSON.stringify({ FileNo: _FileNo, Subject: _Subject, Section: _Section, Description: _Description }),
                dataType: "json",
                contentType: "application/json",
                traditional: false,
                success: function (response) {
                    if (Number(response) === -2) {
                        $(".msg1").empty().html("<div class='jsalert alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> File No. : " + _FileNo + " already exits , try different one.</a></div>");
                    } else if (Number(response) > 0) {
                        $(".msg1").empty().html("<div class='jsalert alert alert-success alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> " + _FileNo + " Created Successfully</a></div>");
                        $("#frmCreateNewFile #FileNo").val('');
                        $("#frmCreateNewFile #Subject").val('');
                        $("#frmCreateNewFile #Department").val('');
                        $("#frmCreateNewFile #Section").val('');
                        $("#frmCreateNewFile #Description").val('');
                        GetTableDetails("addednew", "", "");
                    }
                    else {
                        $(".msg1").empty().html("<div class='jsalert alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> " + _FileNo + " was not created. Check your details and try again</a></div>");
                    }
                    $(".JSCreateFile").text('Create File').prop("disabled", false);
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    alert('status code: ' + jqXHR.status + '\n Please check your internet connection or refresh and try again.');
                    $(".JSCreateFile").text('Create File').prop("disabled", false);
                }
            });
        }
        return false;
    });

    //------------------- Search file ---------------
    $("#frmSearchFilter").submit(function () {
        if ($(this).valid()) {
            var _SearchQuery = $("#frmSearchFilter #Filters_SearchQuery").val();
            var _From = $("#frmSearchFilter #Filters_From").val();
            var _To = $("#frmSearchFilter #Filters_To").val();
            if (_SearchQuery === "" && _From === "" && _To === "") {
                return false;
            }
            $(".JSSearch").text('Working...').prop("disabled", true);
            GetTableDetails(_SearchQuery, _From, _To);
        }
        return false;
    });

    function GetTableDetails(_SearchQuery, _From, _To) {
        $.ajax({
            url: "/CUSrinagarAdminPanel/FileTracking/GetFiles",
            type: "POST",
            async: true,
            data: JSON.stringify({ SearchQuery: _SearchQuery, From: _From, To: _To }),
            dataType: "html",
            contentType: "application/json",
            traditional: false,
            success: function (response) {
                $(".JSFileTrackingDetails").empty().html(response);
                $(".JSSearch").text('Search').prop("disabled", false);
                $('[href="#JSActiveFiles"]').click();
                Badges();
            },
            error: function (jqXHR, textStatus, errorThrown) {
                alert('status code: ' + jqXHR.status + '\n Please check your internet connection or refresh and try again.');
                $(".JSSearch").text('Search').prop("disabled", false);
                Badges();
            }
        });
    }

    //---------------- Get total files count -------------
    function Badges() {
        var FFCount = $("#JSFilesForwardedToYou tbody .FF").length;

        if (FFCount > 0) {
            $(".jsbadge").addClass("quadraText");
        } else {
            $(".jsbadge").removeClass("quadraText");
        }

        $(".badge .JsForwardCount").empty().text("+" + FFCount);
        $(".badge .JsACount").empty().text(($("#JSActiveFiles tbody .AF").length));
        $(".badge .JsCCount").empty().text(($("#JSFileClosedByYou tbody .CF").length));
        $(".badge .JsTCount").empty().text(($("#JSTrackAllYourFiles tbody .TF").length));
    }

    //--------------- Delete File -------------------
    $("body").on("click", ".JSActionDelete", function () {
        var element = $(this);
        var file_ID = element.attr("data-file-id");
        if (confirm("Are you sure you want to delete this file?")) {
            $("#Jspleasewait").show();
            $.ajax({
                url: "/CUSrinagarAdminPanel/FileTracking/DeleteFile",
                type: "POST",
                async: true,
                data: JSON.stringify({ File_ID: file_ID }),
                dataType: "json",
                contentType: "application/json",
                traditional: false,
                success: function (response) {
                    if (response === true) {
                        $("#Jspleasewait").hide();
                        element.closest("tbody tr").remove();
                        Badges();
                    } else {
                        $("#Jspleasewait").hide();
                        alert("File not deleted.");
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    alert('status code: ' + jqXHR.status + '\n Please check your internet connection or refresh and try again.');
                    $("#Jspleasewait").hide();
                }
            });
        }
    });

    //--------------- Close File -------------------
    $("body").on("click", ".JSActionClose", function () {
        var element = $(this);
        var file_ID = element.attr("data-file-id");
        $("#frmCloseFiles #FileTrackingHistory_File_ID").val(file_ID);
    });

    $("#frmCloseFiles").submit(function (e) {
        if ($(this).valid()) {
            var _File_ID = $("#frmCloseFiles #FileTrackingHistory_File_ID").val();
            var _Section = $("#frmCloseFiles #FileTrackingHistory_Section").val();
            var _Remarks = $("#frmCloseFiles #FileTrackingHistory_Remarks").val();

            $(".JSBtnCloseFile").text('Working...').prop("disabled", true);

            $.ajax({
                url: "/CUSrinagarAdminPanel/FileTracking/CloseFile",
                type: "POST",
                async: true,
                data: JSON.stringify({ File_ID: _File_ID, Section: _Section, Remarks: _Remarks }),
                dataType: "json",
                contentType: "application/json",
                traditional: false,
                success: function (response) {
                    if (response === true) {
                        $(".msg3").empty().html("<div class='jsalert alert alert-success alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> Closed Successfully</a></div>");
                        $("#frmCloseFiles #FileTrackingHistory_File_ID").val('');
                        $("#frmCloseFiles #FileTrackingHistory_Section").val('');
                        $("#frmCloseFiles #FileTrackingHistory_Remarks").val('');

                        GetTableDetails("addednew", "", "");
                    }
                    else {
                        $(".msg3").empty().html("<div class='jsalert alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> File not closed. Check your details and try again</a></div>");
                    }
                    $(".JSBtnCloseFile").text('Close File').prop("disabled", false);
                    return false;
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    alert('status code: ' + jqXHR.status + '\n Please check your internet connection or refresh and try again.');
                    $(".JSBtnCloseFile").text('Close File').prop("disabled", false);
                }
            });
        }
        return false;
    });


    //--------------- Re-Open File -------------------
    $("body").on("click", ".JSActionReOpen", function () {
        var element = $(this);
        var file_ID = element.attr("data-file-id");
        $("#frmReOpenFiles #FileTrackingHistory_File_ID").val(file_ID);
    });

    $("#frmReOpenFiles").submit(function (e) {
        if ($(this).valid()) {
            var _File_ID = $("#frmReOpenFiles #FileTrackingHistory_File_ID").val();
            var _Section = $("#frmReOpenFiles #FileTrackingHistory_Section").val();
            var _Remarks = $("#frmReOpenFiles #FileTrackingHistory_Remarks").val();

            $(".JSBtnReOpenFile").text('Working...').prop("disabled", true);

            $.ajax({
                url: "/CUSrinagarAdminPanel/FileTracking/ReOpenFile",
                type: "POST",
                async: true,
                data: JSON.stringify({ File_ID: _File_ID, Section: _Section, Remarks: _Remarks }),
                dataType: "json",
                contentType: "application/json",
                traditional: false,
                success: function (response) {
                    if (response === true) {
                        $(".msg5").empty().html("<div class='jsalert alert alert-success alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> Re-opended Successfully</a></div>");
                        $("#frmReOpenFiles #FileTrackingHistory_File_ID").val('');
                        $("#frmReOpenFiles #FileTrackingHistory_Section").val('');
                        $("#frmReOpenFiles #FileTrackingHistory_Remarks").val('');

                        GetTableDetails("addednew", "", "");
                    }
                    else {
                        $(".msg5").empty().html("<div class='jsalert alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> File not ReOpended. Check your details and try again</a></div>");
                    }
                    $(".JSBtnReOpenFile").text('Re-Open File').prop("disabled", false);
                    return false;
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    alert('status code: ' + jqXHR.status + '\n Please check your internet connection or refresh and try again.');
                    $(".JSBtnReOpenFile").text('Re-Open File').prop("disabled", false);
                }
            });
        }
        return false;
    });


    //--------------- Revert File -------------------
    $("body").on("click", ".JSActionRevert", function () {
        var element = $(this);
        var file_ID = element.attr("data-file-id");
        $("#frmRevertFiles #FileTrackingHistory_File_ID").val(file_ID);
    });

    $("#frmRevertFiles").submit(function (e) {
        if ($(this).valid()) {
            var _File_ID = $("#frmRevertFiles #FileTrackingHistory_File_ID").val();
            var _Section = $("#frmRevertFiles #FileTrackingHistory_Section").val();
            var _Remarks = $("#frmRevertFiles #FileTrackingHistory_Remarks").val();

            $(".JSBtnRevertFile").text('Working...').prop("disabled", true);

            $.ajax({
                url: "/CUSrinagarAdminPanel/FileTracking/RevertFile",
                type: "POST",
                async: true,
                data: JSON.stringify({ File_ID: _File_ID, Section: _Section, Remarks: _Remarks }),
                dataType: "json",
                contentType: "application/json",
                traditional: false,
                success: function (response) {
                    if (response === true) {
                        $(".msg4").empty().html("<div class='jsalert alert alert-success alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> Reverted Successfully</a></div>");
                        $("#frmRevertFiles #FileTrackingHistory_File_ID").val('');
                        $("#frmRevertFiles #FileTrackingHistory_Section").val('');
                        $("#frmRevertFiles #FileTrackingHistory_Remarks").val('');

                        GetTableDetails("addednew", "", "");
                    }
                    else {
                        $(".msg4").empty().html("<div class='jsalert alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> File not reverted. Check your details and try again</a></div>");
                    }
                    $(".JSBtnRevertFile").text('Revert File').prop("disabled", false);
                    return false;
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    alert('status code: ' + jqXHR.status + '\n Please check your internet connection or refresh and try again.');
                    $(".JSBtnRevertFile").text('RevertFile File').prop("disabled", false);
                }
            });
        }
        return false;
    });


    //--------------- Forward File -------------------
    $("body").on("click", ".JSActionForward", function () {
        var element = $(this);
        var file_ID = element.attr("data-file-id");
        $("#frmForwardFiles #FileTrackingHistory_File_ID").val(file_ID);
    });

    $("#frmForwardFiles").submit(function (e) {
        if ($(this).valid()) {
            var _File_ID = $("#frmForwardFiles #FileTrackingHistory_File_ID").val();
            var _Section = $("#frmForwardFiles #FileTrackingHistory_Section").val();
            var _Remarks = $("#frmForwardFiles #FileTrackingHistory_Remarks").val();
            var _User_ID = $("#frmForwardFiles #FileTrackingHistory_User_ID").val();

            $(".JSBtnForwardFile").text('Working...').prop("disabled", true);

            $.ajax({
                url: "/CUSrinagarAdminPanel/FileTracking/ForwardFile",
                type: "POST",
                async: true,
                data: JSON.stringify({ File_ID: _File_ID, Section: _Section, Remarks: _Remarks, User_ID: _User_ID }),
                dataType: "json",
                contentType: "application/json",
                traditional: false,
                success: function (response) {
                    if (response === true) {
                        $(".msg6").empty().html("<div class='jsalert alert alert-success alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> Forwarded Successfully</a></div>");
                        $("#frmForwardFiles #FileTrackingHistory_File_ID").val('');
                        $("#frmForwardFiles #FileTrackingHistory_Section").val('');
                        $("#frmForwardFiles #FileTrackingHistory_Remarks").val('');

                        GetTableDetails("addednew", "", "");
                    }
                    else {
                        $(".msg6").empty().html("<div class='jsalert alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> File not Forwarded. Check your details and try again</a></div>");
                    }
                    $(".JSBtnForwardFile").text('Forward File').prop("disabled", false);
                    return false;
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    alert('status code: ' + jqXHR.status + '\n Please check your internet connection or refresh and try again.');
                    $(".JSBtnForwardFile").text('ForwardFile File').prop("disabled", false);
                }
            });
        }
        return false;
    });


    //--------------- Accept File -------------------
    $("body").on("click", ".JSActionAccept", function () {
        var element = $(this);
        var file_ID = element.attr("data-file-id");
        $("#frmAcceptFiles #FileTrackingHistory_File_ID").val(file_ID);
    });

    $("#frmAcceptFiles").submit(function (e) {
        if ($(this).valid()) {
            var _File_ID = $("#frmAcceptFiles #FileTrackingHistory_File_ID").val();
            var _Section = $("#frmAcceptFiles #FileTrackingHistory_Section").val();
            var _Remarks = $("#frmAcceptFiles #FileTrackingHistory_Remarks").val();

            $(".JSBtnAcceptFile").text('Working...').prop("disabled", true);

            $.ajax({
                url: "/CUSrinagarAdminPanel/FileTracking/AcceptFile",
                type: "POST",
                async: true,
                data: JSON.stringify({ File_ID: _File_ID, Section: _Section, Remarks: _Remarks }),
                dataType: "json",
                contentType: "application/json",
                traditional: false,
                success: function (response) {
                    if (response === true) {
                        $(".msg4").empty().html("<div class='jsalert alert alert-success alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> Accepted Successfully</a></div>");
                        $("#frmAcceptFiles #FileTrackingHistory_File_ID").val('');
                        $("#frmAcceptFiles #FileTrackingHistory_Section").val('');
                        $("#frmAcceptFiles #FileTrackingHistory_Remarks").val('');

                        GetTableDetails("addednew", "", "");
                    }
                    else {
                        $(".msg4").empty().html("<div class='jsalert alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> File not Accepted. Check your details and try again</a></div>");
                    }
                    $(".JSBtnAcceptFile").text('Accept File').prop("disabled", false);
                    return false;
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    alert('status code: ' + jqXHR.status + '\n Please check your internet connection or refresh and try again.');
                    $(".JSBtnAcceptFile").text('Accept File').prop("disabled", false);
                }
            });
        }
        return false;
    });



    //--------------- TrackingDetails File -------------------
    $("body").on("click", ".JSActionTrackDetails", function () {
        var element = $(this);
        var _File_ID = element.attr("data-file-id");
        $(".JSDisplayTrackingDetails").empty().text('Fetching details...');

        $.ajax({
            url: "/CUSrinagarAdminPanel/FileTracking/TrackFileDetails",
            type: "POST",
            async: true,
            data: JSON.stringify({ File_ID: _File_ID }),
            dataType: "html",
            contentType: "application/json",
            traditional: false,
            success: function (response) {
                if (response !== "") {
                    $(".JSDisplayTrackingDetails").empty().html(response);
                }
                else {
                    $(".JSDisplayTrackingDetails").empty().text('Please check your internet connection or refresh and try again.');
                }
            },
            error: function (jqXHR, textStatus, errorThrown) {
                $(".JSDisplayTrackingDetails").empty().text('status code: ' + jqXHR.status + ' Please check your internet connection or refresh and try again.');
            }
        });
    });


});
