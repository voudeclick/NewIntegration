$.ajaxSetup({
    beforeSend: (xhr) =>
        xhr.setRequestHeader("Authorization",
            "Bearer " + localStorage.getItem('tokenApi')),
    statusCode: {
        401: redirectLogin,
        403: redirectAcessDenied
    }
});
