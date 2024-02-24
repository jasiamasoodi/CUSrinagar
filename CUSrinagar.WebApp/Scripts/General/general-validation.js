$(document).ready(function () {
    showErrorMessage($("#ErrorMessage").val());
    showSuccessMessage($("#SuccessMessage").val());
    generalFormValidation();
    math();
    masking();
});



function math() {
    $(document).on('keypress', '.jsnumeric', function () {
        return numbersOnly(event);
    });
    $(document).on('keypress', '.jsinteger', function () {
        return IntegersOnly(event);
    });
}

function masking() {
    if ($('.phone').length > 0) {
        $('.phone').mask('0000000000');
    }
    if ($('.dateMMDDYYY').length > 0) {
        $('.dateMMDDYYY').mask('M0/D0/2Y00', {
            translation: {
                'M': { pattern: /[0-1]/, optional: true },
                'D': { pattern: /[0-3]/, optional: true },
                'Y': { pattern: /[0]/ }
            }, placeholder: "US date format MM/DD/YYYY", title: "US date format{MM/DD/YYYY}"
        });
    }
    if ($('.cus-reg').length > 0) {
        $('.cus-reg').mask('JKL-DD-AAA-DDDDD', {
            translation: {
                'J': { pattern: /[c,C]/ },
                'K': { pattern: /[U,u]/ },
                'L': { pattern: /[S,s]/ },
                'A': { pattern: /[a-z,A-Z]/ },
                'D': { pattern: /[0-9]/ }
            }, placeholder: "Cluster University Registration No", title: "Cluster University Registration No"
        });
    }
}

function generalFormValidation() {
    var $forms = $(".jsMainForm");
    if ($forms.length > 0) {
        $forms.find('input[id*=Captcha]').attr('required', 'required');
        $forms.each(function (index, form) {
            $(form).validate();

            $(form).on('change', 'input[type=checkbox]', function () {
                var $checkbox = $(this);
                var value = $checkbox.is(':checked') ? "True" : "False";
                $checkbox.siblings('input[type=hidden]').val(value);
                $checkbox.val(value);
                return true;
            });

            

        });
        
    }
    $(".jsMainForm").submit(function (event) {
        var $form = $(this);
        if (!$form.valid()) {

            return false;
        }
    });
}
function validateRow($tr) {
    var $form = $tr.closest('form');
    //var $validator = $form.validate({ ignore: $form.find('tr :not(.jsDirtyRow) input'), focusInvalid: true }); // performance issue
    var $validator = $form.validate({focusInvalid: true }); // performance issue
    if ($form.valid()) {
        return true;
    } else {
        $($validator.errorList[0].element).select().focus();
        return false;
    }
}