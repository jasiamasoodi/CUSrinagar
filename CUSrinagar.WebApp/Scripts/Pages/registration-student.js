$(document).ready(function () {

    //$('#s1').cascadingSelect({
    //    subSelects: ['#s2', '#s3'],
    //    data: data
    //});

    function fillSelectOptions(selectType) {
        var $selectElement = $("[data-type=" + selectType + "]");
        var _dependentontypesSelect = $selectElement.attr('data-dependentontype').split(',');
        var unfilledSubtypes = false;
        var _subselectTypes = {};
        $.each(_dependentontypesSelect, function (index, type) {
            var $SelectElement = $("[data-type=" + type + "]").first();
            var _SelectType = $SelectElement.attr('data-type')
            if ($SelectElement.val().length == 0) unfilledSubtypes = true;
            else
                _subselectTypes[_SelectType] = $SelectElement.val();
        });

        if (unfilledSubtypes) return;
        var _url = '/Registration/CascadeAddress';
        $.ajax({
            url: _url,
            type: "POST",
            datatype: "Json",
            data: { CurrentSelectType: selectType, DependentSubSelectType: _subselectTypes },
            success: function (data) {
                $.each(data, function (index, item) {
                    $selectElement.append('<option value="' + item.Value + '">' + item.Text + '</option>');
                });
            },
            error: function (a, b, c, d, e) {
                console.log(c)
            }
        });

    }
    $(document).on('change', 'select', function (e) {

        var $selectElement = $(this);
        var _dependentTypes = $selectElement.attr('data-dependenttype').split(',');
        for (var i = 0; i < _dependentTypes.length; i++) {
            fillSelectOptions(_dependentTypes[i])
        }

        return;

        var _CurrentSelectType = $selectElement.attr('data-type');
        var _url = '/Registration/CascadeAddress';
        var selectGroup = $selectElement.attr('data-group');
        var $subSelects = $('[data-group=' + selectGroup + ']');
        var $tehsil = $("select[name$=Tehsil]");

        var _subselectTypes = [];
        $subSelects.each(function (index, element) {
            var _SelectType = $(element).attr('data-type');
            if (_CurrentSelectType != _SelectType && $(element).val().length > 0)
                _subselectTypes.push({ _SelectType: $(element).val() });
        });
        if (_subselectTypes.length == 0) return;
        $.ajax({
            url: _url,
            type: "POST",
            datatype: "Json",
            data: { CurrentSelectType: _CurrentSelectType, SubSelectType: _subselectTypes },
            success: function (data) {
                $.each(data, function (index, item) {
                    $selectElement.append('<option value="' + item.Value + '">' + item.Text + '</option>');
                });
            },
            error: function (a, b, c, d, e) {
                console.log(c)
            }
        });

    });


});