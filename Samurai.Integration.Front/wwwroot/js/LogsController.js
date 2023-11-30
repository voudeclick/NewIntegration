/*let webjob = 0;*/
let idTenant = 0;
const iconRefresh = `<svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" fill="currentColor" class="bi bi-arrow-clockwise" viewBox="0 0 16 16">
                              <path fill-rule="evenodd" d="M8 3a5 5 0 1 0 4.546 2.914.5.5 0 0 1 .908-.417A6 6 0 1 1 8 2v1z"/>
                              <path d="M8 4.466V.534a.25.25 0 0 1 .41-.192l2.36 1.966c.12.1.12.284 0 .384L8.41 4.658A.25.25 0 0 1 8 4.466z"/>
                          </svg>`;
const iconFile = `<svg xmlns="http://www.w3.org/2000/svg" fill="currentColor" class="bi bi-file-earmark-text" viewBox="0 0 16 16">
  <path d="M5.5 7a.5.5 0 0 0 0 1h5a.5.5 0 0 0 0-1h-5zM5 9.5a.5.5 0 0 1 .5-.5h5a.5.5 0 0 1 0 1h-5a.5.5 0 0 1-.5-.5zm0 2a.5.5 0 0 1 .5-.5h2a.5.5 0 0 1 0 1h-2a.5.5 0 0 1-.5-.5z"/>
  <path d="M9.5 0H4a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h8a2 2 0 0 0 2-2V4.5L9.5 0zm0 1v2A1.5 1.5 0 0 0 11 4.5h2V14a1 1 0 0 1-1 1H4a1 1 0 0 1-1-1V2a1 1 0 0 1 1-1h5.5z"/>
</svg>`


const loadTenants = async () => {
    document.getElementById("pagination").setAttribute("hidden", true);
    const endPoint = `/Tenant/GetTenantForLogs`;
    const resp = await get(endPoint);
    const select = document.getElementById('select');
    resp.map((item) => {
        const option = document.createElement('option');
        option.value = item.id;
        option.innerHTML = `<strong>${item.id}</strong> - ${item.storeName}`;
        select.appendChild(option);
    });

    if (document.getElementById("load").classList.contains('load'))
        document.getElementById("load").classList.remove("load");
    return;
}

const selectWebjob = (event) => {
    webjob = event.target.value;
}

const changeTenantId = async (event) => {
    document.getElementById("load").classList.add("load");
    idTenant = event.target.value;
    if (idTenant == 0) { alert('Nenhum Tenant selecionado'); return; }
    const endPoint = `/Logs/GetList?TenantId=${idTenant}`;
    const resp = await get(endPoint);
    return renderTable(resp);
}



const renderTable = (data) => {
    document.getElementById("pagination").setAttribute("hidden", true);
    const table = document.getElementById('tableLogsBody');
    const totalPage = document.getElementById('total');
    const page = document.getElementById('page');

    if (data.items.length == 0) {
        table.innerHTML = "";
        const row = document.createElement("tr");
        row.innerHTML = '<td colspan="6" style="text-align:center;background-color:#c9c9c9;color:#FFFFFF"><strong>Sem Dados</strong></td>';
        table.appendChild(row);

        if (document.getElementById("load").classList.contains('load'))
            document.getElementById("load").classList.remove("load");
        return;
    }

    if (data.hasNextPage || data.hasPreviousPage)
        document.getElementById("pagination").removeAttribute("hidden");

    if (data.hasPreviousPage)
        document.getElementById("previous").removeAttribute("hidden");
    else
        document.getElementById("previous").setAttribute("hidden", true);

    if (data.hasNextPage)
        document.getElementById("next").removeAttribute("hidden");
    else
        document.getElementById("next").setAttribute("hidden", true);

    page.innerHTML = data.pageNumber;
    totalPage.innerHTML = data.pageCount;

    table.innerHTML = "";
    data.items.map((item) => {
        const row = document.createElement("tr");
        row.innerHTML = `<td>${item.logId}</td>          
                    <td>${item.type}</td>
                    <td>${item.webJob}</td>
                    <td>${item.creationDate}</td>
                    <td class="tdFile" onclick="showModalLogs('${item.logId}')">${iconFile}</td>
                    <td class="tdFile" onclick="showModalPayload('${item.logId}')">${iconFile}</td>`;
        table.appendChild(row);
    });

    if (document.getElementById("load").classList.contains('load'))
        document.getElementById("load").classList.remove("load");
    return;
}

const showModalLogs = async (id) => {

    document.getElementById("load").classList.add("load");
    const endPoint = `/Logs/GetLog?LogId=${id}`;
    const resp = await get(endPoint);

    document.getElementById('TitleModal').innerHTML = '<strong>LogError</strong>';
    document.getElementById('modalLogs').innerHTML = `<pre>Sem Dados</pre>`;

    if (document.getElementById("load").classList.contains('load'))
        document.getElementById("load").classList.remove("load");

    var error = resp.Error
    let Message = '';
    if (!(error && error.Message)) {
        document.getElementById("btn-modal").click();
        return;
    }

    try {
        Message = resp.Error.Message;
        const index = Message.indexOf('{');
        Message = Message.substr(index, Message.length - 1);
        Message = JSON.stringify(JSON.parse(Message), null, 2);
    } catch (e) {
        Message = resp.Error.Message;
    }
    document.getElementById('modalLogs').innerHTML = `<pre>${Message}</pre>`;
    document.getElementById("btn-modal").click()
}

const showModalPayload = async (id) => {
    document.getElementById("load").classList.add("load");
    const endPoint = `/Logs/GetPayload?LogId=${id}`;
    const resp = await get(endPoint);

    console.log(JSON.parse(resp.payload));
    document.getElementById('TitleModal').innerHTML = '<strong>LogPayload</strong>';
    document.getElementById('modalLogs').innerHTML = `<pre>${JSON.stringify(JSON.parse(resp.payload), null, 2)}</pre>`;

    if (document.getElementById("load").classList.contains('load'))
        document.getElementById("load").classList.remove("load");

    document.getElementById("btn-modal").click()
}

const get = async (endPoint) => {
    const url = `${window.config.webapi.url}${endPoint}`;

    var resp = await fetch(url, {
        method: 'get',
        headers: new Headers({
            'Authorization': "Bearer " + localStorage.getItem('tokenApi')
        })
    }).then(async (response) => {
        return await response.json()
    }).catch(function (error) {
        console.log(error);
    });

    return resp;
}

const searchLogs = async () => {
    document.getElementById("load").classList.add("load");
    if (document.querySelector("#search").value == "")
        alert("Você não digitou nenhum valor");

    const endPoint = `/Logs/GetByFilter?Filter=${document.querySelector("#search").value}&TenantId=${idTenant}`;
    var resp = await get(endPoint);

    return renderTable(resp);
}

const changePagination = async (event) => {
    document.getElementById("load").classList.add("load");

    let endPoint;
    let page = Number(document.getElementById('page').innerHTML);
    if (event.target.id == "next")
        page = page + 1;
    else
        page = page - 1;

    if (document.querySelector("#search").value == "")
        endPoint = `/Logs/GetList?TenantId=${idTenant}&Page=${page}`;
    else
        endPoint = `/Logs/GetByFilter?Filter=${document.querySelector("#search").value}&TenantId=${idTenant}&Page=${page}`;

    var resp = await get(endPoint);

    return renderTable(resp);
}

window.onload = loadTenants();