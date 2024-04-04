vdcApp.controller('integrationErrorsController', ['$scope', 'DTOptionsBuilder', 'DTColumnBuilder', function ($scope, DTOptionsBuilder, DTColumnBuilder) {    
    $scope.dtOptions = DTOptionsBuilder.fromSource(window.config.webapi.url + '/IntegrationError/GetAll')
        .withDataProp('value')
        .withPaginationType('full_numbers')
        .withLanguageSource('/lib/datatables/js/Portuguese-Brasil.json');

    $scope.dtColumns = [
        DTColumnBuilder.newColumn('tag').withTitle('Tag'),
        DTColumnBuilder.newColumn('message').withTitle('Mensagem de retorno'),
        DTColumnBuilder.newColumn('description').withTitle('Descrição'),
    ];    
}])