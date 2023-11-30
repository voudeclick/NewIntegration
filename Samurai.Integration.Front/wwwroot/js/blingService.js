samuraiApp.service('blingService', ['$http', function ($http) {
    var endPoint = window.config.webapi.url + '/Bling';

   
    this.getAllWarehouses = function (id, callBack) {
        if (id > 0) {
            $http({
                method: 'GET',
                url: endPoint + '/' + id + '/GetAllWarehouses'
            }).then(function successCallback(response) {
                var ret = { success: true, data: response.data };
                callBack(ret);
            }, function errorCallback(response) {
                var ret = { success: false, response: response };
                callBack(ret);
            });
        }
        else {
            var ret = { success: false, msg: 'Código de tenant inválido' };
            callBack(ret);
        }
    }

    this.getAllCategories = function (id, callBack) {
        if (id > 0) {
            $http({
                method: 'GET',
                url: endPoint + '/' + id + '/GetAllCategories'
            }).then(function successCallback(response) {
                var ret = { success: true, data: response.data };
                callBack(ret);
            }, function errorCallback(response) {
                var ret = { success: false, response: response };
                callBack(ret);
            });
        }
        else {
            var ret = { success: false, msg: 'Código de tenant inválido' };
            callBack(ret);
        }
    }

    this.getAllSituacoes = function (id, callBack) {
        if (id > 0) {
            $http({
                method: 'GET',
                url: endPoint + '/' + id + '/GetAllSituacoes'
            }).then(function successCallback(response) {
                var ret = { success: true, data: response.data };
                callBack(ret);
            }, function errorCallback(response) {
                var ret = { success: false, response: response };
                callBack(ret);
            });
        }
        else {
            var ret = { success: false, msg: 'Código de tenant inválido' };
            callBack(ret);
        }
    }
    
}]);