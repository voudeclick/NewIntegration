vdcApp.controller('usersController', ['$scope', '$compile', 'userService', 'DTOptionsBuilder', 'DTColumnBuilder', function ($scope, $compile, userService, DTOptionsBuilder, DTColumnBuilder) {
    $scope.indexRoles=0;
    $scope.roles;
    $scope.dtOptions = DTOptionsBuilder.fromSource(window.config.webapi.url + '/User')
        .withDataProp('value')
        .withPaginationType('full_numbers')
        .withLanguageSource('/lib/datatables/js/Portuguese-Brasil.json');

    $scope.dtColumns = [
        DTColumnBuilder.newColumn('id').withTitle('ID'),
        DTColumnBuilder.newColumn('userName').withTitle('Nome'),
        DTColumnBuilder.newColumn('email').withTitle('E-mail'),
        DTColumnBuilder.newColumn('roles').withTitle('Papéis').renderWith(function (data, type, full) {

            var checks='';

            data.forEach((role) => {
                const roleCheck = { userId: full.id, id: role.id, name: role.name, selected: role.selected };

                if (!$scope.roles) {
                    $scope.roles = {
                        0: roleCheck
                    };
                } else {
                    $scope.roles[$scope.indexRoles] = roleCheck;
                }
                
                checks += `<div class="form-check form-check-inline"><label class="form-check-label">
                    <input class="form-check-input" type='checkbox'ng-change="changeRole(${$scope.indexRoles})" ng-model="roles[${$scope.indexRoles}].selected"/>
                    ${role.name}</label></div>`;

                $scope.indexRoles++;
            });            

            return checks;
        }).withOption('createdCell', function (td, cellData, rowData, row, col) {
            $compile(td)($scope); 
        })
    ];

    $scope.changeRole = function (i) {

        const role = $scope.roles[i];
        Swal.showLoading();

        if (role.selected) {
            
            userService.addRole(role, function () {
                Swal.hideLoading();
                Swal.fire({
                    position: 'top-end',
                    icon: 'success',
                    title: 'Papel adicionado com sucesso!',
                    showConfirmButton: false,
                    toast: true,
                    timer: 1500
                });
            });
        } else {
            userService.removeRole(role, function () {
                Swal.hideLoading();
                Swal.fire({
                    position: 'top-end',
                    icon: 'success',
                    title: 'Papel removido com sucesso!',
                    showConfirmButton: false,
                    toast: true,
                    timer: 1500
                });
            });
        }        
    }
}])