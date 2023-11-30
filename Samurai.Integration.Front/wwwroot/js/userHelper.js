
function redirectLogin() {
    location.href = '/Identity/Account/Login';
}

function redirectAcessDenied() {
    location.href = '/Identity/Account/AccessDenied';
}

function getUserLogged() {
    return parseJwt(localStorage.getItem('tokenApi'));
}

function readOnly() {
    return isInRole('Viewer');
}

function isInRole(roleName) {
    const userLogged = getUserLogged();

    if (!userLogged) {
        return false;
    }

    const role = userLogged.role;

    if (!role) {
        return false;
    }

    if (Array.isArray(role)) {
        const roles = role;

        return roles.includes(roleName);
    }

    return role == roleName;
}

function parseJwt(token) {
    try {
        return JSON.parse(atob(token.split('.')[1]));
    } catch (e) {
        return null;
    }
};