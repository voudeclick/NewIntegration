samuraiApp.controller('tenantController', ['$scope', '$http', 'util', 'tenantService', 'omieService', 'queueService', 'blingService', 'shopifyService', function ($scope, $http, util, tenantService, omieService, queueService, blingService, shopifyService) {
    $scope.readOnly = readOnly();
    $scope.editMode = false;
    $scope.isSending = false;
    $scope.tenant = {};
    $scope.config = {};
    $scope.tenantType = null;
    $scope.views = {
        Tenant: { title: 'Tenant' },
        Tools: { title: 'Ferramentas' }
    };
    $scope.currentView = $scope.views.Tenant;

    $scope.typeEcommerce = {
        IsShopify: false,
        IsSellerCenter: false,
        IsTray: false
    };

    $scope.typeErp = {
        IsOmie: false,
        IsMillennium: false,
        IsNexaas: false,
        IsBling: false,
        IsPluggTo: false,
        IsAliExpress: false
    };

    $scope.show = {
        erp: true,
        tools: true,
        ecommerce: true
    };

    $scope.init = function (id) {
        $scope.config = window.config;
        if (id && id != 0) {
            $scope.getTenant(id);
        } else {
            $scope.initializeTenant();
        }     
    }

    $scope.setTypeEcommerce = function (type) {
        cleanTypeEcommerce();
        switch (type) {
            case "Shopify":
                $scope.typeEcommerce.IsShopify = true;
                break;
            case "SellerCenter":
                $scope.typeEcommerce.IsSellerCenter = true;
                break;
            case "Tray":
                $scope.typeEcommerce.IsTray = true;
                break;
            default:
                break;
        }

    }

    $scope.showDiv = function (type) {
        $scope.cleanShowDiv();
        switch (type) {
            case "erp":
                $scope.show.erp = true;
                break;
            case "ecommerce":
                $scope.show.ecommerce = true;
                break;
            case "tools":
                $scope.show.tools = true;
                break;
            default:
                break;
        }

    }

    $scope.cleanShowDiv = function (show = false) {
        $scope.show.tools = show;
        $scope.show.erp = show;
        $scope.show.ecommerce = show;

    }

    $scope.setTypeErp = function (type) {
        cleanTypeErp();
        switch (type) {
            case "Omie":
                $scope.typeErp.IsOmie = true;
                break;
            case "Millennium":
                $scope.typeErp.IsMillennium = true;
                break;
            case "Nexaas":
                $scope.typeErp.IsNexaas = true;
                break;
            case "Bling":
                $scope.typeErp.IsBling = true;
                break;
            case "PluggTo":
                $scope.typeErp.IsPluggTo = true;
                break;
            case "Tray":
                $scope.typeErp.IsTray = true;
                break;
            default:
                break;
        }
        $scope.typeChanged();

    }

    cleanTypeEcommerce = function () {
        $scope.typeEcommerce.IsShopify = false;
        $scope.typeEcommerce.IsSellerCenter = false;
        $scope.typeEcommerce.IsTray = false;
    }

    cleanTypeErp = function () {
        $scope.typeErp.IsOmie = false;
        $scope.typeErp.IsMillennium = false;
        $scope.typeErp.IsNexaas = false;
        $scope.typeErp.IsBling = false;
        $scope.typeErp.IsPluggTo = false;
        $scope.typeErp.IsAliExpress = false;
    }

    $scope.setView = function (view) {
        $scope.currentView = view;
    }

    $scope.getTenant = function (id) {
        tenantService.getTenant(id, function (response) {
            if (response.success && response.data.isSuccess) {
                $scope.tenant = response.data.value;
                $scope.tenantType = $scope.tenant.type;
                $scope.editMode = true;
                $scope.setTypeEcommerce($scope.tenant.integrationType);
                $scope.setTypeErp($scope.tenant.type);
                if ($scope.typeErp.IsOmie) {
                    omieService.getAllLocalEstoque(id, function (response) {
                        if (response.success && response.data.isSuccess) {
                            $scope.omieLocaisEstoque = response.data.value;
                        }
                        else {
                            util.showError(response);
                        }
                    });
                    omieService.getAllCategorias(id, function (response) {
                        if (response.success && response.data.isSuccess) {
                            $scope.omieCategorias = response.data.value;
                        }
                        else {
                            util.showError(response);
                        }
                    });
                    omieService.getAllContaCorrente(id, function (response) {
                        if (response.success && response.data.isSuccess) {
                            $scope.omieContasCorrente = response.data.value;
                        }
                        else {
                            util.showError(response);
                        }
                    });
                    omieService.getAllEtapasFaturamento(id, function (response) {
                        if (response.success && response.data.isSuccess) {
                            $scope.omieEtapasFaturamento = response.data.value;
                        }
                        else {
                            util.showError(response);
                        }
                    });
                    omieService.getAllCenarioImposto(id, function (response) {
                        if (response.success && response.data.isSuccess) {
                            $scope.omieCenariosImposto = response.data.value;
                        }
                        else {
                            util.showError(response);
                        }
                    });
                    omieService.getAllTransportadora(id, function (response) {
                        if (response.success && response.data.isSuccess) {
                            $scope.omieTransportadoras = response.data.value;
                        }
                        else {
                            util.showError(response);
                        }
                    });
                    $scope.loadAllCodigoParcela();
                }
                if ($scope.typeErp.IsBling) {
                    blingService.getAllWarehouses(id, function (response) {
                        if (response.success && response.data.isSuccess) {
                            $scope.blingWarehouses = response.data.value;
                        }
                        else {
                            util.showError(response);
                        }
                    });
                    blingService.getAllCategories(id, function (response) {
                        if (response.success && response.data.isSuccess) {
                            $scope.blingCategories = response.data.value;
                        }
                        else {
                            util.showError(response);
                        }
                    });
                    blingService.getAllSituacoes(id, function (response) {
                        if (response.success && response.data.isSuccess) {
                            $scope.blingSituacoes = response.data.value;
                        }
                        else {
                            util.showError(response);
                        }
                    });
                }
                if ($scope.typeEcommerce.IsShopify) {
                    shopifyService.getAllWarehouses(id, function (response) {
                        if (response.success && response.data.isSuccess) {
                            $scope.shopifyWarehouses = response.data.value;
                        }
                        else {
                            util.showError(response);
                        }
                    });
                }
            }
            else {
                util.showError(response);
            }
        });
    }

    $scope.initializeTenant = function () {
        $scope.tenant = {
            shopifyData: {
                daysToDelivery: 0,
                minOrderId: 0,
                shopifyApps: []
            },
            milleniumData: {
                logins: [],
                extraFieldConfigurations: []
            },
            locationMap: {
                locations: []
            },
            methodPayments: []
        }
    }

    $scope.addShopifyApp = function () {
        $scope.tenant.shopifyData.shopifyApps.push({});
    }

    $scope.removeShopifyApp = function (app) {
        $scope.tenant.shopifyData.shopifyApps.splice($scope.tenant.shopifyData.shopifyApps.indexOf(app), 1);
    }

    $scope.addFromToPayment = function () {      
        $scope.tenant.methodPayments.push({});        
    }

    $scope.removeFromToPayment = function (item) {
        $scope.tenant.methodPayments.splice($scope.tenant.methodPayments.indexOf(item), 1);
    }

    $scope.typeChanged = function () {
        if ($scope.typeErp.IsMillennium) {
            if (!$scope.tenant.milleniumData) {
                $scope.tenant.milleniumData = {};
            }
            if (!$scope.tenant.milleniumData.logins) {
                $scope.tenant.milleniumData.logins = [];
            }
            if ($scope.tenant.milleniumData.logins.length == 0) {
                $scope.tenant.milleniumData.logins.push({});
            }
            if ($scope.tenant.methodPayments.length == 0) {
                $scope.tenant.methodPayments.push({});
            }
            if (!$scope.tenant.milleniumData.excludedProductCharacters) {
                $scope.tenant.milleniumData.excludedProductCharacters = [];
            }

            if (!$scope.tenant.milleniumData.extraFieldConfigurations) {
                $scope.tenant.milleniumData.extraFieldConfigurations = [];
            }
        }
        else if ($scope.typeErp.IsOmie) {
            if (!$scope.tenant.omieData) {
                $scope.tenant.omieData = {};
            }

            if (!$scope.tenant.omieData.extraNotaFiscalEmails) {
                $scope.tenant.omieData.extraNotaFiscalEmails = [];
            }
            if (!$scope.tenant.omieData.variantCaracteristicas) {
                $scope.tenant.omieData.variantCaracteristicas = [];
            }
            if (!$scope.tenant.omieData.categoryCaracteristicas) {
                $scope.tenant.omieData.categoryCaracteristicas = [];
            }
            if (!$scope.tenant.omieData.freteMappings) {
                $scope.tenant.omieData.freteMappings = [];
            }
            if (!$scope.tenant.omieData.codigoParcelaMappings) {
                $scope.tenant.omieData.codigoParcelaMappings = [];
            }
        }

        $scope.initLocation();

    }

    $scope.initLocation = function () {
        if ($scope.tenant.multiLocation) {
            if (!$scope.tenant.locationMap.locations) {
                $scope.tenant.locationMap.locations = [];
            }

            if ($scope.tenant.locationMap.locations.length == 0) {
                $scope.tenant.locationMap.locations.push({});
            }
        }
        else if ($scope.typeErp.IsBling) {
            if (!$scope.tenant.blingData) {
                $scope.tenant.blingData = { apiBaseUrl: "https://bling.com.br" };
            }
        }
        else if ($scope.typeErp.IsPluggTo) {
            if (!$scope.tenant.pluggToData) {
                $scope.tenant.pluggToData = { apiBaseUrl: "https://plugg.to/" };
            }
        }
    }

    $scope.addMillenniumLogin = function () {
        $scope.tenant.milleniumData.logins.push({});
    }

    $scope.removeMillenniumLogin = function (app) {
        $scope.tenant.milleniumData.logins.splice($scope.tenant.milleniumData.logins.indexOf(app), 1);
    }
    $scope.removeLocation = function (app) {
        $scope.tenant.locationMap.locations.splice($scope.tenant.locationMap.locations.indexOf(app), 1);
    }
    $scope.addLocation = function () {
        $scope.tenant.locationMap.locations.push({});
    }

    $scope.addMillenniumExtraFieldConfiguration = function () {
        $scope.tenant.milleniumData.extraFieldConfigurations.push({});
    }

    $scope.removeMillenniumExtraFieldConfiguration = function (config) {
        $scope.tenant.milleniumData.extraFieldConfigurations.splice($scope.tenant.milleniumData.extraFieldConfigurations.indexOf(config), 1);
    }

    $scope.addOmieFreteMapping = function () {
        $scope.tenant.omieData.freteMappings.push({});
    }

    $scope.removeOmieFreteMapping = function (config) {
        $scope.tenant.omieData.freteMappings.splice($scope.tenant.omieData.freteMappings.indexOf(config), 1);
    }

    $scope.save = function () {
        if ($scope.tenantForm.$valid) {            
            $scope.isSending = true;
            $scope.cleanShowDiv(true);
           
            tenantService.saveTenant($scope.tenant, $scope.editMode, function (response) {
                $scope.isSending = false;
                if (response.success && response.data.isSuccess) {
                    alert('Tenant salvo com sucesso');
                    location.href = '/admin/dashboard/';
                }
                else {
                    util.showError(response);
                }
            });
        } else {
            console.log($scope.tenantForm.$error.required);
            alert('As propriedades obrigatorias nao foram preenchidas');
        }

    }

    $scope.postOrderShopify = function () {
        if ($scope.orderShopifyForm.$valid) {
            $scope.btnOrderShopify.isSending = true;
            $scope.btnOrderShopify.isSuccess = false;
            queueService.postOrderShopify($scope.tenant.id, $scope.orderShopifyId, function (response) {
                $scope.btnOrderShopify.isSending = false;
                if (response.success && response.data.isSuccess) {
                    $scope.btnOrderShopify.isSuccess = true;
                }
                else {
                    util.showError(response);
                }
            });
        }
    }

    $scope.postShopifyFilterDate = function () {
        if ($scope.orderShopifyFilterDateForm.$valid) {
            $scope.btnOrderShopifyFilterDate.isSending = true;
            $scope.btnOrderShopifyFilterDate.isSuccess = false;
            queueService.postShopifyFilterDate($scope.tenant.id, $scope.dateMin, $scope.dateMax, function (response) {
                $scope.btnOrderShopifyFilterDate.isSending = false;
                if (response.success && response.data.isSuccess) {
                    $scope.btnOrderShopifyFilterDate.isSuccess = true;
                }
                else {
                    util.showError(response);
                }
            });
        }
    }

    $scope.postAllCollectionsShopify = function () {
        if (confirm('tem certeza que deseja fazer carga FULL?')) {
            if ($scope.allCollectionsShopifyForm.$valid) {
                $scope.btnAllCollectionsShopify.isSending = true;
                $scope.btnAllCollectionsShopify.isSuccess = false;
                queueService.postAllCollectionsShopify($scope.tenant.id, function (response) {
                    $scope.btnAllCollectionsShopify.isSending = false;
                    if (response.success && response.data.isSuccess) {
                        $scope.btnAllCollectionsShopify.isSuccess = true;
                    }
                    else {
                        util.showError(response);
                    }
                });
            }
        }
    }

    $scope.postOrderSC = function () {
        if ($scope.orderSCForm.$valid) {
            $scope.btnOrderSC.isSending = true;
            $scope.btnOrderSC.isSuccess = false;
            queueService.postOrderSC($scope.tenant.id, $scope.orderSCNumber, function (response) {
                $scope.btnOrderSC.isSending = false;
                if (response.success && response.data.isSuccess) {
                    $scope.btnOrderSC.isSuccess = true;
                }
                else {
                    util.showError(response);
                }
            });
        }
    }

    $scope.postOrderMillennium = function () {
        if ($scope.orderMillenniumForm.$valid) {
            $scope.btnOrderMillennium.isSending = true;
            $scope.btnOrderMillennium.isSuccess = false;
            queueService.postOrderMillennium($scope.tenant.id, $scope.orderMillenniumId, function (response) {
                $scope.btnOrderMillennium.isSending = false;
                if (response.success && response.data.isSuccess) {
                    $scope.btnOrderMillennium.isSuccess = true;
                }
                else {
                    util.showError(response);
                }
            });
        }
    }

    $scope.postProductMillennium = function () {
        if ($scope.productMillenniumForm.$valid) {
            $scope.btnProductMillennium.isSending = true;
            $scope.btnProductMillennium.isSuccess = false;
            queueService.postProductMillennium($scope.tenant.id, $scope.productMillenniumId, function (response) {
                $scope.btnProductMillennium.isSending = false;
                if (response.success && response.data.isSuccess) {
                    $scope.btnProductMillennium.isSuccess = true;
                }
                else {
                    util.showError(response);
                }
            });
        }
    }

    $scope.postAllProductsMillennium = function () {
        if (!$scope.dateUpdate) {
            util.showError("Você deve escolher uma data");
            return
        }
        if (confirm('tem certeza que deseja fazer carga FULL?')) {
            if ($scope.allProductsMillenniumForm.$valid) {
                $scope.btnAllProductsMillennium.isSending = true;
                $scope.btnAllProductsMillennium.isSuccess = false;
                tenantService.postClearMillenniumTransId($scope.tenant.id, 'ListaVitrine', $scope.dateUpdate, function (response) {
                    $scope.btnAllProductsMillennium.isSending = false;
                    if (response.success && response.data.isSuccess) {
                        tenantService.postClearMillenniumTransId($scope.tenant.id, 'PrecoDeTabela', $scope.dateUpdate, function (response) {
                            $scope.btnAllProductsMillennium.isSending = false;
                            if (response.success && response.data.isSuccess) {
                                tenantService.postClearMillenniumTransId($scope.tenant.id, 'SaldoDeEstoque', $scope.dateUpdate, function (response) {
                                    $scope.btnAllProductsMillennium.isSending = false;
                                    if (response.success && response.data.isSuccess) {
                                        $scope.btnAllProductsMillennium.isSuccess = true;
                                    }
                                    else {
                                        util.showError(response);
                                    }
                                });
                            }
                            else {
                                util.showError(response);
                            }
                        });
                    }
                    else {
                        util.showError(response);
                    }
                });
            }
        }
    }

    $scope.postOrderNexaas = function () {
        if ($scope.orderNexaasForm.$valid) {
            $scope.btnOrderNexaas.isSending = true;
            $scope.btnOrderNexaas.isSuccess = false;
            queueService.postOrderNexaas($scope.tenant.id, $scope.orderNexaasId, function (response) {
                $scope.btnOrderNexaas.isSending = false;
                if (response.success && response.data.isSuccess) {
                    $scope.btnOrderNexaas.isSuccess = true;
                }
                else {
                    util.showError(response);
                }
            });
        }
    }

    $scope.postProductNexaas = function () {
        if ($scope.productNexaasForm.$valid) {
            $scope.btnProductNexaas.isSending = true;
            $scope.btnProductNexaas.isSuccess = false;
            queueService.postProductNexaas($scope.tenant.id, $scope.productNexaasId, false, function (response) {
                $scope.btnProductNexaas.isSending = false;
                if (response.success && response.data.isSuccess) {
                    $scope.btnProductNexaas.isSuccess = true;
                }
                else {
                    util.showError(response);
                }
            });
        }
    }

    $scope.postAllProductsNexaas = function () {
        if (confirm('tem certeza que deseja fazer carga FULL?')) {
            if ($scope.allProductsNexaasForm.$valid) {
                $scope.btnAllProductsNexaas.isSending = true;
                $scope.btnAllProductsNexaas.isSuccess = false;
                queueService.postProductNexaas($scope.tenant.id, 0, true, function (response) {
                    $scope.btnAllProductsNexaas.isSending = false;
                    if (response.success && response.data.isSuccess) {
                        $scope.btnAllProductsNexaas.isSuccess = true;
                    }
                    else {
                        util.showError(response);
                    }
                });
            }
        }
    }

    $scope.postOrderOmie = function () {
        if ($scope.orderOmieForm.$valid) {
            $scope.btnOrderOmie.isSending = true;
            $scope.btnOrderOmie.isSuccess = false;
            queueService.postOrderOmie($scope.tenant.id, $scope.orderOmieId, function (response) {
                $scope.btnOrderOmie.isSending = false;
                if (response.success && response.data.isSuccess) {
                    $scope.btnOrderOmie.isSuccess = true;
                }
                else {
                    util.showError(response);
                }
            });
        }
    }

    $scope.postProductOmie = function () {
        if ($scope.productOmieForm.$valid) {
            $scope.btnProductOmie.isSending = true;
            $scope.btnProductOmie.isSuccess = false;
            queueService.postProductOmie($scope.tenant.id, $scope.productOmieId, false, function (response) {
                $scope.btnProductOmie.isSending = false;
                if (response.success && response.data.isSuccess) {
                    $scope.btnProductOmie.isSuccess = true;
                }
                else {
                    util.showError(response);
                }
            });
        }
    }

    $scope.postAllProductsOmie = function () {
        if (confirm('tem certeza que deseja fazer carga FULL?')) {
            if ($scope.allProductsOmieForm.$valid) {
                $scope.btnAllProductsOmie.isSending = true;
                $scope.btnAllProductsOmie.isSuccess = false;
                queueService.postProductOmie($scope.tenant.id, 0, true, function (response) {
                    $scope.btnAllProductsOmie.isSending = false;
                    if (response.success && response.data.isSuccess) {
                        $scope.btnAllProductsOmie.isSuccess = true;
                    }
                    else {
                        util.showError(response);
                    }
                });
            }
        }
    }

    $scope.postOrderBling = function () {
        if ($scope.orderBlingForm.$valid) {
            $scope.btnOrderBling.isSending = true;
            $scope.btnOrderBling.isSuccess = false;
            queueService.postOrderBling($scope.tenant.id, $scope.orderBlingId, function (response) {
                $scope.btnOrderBling.isSending = false;
                if (response.success && response.data.isSuccess) {
                    $scope.btnOrderBling.isSuccess = true;
                }
                else {
                    util.showError(response);
                }
            });
        }
    }

    $scope.postProductBling = function () {
        if ($scope.productBlingForm.$valid) {
            $scope.btnProductBling.isSending = true;
            $scope.btnProductBling.isSuccess = false;
            queueService.postProductBling($scope.tenant.id, $scope.productBlingId, false, function (response) {
                $scope.btnProductBling.isSending = false;
                if (response.success && response.data.isSuccess) {
                    $scope.btnProductBling.isSuccess = true;
                }
                else {
                    util.showError(response);
                }
            });
        }
    }

    $scope.postAllProductsBling = function () {
        if (confirm('tem certeza que deseja fazer carga FULL?')) {
            if ($scope.allProductsBlingForm.$valid) {
                $scope.btnAllProductsBling.isSending = true;
                $scope.btnAllProductsBling.isSuccess = false;
                queueService.postProductBling($scope.tenant.id, "", true, function (response) {
                    $scope.btnAllProductsBling.isSending = false;
                    if (response.success && response.data.isSuccess) {
                        $scope.btnAllProductsBling.isSuccess = true;
                    }
                    else {
                        util.showError(response);
                    }
                });
            }
        }
    }

    $scope.postOrderPluggTo = function () {
        if ($scope.orderPluggToForm.$valid) {
            $scope.btnOrderPluggTo.isSending = true;
            $scope.btnOrderPluggTo.isSuccess = false;
            queueService.postOrderPluggTo($scope.tenant.id, $scope.orderPluggToId, function (response) {
                $scope.btnOrderPluggTo.isSending = false;
                if (response.success && response.data.isSuccess) {
                    $scope.btnOrderPluggTo.isSuccess = true;
                }
                else {
                    util.showError(response);
                }
            });
        }
    }

    $scope.postProductPluggTo = function () {
        if ($scope.productPluggToForm.$valid) {
            $scope.btnProductPluggTo.isSending = true;
            $scope.btnProductPluggTo.isSuccess = false;
            queueService.postProductPluggTo($scope.tenant.id, $scope.productPluggToId, $scope.productPluggToSku, false, function (response) {
                $scope.btnProductPluggTo.isSending = false;
                if (response.success && response.data.isSuccess) {
                    $scope.btnProductPluggTo.isSuccess = true;
                }
                else {
                    util.showError(response);
                }
            });
        }
    }

    $scope.postAllProductsPluggTo = function () {
        if (confirm('tem certeza que deseja fazer carga FULL?')) {
            if ($scope.allProductsOmieForm.$valid) {
                $scope.btnAllProductsPluggTo.isSending = true;
                $scope.btnAllProductsPluggTo.isSuccess = false;
                queueService.postProductPluggTo($scope.tenant.id, "", "", true, function (response) {
                    $scope.btnAllProductsPluggTo.isSending = false;
                    if (response.success && response.data.isSuccess) {
                        $scope.btnAllProductsPluggTo.isSuccess = true;
                    }
                    else {
                        util.showError(response);
                    }
                });
            }
        }
    }

    $scope.postOrderAliExpress = function () {
        if ($scope.orderAliExpressForm.$valid) {
            $scope.btnOrderAliExpress.isSending = true;
            $scope.btnOrderAliExpress.isSuccess = false;
            queueService.postOrderAliExpress($scope.tenant.id, $scope.orderAliExpressId, function (response) {
                $scope.btnOrderAliExpress.isSending = false;
                if (response.success && response.data.isSuccess) {
                    $scope.btnOrderAliExpress.isSuccess = true;
                }
                else {
                    util.showError(response);
                }
            });
        }
    }

    $scope.getQueuesCount = function () {
        $scope.btnQueuesCount.isSending = true;
        $scope.btnQueuesCount.isSuccess = false;
        queueService.getQueuesCount($scope.tenant.id, function (response) {
            $scope.btnQueuesCount.isSending = false;
            if (response.success && response.data.isSuccess) {
                $scope.btnQueuesCount.isSuccess = true;
                $scope.queuesCount = response.data.value;
            }
            else {
                util.showError(response);
            }
        });
    }

    $scope.postUpdateTrackingPier8 = function () {
        if ($scope.pier8UpdateTrackingForm.$valid) {
            $scope.btnUpdateTrackingPier8.isSending = true;
            $scope.btnUpdateTrackingPier8.isSuccess = false;
            queueService.postUpdateTrackingPier8($scope.tenant.id, $scope.idOrdersShopify, function (response) {
                $scope.btnUpdateTrackingPier8.isSending = false;
                if (response.success && response.data.isSuccess) {
                    $scope.btnUpdateTrackingPier8.isSuccess = true;
                }
                else {
                    util.showError(response);
                }
            });
        }
    }
    $scope.setNameSkuEnabled = function () {

        let obj = document.getElementById("divNomeSKU");
        let check = document.getElementById("objNameSkuEnabled");
        if (check.checked == true)
            obj.style.display = "block";
        else
            obj.style.display = "none";
    }

    $scope.setEnableExtraPaymentInformation = function () {
        const obj = document.getElementById("divEnableExtraPaymentInformation");
        const check = document.getElementById("millenium_enableExtraPaymentInformation");
        if (check.checked == true)
            obj.style.display = "block";
        else
            obj.style.display = "none";
    }

    $scope.setEnableMaxVariants = function () {
        const obj = document.getElementById("divEnableMaxVariants");
        const check = document.getElementById("shopify_EnableMaxVariants");

        if (check.checked == true)
            obj.style.display = "block";
        else
            obj.style.display = "none";
    }

    $scope.loadAllCodigoParcela = function () {
        $scope.omieCodigoParcelas = [];
        $scope.omieCodigoParcelas.push({ codigo_parcela: "000", descricao: "A Vista" }, { codigo_parcela: "A03", descricao: "Para 3 dias" }, { codigo_parcela: "A05", descricao: "Para 5 dias" }, { codigo_parcela: "A07", descricao: "Para 7 dias" }, { codigo_parcela: "A08", descricao: "Para 8 dias" }, { codigo_parcela: "A09", descricao: "Para 9 dias" }, { codigo_parcela: "A10", descricao: "Para 10 dias" }, { codigo_parcela: "A13", descricao: "Para 13 dias" }, { codigo_parcela: "A14", descricao: "Para 14 dias" }, { codigo_parcela: "A15", descricao: "Para 15 dias" }, { codigo_parcela: "A17", descricao: "Para 17 dias" }, { codigo_parcela: "A20", descricao: "Para 20 dias" },
            { codigo_parcela: "A21", descricao: "Para 21 dias" }, { codigo_parcela: "A25", descricao: "Para 25 dias" }, { codigo_parcela: "A26", descricao: "Para 26 dias" }, { codigo_parcela: "A28", descricao: "Para 28 dias" }, { codigo_parcela: "A35", descricao: "Para 35 dias" }, { codigo_parcela: "A36", descricao: "Para 36 dias" }, { codigo_parcela: "A40", descricao: "Para 40 dias" }, { codigo_parcela: "A42", descricao: "Para 42 dias" },
            { codigo_parcela: "A45", descricao: "Para 45 dias" }, { codigo_parcela: "A50", descricao: "Para 50 dias" }, { codigo_parcela: "A56", descricao: "Para 56 dias" }, { codigo_parcela: "A60", descricao: "Para 60 dias" }, { codigo_parcela: "A70", descricao: "Para 70 dias" }, { codigo_parcela: "A75", descricao: "Para 75 dias" }, { codigo_parcela: "A90", descricao: "Para 90 dias" }, { codigo_parcela: "A98", descricao: "Para 98 dias" }, { codigo_parcela: "B20", descricao: "Para 120 dias" },
            { codigo_parcela: "001", descricao: "1 Parcela (para 30 dias)" }, { codigo_parcela: "002", descricao: "2 Parcelas" }, { codigo_parcela: "003", descricao: "3 Parcelas" }, { codigo_parcela: "004", descricao: "4 Parcelas" }, { codigo_parcela: "005", descricao: "5 Parcelas" }, { codigo_parcela: "006", descricao: "6 Parcelas" }, { codigo_parcela: "007", descricao: "7 Parcelas" }, { codigo_parcela: "010", descricao: "10 Parcelas" }, { codigo_parcela: "012", descricao: "12 Parcelas" }, { codigo_parcela: "024", descricao: "24 Parcelas" }, { codigo_parcela: "036", descricao: "36 Parcelas" }, { codigo_parcela: "048", descricao: "48 Parcelas" }, { codigo_parcela: "S01", descricao: "30/60" }, { codigo_parcela: "S02", descricao: "45/60" },
            { codigo_parcela: "S03", descricao: "21/28/35" }, { codigo_parcela: "S04", descricao: "21/28/35/42" }, { codigo_parcela: "S05", descricao: "28/35/42" }, { codigo_parcela: "S06", descricao: "28/35/42/49" }, { codigo_parcela: "S07", descricao: "30/45/60/75/90" }, { codigo_parcela: "S08", descricao: "25/56" }, { codigo_parcela: "S09", descricao: "30/45" }, { codigo_parcela: "S10", descricao: "28/56" }, { codigo_parcela: "S11", descricao: "10/30/60" }, { codigo_parcela: "S12", descricao: "15/30/60" }, { codigo_parcela: "S13", descricao: "28/35" }, { codigo_parcela: "S14", descricao: "7/14/21" }, { codigo_parcela: "S15", descricao: "10/30/60/90" }, { codigo_parcela: "S16", descricao: "60/90/120" }, { codigo_parcela: "S17", descricao: "45/60/90" },
            { codigo_parcela: "S18", descricao: "30/60/90" }, { codigo_parcela: "S19", descricao: "14/21" }, { codigo_parcela: "S20", descricao: "7/14" }, { codigo_parcela: "S21", descricao: "14/21/28" }, { codigo_parcela: "S22", descricao: "45/75" }, { codigo_parcela: "S23", descricao: "30/45/60" }, { codigo_parcela: "S24", descricao: "3/20/40" }, { codigo_parcela: "S25", descricao: "30/60/90/120" }, { codigo_parcela: "S26", descricao: "21/28" }, { codigo_parcela: "S27", descricao: "a Vista/15" }, { codigo_parcela: "S28", descricao: "a Vista/30" }, { codigo_parcela: "S29", descricao: "a Vista/30/60" }, { codigo_parcela: "S30", descricao: "a Vista/30/60/90" }, { codigo_parcela: "S31", descricao: "a Vista/30/60/90/120/150" }, { codigo_parcela: "S41", descricao: "28/42/56" }, { codigo_parcela: "S32", descricao: "15/45/75" },
            { codigo_parcela: "S33", descricao: "14/28/42" }, { codigo_parcela: "S34", descricao: "14/21/28/35/42" }, { codigo_parcela: "S35", descricao: "30/42/54/66/78/90" }, { codigo_parcela: "S36", descricao: "14/21/28/35" }, { codigo_parcela: "S37", descricao: "28/42" }, { codigo_parcela: "S38", descricao: "30/45/60" }, { codigo_parcela: "S39", descricao: "35/42/49/56" }, { codigo_parcela: "S40", descricao: "28/42/56/70" }, { codigo_parcela: "S42", descricao: "30/40/50/60" }, { codigo_parcela: "S43", descricao: "30/50/70/90" }, { codigo_parcela: "S44", descricao: "14/28" }, { codigo_parcela: "S45", descricao: "45/60/75/90" }, { codigo_parcela: "S46", descricao: "a Vista/30/60/90/120" }, { codigo_parcela: "S47", descricao: "a vista/20/40/60" },
            { codigo_parcela: "S48", descricao: "21/42" }, { codigo_parcela: "S49", descricao: "15/30/45" }, { codigo_parcela: "S50", descricao: "14/42" }, { codigo_parcela: "S51", descricao: "21/35" }, { codigo_parcela: "S52", descricao: "28/56/84" }, { codigo_parcela: "S53", descricao: "28/42/56/70/84" }, { codigo_parcela: "S54", descricao: "a Vista/30/45" }, { codigo_parcela: "S55", descricao: "21/45" }, { codigo_parcela: "S56", descricao: "a Vista/28" }, { codigo_parcela: "S57", descricao: "a Vista/60/90" }, { codigo_parcela: "S58", descricao: "35/45/55" }, { codigo_parcela: "S59", descricao: "28/35/42/56" }, { codigo_parcela: "S60", descricao: "30/45/60/75" }, { codigo_parcela: "S61", descricao: "28/35/42/49/56" }, { codigo_parcela: "S62", descricao: "40/70/100" }, { codigo_parcela: "S63", descricao: "42/56" }, { codigo_parcela: "S64", descricao: "a Vista/28/35" }, { codigo_parcela: "S65", descricao: "35/42" },
            { codigo_parcela: "S66", descricao: "20/40" }, { codigo_parcela: "S67", descricao: "a Vista/28/35/42" }, { codigo_parcela: "S68", descricao: "a vista/20/40/60/80" }, { codigo_parcela: "S69", descricao: "a vista/20/40/60/80/100" }, { codigo_parcela: "S70", descricao: "a vista/20/40/60/80/100/120" }, { codigo_parcela: "S71", descricao: "a vista/30/60/90/120/150/180/210/240/270/300/330/360" }, { codigo_parcela: "S72", descricao: "a vista/30/60/90/120/150/180/210/240/270/300" }, { codigo_parcela: "S73", descricao: "28/56/84/112" }, { codigo_parcela: "S74", descricao: "14/28/42/56" },
            { codigo_parcela: "S75", descricao: "28/42/56/70/84/98" }, { codigo_parcela: "S76", descricao: "15/30/45/60/75" }, { codigo_parcela: "S77", descricao: "a Vista/15/30" }, { codigo_parcela: "S78", descricao: "a Vista/20/40" }, { codigo_parcela: "S79", descricao: "35/42/56" }, { codigo_parcela: "S80", descricao: "10/30/60/90/120" }, { codigo_parcela: "S81", descricao: "15/30/45/75/90/105/120" }, { codigo_parcela: "S82", descricao: "30/45/75/90/105/120" }, { codigo_parcela: "S83", descricao: "42/49/56" }, { codigo_parcela: "S84", descricao: "35/42/49" }, { codigo_parcela: "S85", descricao: "a Vista/60/90/120/150" }, { codigo_parcela: "S86", descricao: "a Vista/30/45/60" }, { codigo_parcela: "S87", descricao: "20/40/60/80/100" }, { codigo_parcela: "S89", descricao: "15/30" },
            { codigo_parcela: "S90", descricao: "10/30/50" }, { codigo_parcela: "S91", descricao: "45/52/60" }, { codigo_parcela: "S92", descricao: "10/30" }, { codigo_parcela: "S93", descricao: "20/30/40/50/60/70" }, { codigo_parcela: "S94", descricao: "45/52/59/66" }, { codigo_parcela: "S95", descricao: "15/30/45/60" }, { codigo_parcela: "S96", descricao: "40/50" }, { codigo_parcela: "S97", descricao: "21/42/56" }, { codigo_parcela: "S98", descricao: "a vista/60/90/120/150/180/240/300/330" },
            { codigo_parcela: "S99", descricao: "a vista/180" }, { codigo_parcela: "T01", descricao: "30/40/50" }, { codigo_parcela: "T02", descricao: "21/28/35/42/49" }, { codigo_parcela: "T03", descricao: "30/37/45/60" }, { codigo_parcela: "T04", descricao: "a vista/30/60/90/120/150/180/210/240/270" }, { codigo_parcela: "T05", descricao: "a vista/30/60/90/120/150/180/210" }, { codigo_parcela: "T06", descricao: "30/60/90/120/150/180/210/240" }, { codigo_parcela: "T07", descricao: "56/84/112" }, { codigo_parcela: "T08", descricao: "15/30/45/60/75/90/105" }, { codigo_parcela: "T09", descricao: "21/35/42" }, { codigo_parcela: "T10", descricao: "35/49" }, { codigo_parcela: "T11", descricao: "30/45/60/75/90/105/120" },
            { codigo_parcela: "T12", descricao: "45/75/105/135" }, { codigo_parcela: "T13", descricao: "35/60/75" }, { codigo_parcela: "T14", descricao: "10/40/70/100/130" }, { codigo_parcela: "T15", descricao: "45/60/75" }, { codigo_parcela: "T16", descricao: "40/55/70" }, { codigo_parcela: "T17", descricao: "40/70" }, { codigo_parcela: "T18", descricao: "20/40/60" }, { codigo_parcela: "T19", descricao: "60/90" }, { codigo_parcela: "T20", descricao: "25/35/45/55" }, { codigo_parcela: "T21", descricao: "15/45" }, { codigo_parcela: "T22", descricao: "7/30/45" }, { codigo_parcela: "T23", descricao: "7/30/60" }, { codigo_parcela: "T24", descricao: "64/71" }, { codigo_parcela: "T25", descricao: "20/30/40" }
        );
        console.log($scope.omieCodigoParcelas)

    }

    $scope.addOmieCodigoParcelaMapping = function () {
        if ($scope.tenant.omieData.enableParcelaUnica) {
            if ($scope.tenant.omieData.codigoParcelaMappings.length >= 1)
                return;
        }
        $scope.tenant.omieData.codigoParcelaMappings.push({});
    }

    $scope.removeOmieCodigoParcelaMapping = function (config) {
        $scope.tenant.omieData.codigoParcelaMappings.splice($scope.tenant.omieData.freteMappings.indexOf(config), 1);
    }

    $scope.checkParcelaUnica = function () {
        if ($scope.tenant.omieData.codigoParcelaMappings.length > 1)
            while ($scope.tenant.omieData.codigoParcelaMappings.length > 1) {
                $scope.tenant.omieData.codigoParcelaMappings.pop();
            }
    }
}])