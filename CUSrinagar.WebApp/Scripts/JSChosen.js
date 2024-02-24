//https://harvesthq.github.io/chosen/options.html

function ChosenStyle() {
    $('.chosen-select').chosen({ allow_single_deselect: true, search_contains: true, case_sensitive_search:false});
    //resize the chosen on window resize
    $(window)
        .off('resize.chosen')
        .on('resize.chosen', function () {
            $('.chosen-select').each(function () {
                resizechosen($(this));
            })
        }).trigger('resize.chosen');
    //resize chosen on sidebar collapse/expand
    $(document).on('settings.ace.chosen', function (e, event_name, event_val) {
        if (event_name != 'sidebar_collapsed') return;
        $('.chosen-select').each(function () {
            resizechosen($(this));
        })
    });
}

function resizechosen($selects) {
    if ($selects != null) {
        $selects.each(function (index, element) {
            var $select = $(element);
            var $chosencontainer = $select.closest('.jsDDLContainer').find('.chosen-container');
            //$chosencontainer.css({ 'width': $select.parent().width() < $select.width() ? ($select.width() + 20) : ($select.parent().width() + 20) });
            //$chosencontainer.css({ 'width': ($select.width() + 20) });
            $select.next().css({ 'width': $select.parent().width() });
        });
    }
}

//$('.chosen-select-deselect').trigger("chosen:updated");

//function ChosenStyle() {
//    $('.chosen-select').chosen({ allow_single_deselect: true });
//    //resize the chosen on window resize

//    $(window)
//        .off('resize.chosen')
//        .on('resize.chosen', function () {
//            $('.chosen-select').each(function () {
//                var $this = $(this);
//                $this.next().css({ 'width': $this.parent().width() });
//            })
//        }).trigger('resize.chosen');
//    //resize chosen on sidebar collapse/expand
//    $(document).on('settings.ace.chosen', function (e, event_name, event_val) {
//        if (event_name != 'sidebar_collapsed') return;
//        $('.chosen-select').each(function () {
//            var $this = $(this);
//            $this.next().css({ 'width': $this.parent().width() });
//        })
//    });
//}