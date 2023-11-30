samuraiApp.controller('tenantDashboardController', ['$scope', '$http', 'DTOptionsBuilder', 'DTColumnBuilder', function ($scope, $http, DTOptionsBuilder, DTColumnBuilder) {
    $scope.status = 1;
    $scope.shop = 0;
    $scope.erp = 0;

    $scope.urlSource = window.config.webapi.url + '/Tenant';

    loadDataTable($scope.urlSource + '?status=true&Shop=0&ERP=0');

    function loadDataTable(url) {
        $scope.dtOptions = DTOptionsBuilder
            .newOptions()
            .withOption('ajax', {
                url: url,
                type: 'GET'
            })
            .withOption('processing', true)
            .withOption('serverSide', true)
            .withOption('searchDelay', 500)
            .withOption('lengthMenu', [
                [10, 25, 50, 100, -1],
                [10, 25, 50, 100, 'Todas'],
            ])
            .withDataProp('data')
            .withPaginationType('full_numbers')
            .withLanguageSource('/lib/datatables/js/Portuguese-Brasil.json')

        const btnNovaLoja = {
            text: 'Nova Loja',
            key: '1',
            action: function (e, dt, node, config) {
                location.href = '/admin/tenant/';
            },

        };

        const btnExportarCsv = {
            extend: 'csvHtml5',
            text: 'Exportar CSV',
            filename: 'lojas',
            charset: 'UTF-8',
            fieldSeparator: ';',
            bom: true,
            exportOptions: {
                columns: [1, 2, 3, 4, 5]
            }
        };

        if (readOnly()) {
            $scope.dtOptions.withButtons([
                btnExportarCsv
            ]);
        }

        else {
            $scope.dtOptions.withButtons([
                btnNovaLoja, btnExportarCsv
            ]);
        }

        $scope.dtColumns = [
            DTColumnBuilder.newColumn('id').withTitle('ID'),
            DTColumnBuilder.newColumn('storeName').withTitle('Loja'),
            DTColumnBuilder.newColumn('type').withTitle('Tipo de Integração').renderWith(function (data, type, full) {
                return `${data} - ${full.integrationType}`;
            }),
            DTColumnBuilder.newColumn('status').withTitle('Status').renderWith(function (data, type, full) {
                return data == true ? 'Ativa' : 'Inativa';
            }),
            DTColumnBuilder.newColumn('creationDate').withTitle('Data de Ativação').renderWith(function (data, type, full) {
                return data ? new Date(data).toLocaleDateString() : null;
            }),
            DTColumnBuilder.newColumn('deactivatedDate').withTitle('Data de Desativação').renderWith(function (data, type, full) {
                return data ? new Date(data).toLocaleDateString() : null;
            }),
            DTColumnBuilder.newColumn('id').withTitle('').notSortable().renderWith(
                function (data, type, full) {
                    return '<button class="btnEdit" onclick="location.href=\'/admin/tenant/' + data + '\'">'
                        + (readOnly() ? 'Ver' : '<i class="bi bi-pencil-square"></i>') + '</button>';
                })
        ];   

    }

    $scope.changeStatus = function () {
        const status = $scope.status;

        if (status == -1) {
            loadDataTable(`${$scope.urlSource}?Shop=${$scope.shop}&ERP=${$scope.erp}`);
        } else if (status == 0) {
            loadDataTable(`${$scope.urlSource}?status=false&Shop=${$scope.shop}&ERP=${$scope.erp}`);
        } else {
            loadDataTable(`${$scope.urlSource}?status=true&Shop=${$scope.shop}&ERP=${$scope.erp}`);
        }
    };

    $scope.changeIntegrationType = function () {
        const status = $scope.status;

        if (status == -1) {
            loadDataTable(`${$scope.urlSource}?Shop=${$scope.shop}&ERP=${$scope.erp}`);
        } else if (status == 0) {
            loadDataTable(`${$scope.urlSource}?status=false&Shop=${$scope.shop}&ERP=${$scope.erp}`);
        } else {
            loadDataTable(`${$scope.urlSource}?status=true&Shop=${$scope.shop}&ERP=${$scope.erp}`);
        }
    };


    $scope.typeOptions = [
        { name: 'Ativas', value: 1 },
        { name: 'Inativas', value: 0 },
        { name: 'Todos', value: -1 }
    ];

    $scope.integrationsShop = [
        { name: 'Todos', value: 0 },
        { name: 'Shopify', value: 1 },
        { name: 'SellerCenter', value: 2 },
        { name: 'Tray', value: 3 },
    ];

    $scope.integrationsERP = [
        { name: 'Todos', value: 0 },
        { name: 'Millennium', value: 1 },
        { name: 'Nexaas', value: 2 },
        { name: 'Omie', value: 3 },
        { name: 'Bling', value: 4 },
        { name: 'PluggTo', value: 5 },
        { name: 'AliExpress', value: 6 },
    ];

}])

