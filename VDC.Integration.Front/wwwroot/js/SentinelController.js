const iconRefresh = `<svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" fill="currentColor" class="bi bi-arrow-clockwise" viewBox="0 0 16 16">
                              <path fill-rule="evenodd" d="M8 3a5 5 0 1 0 4.546 2.914.5.5 0 0 1 .908-.417A6 6 0 1 1 8 2v1z"/>
                              <path d="M8 4.466V.534a.25.25 0 0 1 .41-.192l2.36 1.966c.12.1.12.284 0 .384L8.41 4.658A.25.25 0 0 1 8 4.466z"/>
                          </svg>`;

const sentinelIcon = `<svg xmlns="http://www.w3.org/2000/svg" width="40" height="40" fill="currentColor" class="bi bi-robot" viewBox="0 0 16 16">
                        <path d="M6 12.5a.5.5 0 0 1 .5-.5h3a.5.5 0 0 1 0 1h-3a.5.5 0 0 1-.5-.5ZM3 8.062C3 6.76 4.235 5.765 5.53 5.886a26.58 26.58 0 0 0 4.94 0C11.765 5.765 13 6.76 13 8.062v1.157a.933.933 0 0 1-.765.935c-.845.147-2.34.346-4.235.346-1.895 0-3.39-.2-4.235-.346A.933.933 0 0 1 3 9.219V8.062Zm4.542-.827a.25.25 0 0 0-.217.068l-.92.9a24.767 24.767 0 0 1-1.871-.183.25.25 0 0 0-.068.495c.55.076 1.232.149 2.02.193a.25.25 0 0 0 .189-.071l.754-.736.847 1.71a.25.25 0 0 0 .404.062l.932-.97a25.286 25.286 0 0 0 1.922-.188.25.25 0 0 0-.068-.495c-.538.074-1.207.145-1.98.189a.25.25 0 0 0-.166.076l-.754.785-.842-1.7a.25.25 0 0 0-.182-.135Z" />
                        <path d="M8.5 1.866a1 1 0 1 0-1 0V3h-2A4.5 4.5 0 0 0 1 7.5V8a1 1 0 0 0-1 1v2a1 1 0 0 0 1 1v1a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2v-1a1 1 0 0 0 1-1V9a1 1 0 0 0-1-1v-.5A4.5 4.5 0 0 0 10.5 3h-2V1.866ZM14 7.5V13a1 1 0 0 1-1 1H3a1 1 0 0 1-1-1V7.5A3.5 3.5 0 0 1 5.5 4h5A3.5 3.5 0 0 1 14 7.5Z" />
                    </svg>`

const loadQueues = async () => {

    const endPoint = `${window.config.webapi.url}/Queue/GetQueuesOverflow`;

    document.querySelector('#stores').setAttribute('hidden', 'true');

    const resp = await fetch(endPoint, {
        method: 'get',
        headers: new Headers({
            'Authorization': "Bearer " + localStorage.getItem('tokenApi')
        })
    }).then(async (response) => {
        return await response.json()
    }).catch(function (error) {
        console.log(error);
    });

    const accordion = document.getElementById("accordion");
    resp.value.map((item, index) => {

        const card = document.createElement("div");
        card.classList.add("card");
        card.style.border = '1px solid #dee2e6';

        const cardHeader = document.createElement("div");
        cardHeader.classList.add("card-header");

        const H2 = document.createElement("h2");
        H2.classList.add("mb-0");

        const buttonLink = document.createElement("button");
        buttonLink.classList.add("btn");
        buttonLink.classList.add("btn-link");
        buttonLink.type = "button"
        buttonLink.setAttribute('data-toggle', 'collapse');
        buttonLink.setAttribute('data-target', `#collapse${index}`);
        buttonLink.setAttribute('aria-controls', `collapse${index}`);
        buttonLink.setAttribute('aria-expanded', 'true');

        buttonLink.innerHTML = item.tanantName;

        const collapse = document.createElement("div");
        collapse.classList.add('collapse');
        collapse.setAttribute('id', `collapse${index}`);

        const cardBody = document.createElement("div");
        cardBody.classList.add("card-body");

        H2.appendChild(buttonLink);
        cardHeader.appendChild(H2);
        card.appendChild(cardHeader);

        let innerText = "";
        item.queuesOverflow.map(msg => {

            innerText += `{ <br/>
                             &emsp;<strong>Fila:</strong>&ensp; <u>${msg.queueName}</u><br/>
                             &emsp;<strong>Mensagens ativas:</strong>&ensp; ${msg.messageCount} <br/>
                             &emsp;<strong>Dead-Letter:</strong>&ensp; ${msg.deadLetterMessageCount} <br/>
                           },<br/>`;

        });

        cardBody.innerHTML = innerText;

        document.querySelector('#accordionLoad').setAttribute('hidden', 'true');

        collapse.appendChild(cardBody);
        accordion.appendChild(card);
        accordion.appendChild(collapse);
    });
}

const loadStores = async () => {
    document.querySelector("#stores").removeAttribute("hidden");
    const tbody = document.querySelector('#storeTbody');
    tbody.innerHTML = `<tr><td style="text-align:center" colspan="3"><strong>Carregando...</strong></td></tr>`;

    const endPoint = `${window.config.webapi.url}/Shopify/GetAllIntegrationFail`;

    const resp = await fetch(endPoint, {
        method: 'get',
        headers: new Headers({
            'Authorization': "Bearer " + localStorage.getItem('tokenApi')
        })
    }).then(async (response) => {
        return await response.json()
    }).catch(function (error) {
        console.log(error);
    });

    tbody.innerHTML = "";

    for (var i = 0; i < resp.value.length; i++) {
        const loja = resp.value[i];
        const tr = document.createElement("tr");
        
        const td1 = document.createElement("td");
        const td2 = document.createElement("td");
        const td3 = document.createElement("td");

        td1.innerHTML = i + 1;
        td2.innerHTML = loja.storeName;
        td3.innerHTML = loja.totalOrdersLosted;

        tr.appendChild(td1);
        tr.appendChild(td2);
        tr.appendChild(td3);

        tbody.appendChild(tr);
    }
}

const adjustBtns = () => {
    const btnQueue = document.querySelector("#btn-queue");
    const btnCustomer = document.querySelector("#btn-customer");
    const divBtns = document.querySelector("#orientation-buttons");

    divBtns.classList.remove('orientation-buttons-center');
    btnQueue.classList.remove('btn-center');
    btnCustomer.classList.remove('btn-center');

    divBtns.classList.add('orientation-buttons-top');
    btnQueue.classList.add('top-right');
    btnCustomer.classList.add('top-right');
}

const queues = () => {
    adjustBtns();
    document.querySelector("#btn-customer").innerHTML = 'Lojas';
    document.querySelector("#btn-queue").innerHTML = iconRefresh;

    document.querySelector("#queues").removeAttribute("hidden");
    document.querySelector("#accordion").innerHTML = null;
    document.querySelector('#accordionLoad').removeAttribute('hidden');

    return loadQueues();
}

const customer = () => {
    adjustBtns();
    document.querySelector("#btn-queue").innerHTML = 'Filas';
    document.querySelector("#btn-customer").innerHTML = iconRefresh;

    document.querySelector("#queues").setAttribute("hidden", 'true');
    document.querySelector("#accordion").innerHTML = null;
    document.querySelector('#accordionLoad').setAttribute("hidden", 'true');

    return loadStores();
}

window.onload = () => {
    document.querySelector('#sentinelIcon').innerHTML = sentinelIcon;
}