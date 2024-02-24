
function IsActive(event) {
    $.ajax({
        url: _ChangeStatus,
        type: "GET",
        data: { id: $(event).attr("id") },
        cache: false,
        async: true,
        datatype: "html",
        contentType: "application/html; charset=utf-8",
        success: function (response) {
            alert(response);

        },
        error: function (response) {
        }
    });
    return true;
}




$(document).on('click', '.JSDelete', function (e) {

    var ans = confirm("Are you sure ,you want to delete user.");
    if (ans) {
        $.post(DeletePath, { id: $(this).attr("id") },
            function (response) {
                var $pagetable = $(".js-pager-table");
                loadpagertable($pagetable);
                alert(response.msg);

            }).fail(function () {
                console.log("error");
            });

        return true;
    } else {
        return false;
    }
});
$(document).on('click', '.JSRemoveRole', function (e) {

    var ans = confirm("Are you sure ,you want to remove role.");
    if (ans) {
        $.post(RemoveRolePath, { id: $(this).attr("id"), role: $(this).data("role") },
            function (response) {
                var $pagetable = $(".js-pager-table");
                loadpagertable($pagetable);
                alert(response.msg);

            }).fail(function () {
                console.log("error");
            });

        return true;
    } else {
        return false;
    }
});
$(document).on('click', '.JSAddRole', function (e) {

    var ans = confirm("Are you sure ,you want to add role.");
    var id = $(this).attr("id");
    if (ans) {
        $.post(AddRolePath, { id: id, role: $(this).data("role") },
            function (response) {
                if (response.status) {
                    var url = "/CUSrinagarAdminPanel/Bill/EditSetter?id=" + id;
                    window.location.href = url;;
                }

            }).fail(function () {
                console.log("error");
            });

        return true;
    } else {
        return false;
    }
});