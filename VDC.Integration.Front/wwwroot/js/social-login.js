 $(function () {
    $('.btn-entrar').click(function (e) {
        $(this).prop('disabled', true);
        $(this).html(
            '<span class="spinner-grow spinner-grow-sm" role="status" aria-hidden="true"></span>Entrando...'
        );
        $('#provider').val($(this).val());
        $('form').submit();
    });
});

