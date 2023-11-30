samuraiApp.service('userService', ['$http', function ($http) {
    var endPoint = window.config.webapi.url + '/user';


    this.addRole = function (role, callBack) {        
        $http({
            method: 'POST',
            url: `${endPoint}/AddRole`,
            headers: {
                'Content-Type': 'application/json'
            },
            data: JSON.stringify(role)
        }).then(function successCallback(response) {
            var ret = { success: true, data: response.data };
            callBack(ret);
        }, function errorCallback(response) {
            var ret = { success: false, response: response };
            callBack(ret);
        });        
    }

    this.removeRole = function (role, callBack) {
        $http({
            method: 'DELETE',
            url: `${endPoint}/RemoveRole`,
            headers: {
                'Content-Type': 'application/json'
            },
            data: JSON.stringify(role)
        }).then(function successCallback(response) {
            var ret = { success: true, data: response.data };
            callBack(ret);
        }, function errorCallback(response) {
            var ret = { success: false, response: response };
            callBack(ret);
        });
    }
}]);