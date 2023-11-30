var samuraiApp = angular.module('samuraiApp', ['datatables', 'datatables.buttons', 'ngScrollToError', 'ngTagsInput'])
    .factory('Interceptor', Interceptor);

Interceptor.$inject = ['$q'];

function Interceptor($q) {
    return {
        request: function (config) {
            if (localStorage.getItem('tokenApi')) {
                config.headers['Authorization'] = "Bearer " + localStorage.getItem('tokenApi');
            }
            
            return config;
        },
        responseError: function (error) {
            if (error.status === 401) {
                redirectLogin();
            } else if (error.status === 403) {
                redirectAcessDenied();
            }

            return $q.reject(error);
        }
    };
}

samuraiApp.config(['$httpProvider', InterceptorProvider]);

function InterceptorProvider($httpProvider) {
    $httpProvider.interceptors.push('Interceptor');
}

samuraiApp.service('util', ['$rootScope', '$http', '$window', function ($rootScope, $http, $window) {
    this.showError = function (result) {
        var errors = '';
        if (result.response)
            result = result.response;

        if (result.data && result.data.errors) {
            $.each(result.data.errors, function (i, m) {
                if (m.reason)
                    errors += m.reason;
                else
                    errors += JSON.stringify(m);
            });
        }
        else if (typeof result === 'object') {
            errors = JSON.stringify(result);
        } else {
            errors = result;
        }

        alert(errors);
    }
}]);