const icon = `<svg xmlns="http://www.w3.org/2000/svg" width="40" height="40" fill="currentColor" class="bi bi-list-columns-reverse" viewBox="0 0 16 16">
  <path fill-rule="evenodd" d="M0 .5A.5.5 0 0 1 .5 0h2a.5.5 0 0 1 0 1h-2A.5.5 0 0 1 0 .5Zm4 0a.5.5 0 0 1 .5-.5h10a.5.5 0 0 1 0 1h-10A.5.5 0 0 1 4 .5Zm-4 2A.5.5 0 0 1 .5 2h2a.5.5 0 0 1 0 1h-2a.5.5 0 0 1-.5-.5Zm4 0a.5.5 0 0 1 .5-.5h9a.5.5 0 0 1 0 1h-9a.5.5 0 0 1-.5-.5Zm-4 2A.5.5 0 0 1 .5 4h2a.5.5 0 0 1 0 1h-2a.5.5 0 0 1-.5-.5Zm4 0a.5.5 0 0 1 .5-.5h11a.5.5 0 0 1 0 1h-11a.5.5 0 0 1-.5-.5Zm-4 2A.5.5 0 0 1 .5 6h2a.5.5 0 0 1 0 1h-2a.5.5 0 0 1-.5-.5Zm4 0a.5.5 0 0 1 .5-.5h8a.5.5 0 0 1 0 1h-8a.5.5 0 0 1-.5-.5Zm-4 2A.5.5 0 0 1 .5 8h2a.5.5 0 0 1 0 1h-2a.5.5 0 0 1-.5-.5Zm4 0a.5.5 0 0 1 .5-.5h8a.5.5 0 0 1 0 1h-8a.5.5 0 0 1-.5-.5Zm-4 2a.5.5 0 0 1 .5-.5h2a.5.5 0 0 1 0 1h-2a.5.5 0 0 1-.5-.5Zm4 0a.5.5 0 0 1 .5-.5h10a.5.5 0 0 1 0 1h-10a.5.5 0 0 1-.5-.5Zm-4 2a.5.5 0 0 1 .5-.5h2a.5.5 0 0 1 0 1h-2a.5.5 0 0 1-.5-.5Zm4 0a.5.5 0 0 1 .5-.5h6a.5.5 0 0 1 0 1h-6a.5.5 0 0 1-.5-.5Zm-4 2a.5.5 0 0 1 .5-.5h2a.5.5 0 0 1 0 1h-2a.5.5 0 0 1-.5-.5Zm4 0a.5.5 0 0 1 .5-.5h11a.5.5 0 0 1 0 1h-11a.5.5 0 0 1-.5-.5Z"/>
</svg>`;


window.onload = () => {
    //document.querySelector('#dashboardlIcon').innerHTML = icon;
    //const oldDivFilter = document.getElementById('DataTables_Table_0_filter');
    //const oldInputSearch = oldDivFilter.getElementsByTagName('input')[0];
    //oldInputSearch.classList.add('form-control');

    //const resultsForPage = document.getElementById('DataTables_Table_0_length').getElementsByTagName('select')[0];
    //resultsForPage.classList.add('form-control');
}
