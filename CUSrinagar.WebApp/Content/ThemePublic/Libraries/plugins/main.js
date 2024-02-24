var map;
$(document).ready(function () {

    /* ======= Flexslider ======= */
    $('.flexslider').flexslider({
        animation: "fade"
    });

    /* ======= jQuery Placeholder ======= */
    $('input, textarea').placeholder();


    /* ======= Carousels ======= */
    $('#news-carousel').carousel({ interval: false });
    $('#videos-carousel').carousel({ interval: false });
    $('#testimonials-carousel').carousel({ interval: 6000, pause: "hover" });
    $('#awards-carousel').carousel({ interval: false });


    /* ======= Flickr PhotoStream ======= */
    $('#flickr-photos').jflickrfeed({
        limit: 12,
        qstrings: {
            id: '32104790@N02' /* Use idGettr.com to find the flickr user id */
        },
        itemTemplate:
        '<li>' +
        '<a rel="prettyPhoto[flickr]" href="{{image}}" title="{{title}}">' +
        '<img src="{{image_s}}" alt="{{title}}" />' +
        '</a>' +
        '</li>'

    }, function (data) {
        $('#flickr-photos a').prettyPhoto();
    });

    /* ======= Pretty Photo ======= */
    // http://www.no-margin-for-errors.com/projects/prettyphoto-jquery-lightbox-clone/ 
    $('a.prettyphoto').prettyPhoto();

    /* ======= Twitter Bootstrap hover dropdown ======= */

    // apply dropdownHover to all elements with the data-hover="dropdown" attribute
    $('[data-hover="dropdown"]').dropdownHover();

    /* Nested Sub-Menus mobile fix */

    $('li.dropdown-submenu > a.trigger').on('click', function (e) {
        var current = $(this).next();
        current.toggle();
        e.stopPropagation();
        e.preventDefault();
        if (current.is(':visible')) {
            $(this).closest('li.dropdown-submenu').siblings().find('ul.dropdown-menu').hide();
        }
    });


    /* ======= Style Switcher ======= */

    $('#config-trigger').on('click', function (e) {
        var $panel = $('#config-panel');
        var panelVisible = $('#config-panel').is(':visible');
        if (panelVisible) {
            $panel.hide();
        } else {
            $panel.show();
        }
        e.preventDefault();
    });

    $('#config-close').on('click', function (e) {
        e.preventDefault();
        $('#config-panel').hide();
    });


    $('#color-options a').on('click', function (e) {
        var $styleSheet = $(this).attr('data-style');
        var $logoImage = $(this).attr('data-logo');
        $('#theme-style').attr('href', $styleSheet);
        $('#logo').attr('src', $logoImage);

        var $listItem = $(this).closest('li');
        $listItem.addClass('active');
        $listItem.siblings().removeClass('active');

        e.preventDefault();

    });




});

function getquicknotifications() {
    var currentDate = $("#currentDate").val();
    $.ajax({
        url: _QuickNotifications,
        dataType: "json",
        success: function (data) {
            var ul = $(".js-quick-notification");
            if (data != null) {
                ul.find('li').remove();
                $(data).each(function (index, notification) {
                    var li = '<li><a style="color:#4e4e4e !important;" href="#">' + notification.Description + '</a></li>';
                    if (notification.Link) {
                        var showLink = (calculateDate(currentDate, 0) <= calculateDate(notification.CreatedOn.split('T')[0], 6));

                        var newgif = showLink ? '<img style="width:"30px" src="/Content/ThemePublic/PrintImages/new.gif" />' : "";
                        var href = ((notification.IsLink) ? "http://" : "") + notification.Link.toLowerCase().replace("http://", "").replace("https://", "");
                        li = $('<li> ' + newgif + '<a style="color:#4e4e4e !important;" target="_blank" href="' + href + '"> ' + notification.Description + '</a> </li>');

                        if (!notification.IsLink) {
                            //li = $('<li> ' + newgif + '<a style="color:#4e4e4e !important;" href="https://docs.google.com/gview?url=http://www.cusrinagar.edu.in' + href + '&embedded=true"> ' + notification.Description + '</a> </li>');
                            li = $('<li> ' + newgif + '<a style="color:#4e4e4e !important;" href="' + href + '"> ' + notification.Description + '</a> </li>');
                        }
                    }
                    ul.append(li);
                });
            }
        }
    });
}
function calculateDate(date, addDay) {
    try {
        date = addDays(new Date(date), addDay);
        var d = new Date(date),
            month = '' + (d.getMonth() + 1),
            day = '' + d.getDate(),
            year = d.getFullYear();

        if (month.length < 2) month = '0' + month;
        if (day.length < 2) day = '0' + day;

        return [year, month, day].join('-');

    }
    catch (e)
    { return date; }
}

function addDays(theDate, days) {
    return new Date(theDate.getTime() + days * 24 * 60 * 60 * 1000);
}

function getTabContent(NotificationType, TabName) {
    $.ajax({
        url: _TabContentUrl + "?NotificationTypeIs=" + NotificationType,
        dataType: "json",
        success: function (data) {
            var ul = $("#" + TabName);
            $("#JsviewAll").attr('href', getBaseUrlWebApp() + "Notification/Notification" + "?TypeIS=" + TabName);

            if (data != null) {
                ul.find('li').remove();
                $(data).each(function (index, notification) {
                    var li = '<li><a href="' + notification.Link + '">' + notification.Description + '</a></li>';
                    var showLink = (calculateDate(currentDate, 0) <= calculateDate(notification.CreatedOn, 6));
                    if (notification.IsLink) {
                        //var href = ((notification.IsLink) ? "http://" : "") + notification.Link.toLowerCase().replace("http://", "").replace("https://", "");
                        li = $('<li> <a target="_blank" href="' + notification.Link + '">' + notification.Description + '</a> </li>');
                    }
                    ul.append(li);

                });
                if ($("#JsviewAll").length == 0) {
                    //it doesn't exist
                    return;
                    $("#tabs").append('<div class="panel-heading" style="border:0px dashed blue;padding:5px 0 5px 10px;margin:0; display:inline-block;width:100%;">'
                        + '<div style= "display: inline-block;width: 50%;float: left;border: 0px solid red; text-align:left;" >'
                        + '<a class="text-right" id= "JsviewAll" style="padding:0; border:0px solid red;" href="' + _notificationUrl + '">View all...</a>'
                        + '</div >'
                        + '<div style="display:inline-block; text-align:left; padding-left:10px; width: 49%;border: 0px solid cyan;">'
                        + '</div>'
                        + ' </div >');
                }
            }

        }
    });
}
function getenrollments() {
    $.ajax({
        url: _enrollmentUrl,
        dataType: "html",
        success: function (data) {
             $("#Enrollments").html(data);
        },
        error: function (xhr,error,ddff,fgfg) {
           
            }
    });
}