samuraiApp.service('tenantService', ['$http', function ($http) {
    var endPoint = window.config.webapi.url + '/Tenant';

    this.getTenant = function (id, callBack) {
        if (id > 0) {
            $http({
                method: 'GET',
                url: endPoint + '/' + id
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

    this.saveTenant = function (tenant, isEdit, callBack) {
        var method = isEdit ? 'PUT' : 'POST';
        var url = endPoint + (isEdit ? '/' + tenant.id : '/');
       
        $http({
            method: method,
            url: url,
            headers: {
                'Content-Type': 'application/json'
            },
            data: JSON.stringify(tenant)            
        }).then(function successCallback(response) {            
            var ret = { success: true, data: response.data };
            callBack(ret);
        }, function errorCallback(response) {
            var ret = { success: false, response: response };
            callBack(ret);
        });
    }

    this.postClearMillenniumTransId = function (tenantId, transIdType, dateUpdate, callBack) {

        const url = `${endPoint}/${tenantId}/ClearMillenniumTransId/${transIdType}/${dateUpdate.toISOString()}`;
        
        $http({
            method: 'POST',
            url: url,
            headers: {
                'Content-Type': 'application/json'
            }
        }).then(function successCallback(response) {
            var ret = { success: true, data: response.data };
            callBack(ret);
        }, function errorCallback(response) {
            var ret = { success: false, response: response };
            callBack(ret);
        });
    }
}]);

samuraiApp.service('omieService', ['$http', function ($http) {
    var endPoint = window.config.webapi.url + '/Omie';

    this.getAllLocalEstoque = function (id, callBack) {
        if (id > 0) {
            $http({
                method: 'GET',
                url: endPoint + '/' + id + '/GetAllLocalEstoque'
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

    this.getAllCategorias = function (id, callBack) {
        if (id > 0) {
            $http({
                method: 'GET',
                url: endPoint + '/' + id + '/GetAllCategorias'
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

    this.getAllContaCorrente = function (id, callBack) {
        if (id > 0) {
            $http({
                method: 'GET',
                url: endPoint + '/' + id + '/GetAllContaCorrente'
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

    this.getAllEtapasFaturamento = function (id, callBack) {
        if (id > 0) {
            $http({
                method: 'GET',
                url: endPoint + '/' + id + '/getAllEtapasFaturamento'
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

    this.getAllCenarioImposto = function (id, callBack) {
        if (id > 0) {
            $http({
                method: 'GET',
                url: endPoint + '/' + id + '/GetAllCenarioImposto'
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

    this.getAllTransportadora = function (id, callBack) {
        if (id > 0) {
            $http({
                method: 'GET',
                url: endPoint + '/' + id + '/getAllTransportadora'
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

samuraiApp.service('queueService', ['$http', function ($http) {
    var endPoint = window.config.webapi.url + '/Queue';

    this.getQueuesCount = function (id, callBack) {
        if (id > 0) {
            $http({
                method: 'GET',
                url: endPoint + '/' + id + '/Count'
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

    this.postOrderShopify = function (tenantId, orderId, callBack) {
        var url = endPoint + '/Order/Shopify';

        $http({
            method: 'POST',
            url: url,
            headers: {
                'Content-Type': 'application/json'
            },
            data: {
                "tenantId": tenantId,
                "orderId": orderId
            }
        }).then(function successCallback(response) {
            var ret = { success: true, data: response.data };
            callBack(ret);
        }, function errorCallback(response) {
            var ret = { success: false, response: response };
            callBack(ret);
        });
    }

    this.postShopifyFilterDate = function (tenantId, dateMin, dateMax, callBack) {
        var url = endPoint + '/Order/ShopifyFilterDate';

        $http({
            method: 'POST',
            url: url,
            headers: {
                'Content-Type': 'application/json'
            },
            data: {
                "tenantId": tenantId,
                "DateMin": dateMin,
                "DateMax": dateMax
            }
        }).then(function successCallback(response) {
            var ret = { success: true, data: response.data };
            callBack(ret);
        }, function errorCallback(response) {
            var ret = { success: false, response: response };
            callBack(ret);
        });
    }

    this.postAllCollectionsShopify = function (tenantId, callBack) {
        var url = endPoint + '/Collection/Shopify';

        $http({
            method: 'POST',
            url: url,
            headers: {
                'Content-Type': 'application/json'
            },
            data: {
                "tenantId": tenantId
            }
        }).then(function successCallback(response) {
            var ret = { success: true, data: response.data };
            callBack(ret);
        }, function errorCallback(response) {
            var ret = { success: false, response: response };
            callBack(ret);
        });
    }

    this.postOrderSC = function (tenantId, orderNumber, callBack) {
        var url = endPoint + '/Order/SC';

        $http({
            method: 'POST',
            url: url,
            headers: {
                'Content-Type': 'application/json'
            },
            data: {
                "tenantId": tenantId,
                "orderNumber": orderNumber
            }
        }).then(function successCallback(response) {
            var ret = { success: true, data: response.data };
            callBack(ret);
        }, function errorCallback(response) {
            var ret = { success: false, response: response };
            callBack(ret);
        });
    }

    this.postOrderMillennium = function (tenantId, externalOrderId, callBack) {
        var url = endPoint + '/Order/Millennium';

        $http({
            method: 'POST',
            url: url,
            headers: {
                'Content-Type': 'application/json'
            },
            data: {
                "tenantId": tenantId,
                "externalOrderId": externalOrderId
            }
        }).then(function successCallback(response) {
            var ret = { success: true, data: response.data };
            callBack(ret);
        }, function errorCallback(response) {
            var ret = { success: false, response: response };
            callBack(ret);
        });
    }

    this.postProductMillennium = function (tenantId, productId, callBack) {
        var url = endPoint + '/Product/Millennium';

        $http({
            method: 'POST',
            url: url,
            headers: {
                'Content-Type': 'application/json'
            },
            data: {
                "tenantId": tenantId,
                "productId": productId
            }
        }).then(function successCallback(response) {
            var ret = { success: true, data: response.data };
            callBack(ret);
        }, function errorCallback(response) {
            var ret = { success: false, response: response };
            callBack(ret);
        });
    }

    this.postOrderNexaas = function (tenantId, externalOrderId, callBack) {
        var url = endPoint + '/Order/Nexaas';

        $http({
            method: 'POST',
            url: url,
            headers: {
                'Content-Type': 'application/json'
            },
            data: {
                "tenantId": tenantId,
                "externalOrderId": externalOrderId
            }
        }).then(function successCallback(response) {
            var ret = { success: true, data: response.data };
            callBack(ret);
        }, function errorCallback(response) {
            var ret = { success: false, response: response };
            callBack(ret);
        });
    }

    this.postProductNexaas = function (tenantId, productId, listAllProducts, callBack) {
        var url = endPoint + '/Product/Nexaas';

        $http({
            method: 'POST',
            url: url,
            headers: {
                'Content-Type': 'application/json'
            },
            data: {
                "tenantId": tenantId,
                "productId": parseInt(productId),
                "listAllProducts": listAllProducts
            }
        }).then(function successCallback(response) {
            var ret = { success: true, data: response.data };
            callBack(ret);
        }, function errorCallback(response) {
            var ret = { success: false, response: response };
            callBack(ret);
        });
    }

    this.postOrderOmie = function (tenantId, externalOrderId, callBack) {
        var url = endPoint + '/Order/Omie';

        $http({
            method: 'POST',
            url: url,
            headers: {
                'Content-Type': 'application/json'
            },
            data: {
                "tenantId": tenantId,
                "externalOrderId": externalOrderId
            }
        }).then(function successCallback(response) {
            var ret = { success: true, data: response.data };
            callBack(ret);
        }, function errorCallback(response) {
            var ret = { success: false, response: response };
            callBack(ret);
        });
    }

    this.postProductOmie = function (tenantId, productId, listAllProducts, callBack) {
        var url = endPoint + '/Product/Omie';

        $http({
            method: 'POST',
            url: url,
            headers: {
                'Content-Type': 'application/json'
            },
            data: {
                "tenantId": tenantId,
                "productId": parseInt(productId),
                "listAllProducts": listAllProducts
            }
        }).then(function successCallback(response) {
            var ret = { success: true, data: response.data };
            callBack(ret);
        }, function errorCallback(response) {
            var ret = { success: false, response: response };
            callBack(ret);
        });
    }

    this.postUpdateTrackingPier8 = function (tenantId, externalOrderId, callBack) {
        var url = endPoint + '/Pier8/UpdateTrackingByIdShopify';

        $http({
            method: 'POST',
            url: url,
            headers: {
                'Content-Type': 'application/json'
            },
            data: {
                "tenantId": tenantId,
                "externalOrderId": externalOrderId
            }
        }).then(function successCallback(response) {
            var ret = { success: true, data: response.data };
            callBack(ret);
        }, function errorCallback(response) {
            var ret = { success: false, response: response };
            callBack(ret);
        });
    }

    this.postOrderBling = function (tenantId, externalOrderId, callBack) {
        var url = endPoint + '/Order/Bling';

        $http({
            method: 'POST',
            url: url,
            headers: {
                'Content-Type': 'application/json'
            },
            data: {
                "tenantId": tenantId,
                "externalOrderId": externalOrderId
            }
        }).then(function successCallback(response) {
            var ret = { success: true, data: response.data };
            callBack(ret);
        }, function errorCallback(response) {
            var ret = { success: false, response: response };
            callBack(ret);
        });
    }

    this.postProductBling = function (tenantId, productId, listAllProducts, callBack) {
        var url = endPoint + '/Product/Bling';

        $http({
            method: 'POST',
            url: url,
            headers: {
                'Content-Type': 'application/json'
            },
            data: {
                "tenantId": tenantId,
                "productId": productId,
                "listAllProducts": listAllProducts
            }
        }).then(function successCallback(response) {
            var ret = { success: true, data: response.data };
            callBack(ret);
        }, function errorCallback(response) {
            var ret = { success: false, response: response };
            callBack(ret);
        });
    }

    this.postOrderPluggTo = function (tenantId, externalOrderId, callBack) {
        var url = endPoint + '/Order/PluggTo';

        $http({
            method: 'POST',
            url: url,
            headers: {
                'Content-Type': 'application/json'
            },
            data: {
                "tenantId": tenantId,
                "externalOrderId": externalOrderId
            }
        }).then(function successCallback(response) {
            var ret = { success: true, data: response.data };
            callBack(ret);
        }, function errorCallback(response) {
            var ret = { success: false, response: response };
            callBack(ret);
        });
    }

    this.postProductPluggTo = function (tenantId, productId, productSku, listAllProducts, callBack) {
        var url = endPoint + '/Product/PluggTo';

        $http({
            method: 'POST',
            url: url,
            headers: {
                'Content-Type': 'application/json'
            },
            data: {
                "tenantId": tenantId,
                "productCode": productSku,
                "externalId": productId,
                "listAllProducts": listAllProducts
            }
        }).then(function successCallback(response) {
            var ret = { success: true, data: response.data };
            callBack(ret);
        }, function errorCallback(response) {
            var ret = { success: false, response: response };
            callBack(ret);
        });
    }

    this.postOrderAliExpress = function (tenantId, externalOrderId, callBack) {
        var url = endPoint + '/Order/AliExpress';

        $http({
            method: 'POST',
            url: url,
            headers: {
                'Content-Type': 'application/json'
            },
            data: {
                "tenantId": tenantId,
                "aliExpressOrderId": parseInt(externalOrderId)
            }
        }).then(function successCallback(response) {
            var ret = { success: true, data: response.data };
            callBack(ret);
        }, function errorCallback(response) {
            var ret = { success: false, response: response };
            callBack(ret);
        });
    }
}]);

samuraiApp.directive('loadingButton', function ($compile) {

    return {
        restrict: 'E',
        replace: true,
        scope: {
            ngModel: '='
        },
        template: "<button class=\"{{class}}\" type=\"submit\" ng-disabled=\"ngModel.isSending\"><span class=\"spinner-border spinner-border-sm\" role=\"status\" aria-hidden=\"true\" ng-show=\"ngModel.isSending\"></span>Enviar</button>",

        link: function (scope, element, attrs) {
            scope.ngModel = {
                isSending: false,
                isSuccess: false
            };
        }
    }
});

samuraiApp.directive('convertToNumber', function () {
    return {
        require: 'ngModel',
        link: function (scope, el, attr, ctrl) {
            ctrl.$parsers.push(function (value) {
                return parseInt(value, 10);
            });

            ctrl.$formatters.push(function (value) {
                return value !== null ? value.toString() : null;
            });
        }
    }
});