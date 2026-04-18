
const params = new URLSearchParams(location.search);
const credId = params.get('id');
const credName = decodeURIComponent(params.get('name') || '');
const isMyItems = (credName === 'My items');


var miAllItems = [];   // [{type, id, primary, secondary, raw}]
var miFilter = 'all';

let userPermission = 'None';
let editingId = null;
let deletingId = null;
let deletingType = null;
let sharingItemId = null;
let sharingItemType = null;

if (!credId) { window.location.href = '/'; }

document.addEventListener('DOMContentLoaded', function () {
    var nameEl = document.getElementById('vault-name');
    if (nameEl) nameEl.textContent = credName;
    document.title = credName + ' — FamilyVault';
    var pinInput = document.getElementById('pin-input');
    if (pinInput) {
        pinInput.focus();
        pinInput.addEventListener('keydown', function (e) { if (e.key === 'Enter') submitPin(); });
    }
});

async function submitPin() {
    var pin = document.getElementById('pin-input').value.trim();
    var err = document.getElementById('pin-error');
    err.textContent = '';
    if (!pin) { err.textContent = 'Please enter your PIN.'; return; }
    var res = await apiFetch('/api/pin/verify', 'POST', { pin: pin });
    if (res && res.success) {
        document.getElementById('pin-gate').style.display = 'none';
        document.getElementById('vault-content').style.display = '';
        loadAll();
    } else {
        err.textContent = (res && res.message) ? res.message : 'Incorrect PIN.';
        document.getElementById('pin-input').value = '';
        document.getElementById('pin-input').focus();
    }
}

async function loadAll() {
    var res = await apiFetch('/api/access/my-permission/' + credId);
    if (!res || !res.success) { window.location.href = '/'; return; }
    userPermission = res.data;
    var isOwner = (userPermission === 'Owner');
    var canEdit = isOwner || (userPermission === 'Edit');
    var badge = document.getElementById('perm-badge');
    if (badge && !isOwner) {
        badge.textContent = userPermission;
        badge.className = 'badge badge-' + (userPermission === 'Edit' ? 'edit' : 'view');
    }

    if (isMyItems) {
        document.getElementById('vault-tab-bar').style.display = 'none';
        document.getElementById('my-items-ui').style.display = '';
        ['tab-web', 'tab-cards', 'tab-keys', 'tab-access'].forEach(function (id) {
            document.getElementById(id).style.display = 'none';
        });
        var addBtn = document.getElementById('mi-btn-add');
        if (addBtn) addBtn.style.display = canEdit ? '' : 'none';
        await loadMyItemsFlat();
        return;
    }

    ['btn-add-web', 'btn-add-card', 'btn-add-key'].forEach(function (id) {
        var el = document.getElementById(id);
        if (el) el.style.display = canEdit ? '' : 'none';
    });
    var shareBtn = document.getElementById('btn-share');
    if (shareBtn) shareBtn.style.display = isOwner ? '' : 'none';
    await Promise.all([loadWebLogins(), loadCards(), loadKeys(), loadAccess()]);
}



async function loadMyItemsFlat() {
    document.getElementById('mi-list').innerHTML = '<div class="loading-row"><span class="spinner"></span></div>';
    miAllItems = [];

    var results = await Promise.all([
        apiFetch('/api/webcredentials/by-credential/' + credId),
        apiFetch('/api/creditdebitcards/by-credential/' + credId),
        apiFetch('/api/securitykeys/by-credential/' + credId)
    ]);

    var webs = (results[0] && results[0].data) ? results[0].data : [];
    var cards = (results[1] && results[1].data) ? results[1].data : [];
    var keys = (results[2] && results[2].data) ? results[2].data : [];

    webs.forEach(function (w) {
        miAllItems.push({ type: 'web', id: w.webCredentialId, primary: w.username, secondary: w.url, raw: w, canEdit: w.canEdit });
    });
    cards.forEach(function (c) {
        miAllItems.push({ type: 'cards', id: c.creditDebitId, primary: c.cardHolderName, secondary: '**** ' + c.expiryMonth + '/' + c.expiryYear, raw: c, canEdit: c.canEdit });
    });
    keys.forEach(function (k) {
        miAllItems.push({ type: 'keys', id: k.securityKeyId, primary: k.label, secondary: k.notes || 'Security key', raw: k, canEdit: k.canEdit });
    });

    // Distinct items
    var seen = {};
    miAllItems = miAllItems.filter(function (i) {
        if (seen[i.id]) return false;
        seen[i.id] = true;
        return true;
    });

    renderMyItemsFlat();
}
//filter buttons
function setMyItemsFilter(filter, btn) {
    miFilter = filter;
    document.querySelectorAll('.my-items-toolbar .filter-btn').forEach(function (b) { b.classList.remove('active'); });
    btn.classList.add('active');
    renderMyItemsFlat();
}

function renderMyItemsFlat() {
    var filtered = miFilter === 'all' ? miAllItems : miAllItems.filter(function (i) { return i.type === miFilter; });
    document.getElementById('mi-count').textContent = filtered.length + ' item' + (filtered.length !== 1 ? 's' : '');

    var list = document.getElementById('mi-list');
    if (!filtered.length) { list.innerHTML = emptyState('No items here yet.'); return; }

    var typeIcons = {
        web: '<svg width="15" height="15" viewBox="0 0 16 16" fill="none"><circle cx="8" cy="8" r="6.5" stroke="currentColor" stroke-width="1.3" fill="none"/><path d="M8 1.5C6.5 4.5 6 6 6 8s.5 3.5 2 6.5M8 1.5C9.5 4.5 10 6 10 8s-.5 3.5-2 6.5M1.5 8h13" stroke="currentColor" stroke-width="1.1" stroke-linecap="round"/></svg>',
        cards: '<svg width="15" height="15" viewBox="0 0 16 16" fill="none"><rect x="1" y="3.5" width="14" height="9" rx="1.5" stroke="currentColor" stroke-width="1.3" fill="none"/><path d="M1 7h14" stroke="currentColor" stroke-width="1.3"/><rect x="2.5" y="9" width="3" height="1.5" rx="0.5" fill="currentColor" opacity="0.5"/></svg>',
        keys: '<svg width="15" height="15" viewBox="0 0 16 16" fill="none"><circle cx="6" cy="6" r="3.5" stroke="currentColor" stroke-width="1.3" fill="none"/><path d="M8.5 8.5L14 14M11.5 11l1.5 1.5" stroke="currentColor" stroke-width="1.3" stroke-linecap="round"/></svg>'
    };
    var typeLabels = { web: 'Login', cards: 'Card', keys: 'Key' };
    var isOwner = (userPermission === 'Owner');
    var canEdit = isOwner || (userPermission === 'Edit');
    var apiType = { web: 'web', cards: 'card', keys: 'key' };
    var editFns = { web: 'openEditWebModal', cards: 'openEditCardModal', keys: 'openEditKeyModal' };
    var delTypes = { web: 'web', cards: 'card', keys: 'key' };
    var iconClass = { web: 'web', cards: 'card', keys: 'key' };

    list.innerHTML = filtered.map(function (item) {
        var t = item.type;
        var safeJson = JSON.stringify(item.raw).replace(/'/g, '&#39;');
        var typePill = '<span class="shared-item-type-tag">' + typeLabels[t] + '</span>';

        var html = '<div class="item-row mi-item-row" style="cursor:pointer"'
            + ' data-type="' + t + '" data-raw=\'' + safeJson + '\'>'
            + '<div class="item-icon item-icon-' + iconClass[t] + '">' + typeIcons[t] + '</div>'
            + '<div class="item-info">'
            + '<div class="item-primary">' + escHtml(item.primary) + '</div>'
            + '<div class="item-secondary">' + typePill + ' · ' + escHtml(item.secondary) + '</div>'
            + '</div>'
            + '<div class="item-actions" onclick="event.stopPropagation()">'
            + '<span class="item-meta">' + timeAgo(item.raw.createdAt) + '</span>'
            // History
            + '<button class="icon-btn" title="History"'
            + ' data-type="' + apiType[t] + '" data-id="' + item.id + '" data-name="' + escHtml(item.primary) + '"'
            + ' onclick="loadHistory(this.dataset.type,this.dataset.id,this.dataset.name)">'
            + '<svg width="13" height="13" viewBox="0 0 14 14" fill="none"><circle cx="7" cy="7" r="5.5" stroke="currentColor" stroke-width="1.2" fill="none"/><path d="M7 4v3l2 1.5" stroke="currentColor" stroke-width="1.2" stroke-linecap="round"/></svg>'
            + '</button>';

        // Share (owner only)
        if (isOwner) {
            html += '<button class="icon-btn" title="Share"'
                + ' data-type="' + apiType[t] + '" data-id="' + item.id + '" data-name="' + escHtml(item.primary) + '"'
                + ' onclick="openShareItemModal(this.dataset.type,this.dataset.id,this.dataset.name)">'
                + '<svg width="13" height="13" viewBox="0 0 14 14" fill="none"><circle cx="5" cy="4.5" r="2.5" stroke="currentColor" stroke-width="1.3" fill="none"/><path d="M1 12c0-2.761 1.79-5 4-5M10 8v4M8 10h4" stroke="currentColor" stroke-width="1.3" stroke-linecap="round"/></svg>'
                + '</button>';
        }

        // Edit
        if (canEdit) {
            html += '<button class="icon-btn" title="Edit" data-item=\'' + safeJson + '\' onclick="' + editFns[t] + '(JSON.parse(this.dataset.item))">'
                + '<svg width="13" height="13" viewBox="0 0 14 14" fill="none"><path d="M9.5 2.5l2 2L4 12H2v-2L9.5 2.5z" stroke="currentColor" stroke-width="1.3" stroke-linecap="round" stroke-linejoin="round"/></svg>'
                + '</button>';
        }

        // Delete (owner only)
        if (isOwner) {
            var dtype = delTypes[t];
            html += '<button class="icon-btn icon-btn-danger" title="Delete"'
                + ' data-id="' + item.id + '" data-name="' + escHtml(item.primary) + '" data-dtype="' + dtype + '"'
                + ' onclick="openDelItem(this.dataset.id,this.dataset.dtype,this.dataset.name)">'
                + '<svg width="13" height="13" viewBox="0 0 14 14" fill="none"><path d="M2 3.5h10M5.5 3.5V2h3v1.5M3 3.5l.75 8.5h6.5L11 3.5" stroke="currentColor" stroke-width="1.3" stroke-linecap="round" stroke-linejoin="round"/></svg>'
                + '</button>';
        }

        html += '</div></div>';
        return html;
    }).join('');

    //detail modal
    list.onclick = function (e) {
        if (e.target.closest('.icon-btn, button')) return;
        var row = e.target.closest('.item-row');
        if (!row) return;
        var t = row.dataset.type;
        var raw = JSON.parse(row.dataset.raw);
        if (t === 'web') showWebDetail(raw);
        if (t === 'cards') showCardDetail(raw);
        if (t === 'keys') showKeyDetail(raw);
    };
}

function openAddItemFromVault() {
    // Default to web login;switch to other tabs
    openAddWebModal();
}

function switchVaultTab(tab, btn) {
    document.querySelectorAll('.tab-btn').forEach(function (b) { b.classList.remove('active'); });
    btn.classList.add('active');
    ['web', 'cards', 'keys', 'access'].forEach(function (t) {
        document.getElementById('tab-' + t).style.display = (t === tab) ? '' : 'none';
    });
}

//Detail modal

function showDetailModal(title, rows) {
    document.getElementById('detail-title').textContent = title;
    document.getElementById('detail-body').innerHTML = rows.map(function (r) {
        return '<div class="detail-row">'
            + '<div class="detail-label">' + escHtml(r.label) + '</div>'
            + '<div class="detail-value' + (r.mono ? ' detail-mono' : '') + (r.muted ? ' detail-muted' : '') + '">' + r.value + '</div>'
            + '</div>';
    }).join('');
    openModal('modal-detail');
}

function revealBtn(endpoint, field) {
    return '<button class="btn-ghost btn-sm" style="padding:3px 10px;font-size:12px" '
        + 'onclick="revealField(this,\'' + endpoint + '\',\'' + field + '\')">Click to reveal</button>';
}

async function revealField(btn, endpoint, field) {
    btn.textContent = '…';
    btn.disabled = true;
    var res = await apiFetch(endpoint);
    if (res && res.success && res.data) {
        btn.outerHTML = '<span class="detail-mono">' + escHtml(res.data[field]) + '</span>';
    } else {
        btn.textContent = 'Error';
        btn.disabled = false;
    }
}

function showWebDetail(w) {
    showDetailModal('Web login', [
        { label: 'Username', value: escHtml(w.username) },
        { label: 'URL', value: '<a href="' + escHtml(w.url) + '" target="_blank" rel="noopener">' + escHtml(w.url) + '</a>' },
        { label: 'Password', value: revealBtn('/api/webcredentials/' + w.webCredentialId + '/reveal', 'password') },
        { label: 'Notes', value: w.notes ? escHtml(w.notes) : '—', muted: !w.notes },
        { label: 'Added', value: formatDate(w.createdAt), muted: true },
        { label: 'Edited', value: w.editedAt ? formatDate(w.editedAt) : '—', muted: true }
    ]);
}

function showCardDetail(c) {
    showDetailModal('Credit / debit card', [
        { label: 'Cardholder', value: escHtml(c.cardHolderName) },
        { label: 'Card number', value: revealBtn('/api/creditdebitcards/' + c.creditDebitId + '/reveal', 'cardNumber') },
        { label: 'Expiry', value: c.expiryMonth + ' / ' + c.expiryYear, mono: true },
        { label: 'CVV', value: revealBtn('/api/creditdebitcards/' + c.creditDebitId + '/reveal', 'cvv') },
        { label: 'PIN', value: revealBtn('/api/creditdebitcards/' + c.creditDebitId + '/reveal', 'pin') },
        { label: 'Billing', value: c.billingAddress ? escHtml(c.billingAddress) : '—', muted: !c.billingAddress },
        { label: 'Notes', value: c.notes ? escHtml(c.notes) : '—', muted: !c.notes },
        { label: 'Added', value: formatDate(c.createdAt), muted: true }
    ]);
}

function showKeyDetail(k) {
    showDetailModal('Security key', [
        { label: 'Label', value: escHtml(k.label) },
        { label: 'Secret', value: revealBtn('/api/securitykeys/' + k.securityKeyId + '/reveal', 'pin') },
        { label: 'Notes', value: k.notes ? escHtml(k.notes) : '—', muted: !k.notes },
        { label: 'Added', value: formatDate(k.createdAt), muted: true },
        { label: 'Edited', value: k.editedAt ? formatDate(k.editedAt) : '—', muted: true }
    ]);
}

// Item lists 

async function loadWebLogins() {
    var res = await apiFetch('/api/webcredentials/by-credential/' + credId);
    var items = (res && res.data) ? res.data : [];
    document.getElementById('web-count').textContent = items.length + ' login' + (items.length !== 1 ? 's' : '');
    var list = document.getElementById('web-list');
    if (!items.length) { list.innerHTML = emptyState('No web logins yet.'); return; }
    list.innerHTML = items.map(function (w) {
        return buildRow('web', w.webCredentialId, escHtml(w.username), escHtml(w.url), timeAgo(w.createdAt), safeJson(w));
    }).join('');
    list.onclick = function (e) {
        if (e.target.closest('.icon-btn, .reveal-btn, button')) return;
        var row = e.target.closest('.item-row');
        if (!row || !row.dataset.item) return;
        showWebDetail(JSON.parse(row.dataset.item));
    };
}

async function loadCards() {
    var res = await apiFetch('/api/creditdebitcards/by-credential/' + credId);
    var items = (res && res.data) ? res.data : [];
    document.getElementById('cards-count').textContent = items.length + ' card' + (items.length !== 1 ? 's' : '');
    var list = document.getElementById('cards-list');
    if (!items.length) { list.innerHTML = emptyState('No cards yet.'); return; }
    list.innerHTML = items.map(function (c) {
        return buildRow('card', c.creditDebitId, escHtml(c.cardHolderName),
            escHtml(c.maskedCardNumber) + ' · ' + c.expiryMonth + '/' + c.expiryYear,
            timeAgo(c.createdAt), safeJson(c));
    }).join('');
    list.onclick = function (e) {
        if (e.target.closest('.icon-btn, button')) return;
        var row = e.target.closest('.item-row');
        if (!row || !row.dataset.item) return;
        showCardDetail(JSON.parse(row.dataset.item));
    };
}

async function loadKeys() {
    var res = await apiFetch('/api/securitykeys/by-credential/' + credId);
    var items = (res && res.data) ? res.data : [];
    document.getElementById('keys-count').textContent = items.length + ' key' + (items.length !== 1 ? 's' : '');
    var list = document.getElementById('keys-list');
    if (!items.length) { list.innerHTML = emptyState('No security keys yet.'); return; }
    list.innerHTML = items.map(function (k) {
        return buildRow('key', k.securityKeyId, escHtml(k.label),
            k.notes ? escHtml(k.notes) : 'No notes',
            timeAgo(k.createdAt), safeJson(k));
    }).join('');
    list.onclick = function (e) {
        if (e.target.closest('.icon-btn, button')) return;
        var row = e.target.closest('.item-row');
        if (!row || !row.dataset.item) return;
        showKeyDetail(JSON.parse(row.dataset.item));
    };
}

//Row builder

function buildRow(type, id, primary, secondary, meta, itemJson) {
    var isOwner = (userPermission === 'Owner');
    var canEdit = isOwner || (userPermission === 'Edit');

    var icons = {
        web: '<svg width="15" height="15" viewBox="0 0 16 16" fill="none"><circle cx="8" cy="8" r="6.5" stroke="currentColor" stroke-width="1.3" fill="none"/><path d="M8 1.5C6.5 4.5 6 6 6 8s.5 3.5 2 6.5M8 1.5C9.5 4.5 10 6 10 8s-.5 3.5-2 6.5M1.5 8h13" stroke="currentColor" stroke-width="1.1" stroke-linecap="round"/></svg>',
        card: '<svg width="15" height="15" viewBox="0 0 16 16" fill="none"><rect x="1" y="3.5" width="14" height="9" rx="1.5" stroke="currentColor" stroke-width="1.3" fill="none"/><path d="M1 7h14" stroke="currentColor" stroke-width="1.3"/><rect x="2.5" y="9" width="3" height="1.5" rx="0.5" fill="currentColor" opacity="0.5"/></svg>',
        key: '<svg width="15" height="15" viewBox="0 0 16 16" fill="none"><circle cx="6" cy="6" r="3.5" stroke="currentColor" stroke-width="1.3" fill="none"/><path d="M8.5 8.5L14 14M11.5 11l1.5 1.5" stroke="currentColor" stroke-width="1.3" stroke-linecap="round"/></svg>'
    };
    var editFns = { web: 'openEditWebModal', card: 'openEditCardModal', key: 'openEditKeyModal' };

    //icon 
    var shareIconSvg = '<svg width="13" height="13" viewBox="0 0 14 14" fill="none">'
        + '<circle cx="5" cy="4.5" r="2.5" stroke="currentColor" stroke-width="1.3" fill="none"/>'
        + '<path d="M1 12c0-2.761 1.79-5 4-5M10 8v4M8 10h4" stroke="currentColor" stroke-width="1.3" stroke-linecap="round"/>'
        + '</svg>';

    return '<div class="item-row" style="cursor:pointer"'
        + ' data-item-id="' + id + '"'
        + ' data-primary="' + primary + '"'
        + ' data-secondary="' + secondary + '"'
        + ' data-item=\'' + itemJson + '\'>'
        + '<div class="item-icon item-icon-' + type + '">' + icons[type] + '</div>'
        + '<div class="item-info">'
        + '<div class="item-primary">' + primary + '</div>'
        + '<div class="item-secondary">' + secondary + '</div>'
        + '</div>'
        + '<div class="item-actions" onclick="event.stopPropagation()">'
        + '<span class="item-meta">' + meta + '</span>'
        // History
        + '<button class="icon-btn" title="History"'
        + ' data-type="' + type + '" data-id="' + id + '" data-name="' + primary + '"'
        + ' onclick="loadHistory(this.dataset.type,this.dataset.id,this.dataset.name)">'
        + '<svg width="13" height="13" viewBox="0 0 14 14" fill="none"><circle cx="7" cy="7" r="5.5" stroke="currentColor" stroke-width="1.2" fill="none"/><path d="M7 4v3l2 1.5" stroke="currentColor" stroke-width="1.2" stroke-linecap="round"/></svg>'
        + '</button>'
        // Share (owners only)
        + (isOwner
            ? '<button class="icon-btn" title="Share item"'
            + ' data-type="' + type + '" data-id="' + id + '" data-name="' + primary + '"'
            + ' onclick="openShareItemModal(this.dataset.type,this.dataset.id,this.dataset.name)">'
            + shareIconSvg + '</button>'
            : '')
        // Edit
        + (canEdit
            ? '<button class="icon-btn" title="Edit"'
            + ' data-item=\'' + itemJson + '\''
            + ' onclick="' + editFns[type] + '(JSON.parse(this.dataset.item))">'
            + '<svg width="13" height="13" viewBox="0 0 14 14" fill="none"><path d="M9.5 2.5l2 2L4 12H2v-2L9.5 2.5z" stroke="currentColor" stroke-width="1.3" stroke-linecap="round" stroke-linejoin="round"/></svg>'
            + '</button>'
            : '')
        // Delete(owner only)
        + (isOwner
            ? '<button class="icon-btn icon-btn-danger" title="Delete"'
            + ' data-id="' + id + '" data-name="' + primary + '"'
            + ' onclick="openDelItem(this.dataset.id,\'' + type + '\',this.dataset.name)">'
            + '<svg width="13" height="13" viewBox="0 0 14 14" fill="none"><path d="M2 3.5h10M5.5 3.5V2h3v1.5M3 3.5l.75 8.5h6.5L11 3.5" stroke="currentColor" stroke-width="1.3" stroke-linecap="round" stroke-linejoin="round"/></svg>'
            + '</button>'
            : '')
        + '</div></div>';
}

// Item-level share modal

function openShareItemModal(type, id, name) {
    sharingItemType = type;
    sharingItemId = id;

    document.getElementById('share-item-name').textContent = name;
    document.getElementById('share-item-username').value = '';
    document.getElementById('share-item-permission').value = 'View';
    document.getElementById('share-item-error').textContent = '';

    // Default expiry: 1 year
    var d = new Date();
    d.setFullYear(d.getFullYear() + 1);
    document.getElementById('share-item-expiry').value = d.toISOString().split('T')[0];

    // Load existing shares
    loadItemAccessList(type, id);
    openModal('modal-share-item');
}

async function loadItemAccessList(type, id) {
    var endpoints = { web: 'webcredentials', card: 'creditdebitcards', key: 'securitykeys' };
    var listEl = document.getElementById('share-item-current');
    listEl.innerHTML = '<div class="loading-row" style="padding:0.75rem 0"><span class="spinner"></span></div>';

    var res = await apiFetch('/api/' + endpoints[type] + '/' + id + '/access');
    var items = (res && res.data) ? res.data : [];

    if (!items.length) {
        listEl.innerHTML = '<p style="font-size:13px;color:var(--text-muted);margin:0">Not shared with anyone yet.</p>';
        return;
    }

    listEl.innerHTML = items.map(function (a) {
        return '<div class="share-access-row">'
            + '<div class="share-access-info">'
            + '<span class="share-access-user">' + escHtml(a.sharedToUsername) + '</span>'
            + '<span class="badge badge-' + (a.permissionLevel === 'Edit' ? 'edit' : 'view') + '" style="margin-left:6px">' + a.permissionLevel + '</span>'
            + '</div>'
            + '<div class="share-access-meta">'
            + '<span class="shared-expires' + (a.isExpired ? ' expired' : '') + '">'
            + (a.isExpired ? 'Expired' : 'Exp. ' + formatDate(a.expireAt))
            + '</span>'
            + '<button class="icon-btn icon-btn-danger" style="margin-left:4px" title="Revoke"'
            + ' data-id="' + a.accessId + '"'
            + ' onclick="revokeItemAccess(this.dataset.id)">'
            + '<svg width="12" height="12" viewBox="0 0 14 14" fill="none"><path d="M3 3l8 8M11 3l-8 8" stroke="currentColor" stroke-width="1.4" stroke-linecap="round"/></svg>'
            + '</button>'
            + '</div>'
            + '</div>';
    }).join('');
}

async function saveItemShare() {
    var username = document.getElementById('share-item-username').value.trim();
    var perm = document.getElementById('share-item-permission').value;
    var expiry = document.getElementById('share-item-expiry').value;
    var errEl = document.getElementById('share-item-error');

    if (!username) { errEl.textContent = 'Username is required.'; return; }
    if (!expiry) { errEl.textContent = 'Expiry date is required.'; return; }

    var endpoints = { web: 'webcredentials', card: 'creditdebitcards', key: 'securitykeys' };
    var url = '/api/' + endpoints[sharingItemType] + '/' + sharingItemId + '/access';

    var res = await apiFetch(url, 'POST', {
        sharedToUsername: username,
        permissionLevel: perm,
        expireAt: new Date(expiry).toISOString()
    });

    if (res && res.success) {
        document.getElementById('share-item-username').value = '';
        errEl.textContent = '';
        loadItemAccessList(sharingItemType, sharingItemId);
    } else {
        errEl.textContent = (res && res.message) ? res.message : 'Error sharing.';
    }
}

async function revokeItemAccess(accessId) {
    if (!confirm("Revoke this user's access?")) return;
    var endpoints = { web: 'webcredentials', card: 'creditdebitcards', key: 'securitykeys' };
    var url = '/api/' + endpoints[sharingItemType] + '/' + sharingItemId + '/access/' + accessId;
    var res = await apiFetch(url, 'DELETE');
    if (res && res.success) loadItemAccessList(sharingItemType, sharingItemId);
}

//Web modal

function openAddWebModal() {
    editingId = null;
    document.getElementById('modal-web-title').textContent = 'Add web login';
    ['web-url', 'web-username', 'web-password', 'web-notes'].forEach(function (id) { document.getElementById(id).value = ''; });
    document.getElementById('web-pass-hint').textContent = '';
    document.getElementById('web-error').textContent = '';
    openModal('modal-web');
}
function openEditWebModal(w) {
    editingId = w.webCredentialId;
    document.getElementById('modal-web-title').textContent = 'Edit login';
    document.getElementById('web-url').value = w.url;
    document.getElementById('web-username').value = w.username;
    document.getElementById('web-password').value = '';
    document.getElementById('web-notes').value = w.notes || '';
    document.getElementById('web-pass-hint').textContent = '(leave blank to keep current)';
    document.getElementById('web-error').textContent = '';
    openModal('modal-web');
}
async function saveWebLogin() {
    var url = document.getElementById('web-url').value.trim();
    var user = document.getElementById('web-username').value.trim();
    var pass = document.getElementById('web-password').value;
    var notes = document.getElementById('web-notes').value.trim();
    var errEl = document.getElementById('web-error');
    if (!url || !user) { errEl.textContent = 'URL and username are required.'; return; }
    if (!editingId && !pass) { errEl.textContent = 'Password is required.'; return; }
    var res = editingId
        ? await apiFetch('/api/webcredentials/' + editingId, 'PUT', { url, username: user, password: pass || null, notes })
        : await apiFetch('/api/webcredentials', 'POST', { credentialId: credId, url, username: user, password: pass, notes });
    if (res && res.success) {
        closeModal('modal-web');
        if (editingId && res.data) showWebDetail(res.data);
        if (isMyItems) { loadMyItemsFlat(); } else { loadWebLogins(); }
    }
    else errEl.textContent = (res && res.message) ? res.message : 'Error saving.';
}

// Card modal

function openAddCardModal() {
    editingId = null;
    document.getElementById('modal-card-title').textContent = 'Add card';
    ['card-holder', 'card-number', 'card-month', 'card-year', 'card-cvv', 'card-pin', 'card-billing', 'card-notes']
        .forEach(function (id) { document.getElementById(id).value = ''; });
    ['card-num-hint', 'card-cvv-hint', 'card-pin-hint']
        .forEach(function (id) { document.getElementById(id).textContent = ''; });
    document.getElementById('card-error').textContent = '';
    openModal('modal-card');
}
function openEditCardModal(c) {
    editingId = c.creditDebitId;
    document.getElementById('modal-card-title').textContent = 'Edit card';
    document.getElementById('card-holder').value = c.cardHolderName;
    document.getElementById('card-number').value = '';
    document.getElementById('card-month').value = c.expiryMonth;
    document.getElementById('card-year').value = c.expiryYear;
    document.getElementById('card-cvv').value = '';
    document.getElementById('card-pin').value = '';
    document.getElementById('card-billing').value = c.billingAddress || '';
    document.getElementById('card-notes').value = c.notes || '';
    ['card-num-hint', 'card-cvv-hint', 'card-pin-hint']
        .forEach(function (id) { document.getElementById(id).textContent = '(leave blank to keep)'; });
    document.getElementById('card-error').textContent = '';
    openModal('modal-card');
}
async function saveCard() {
    var holder = document.getElementById('card-holder').value.trim();
    var number = document.getElementById('card-number').value.replace(/\s/g, '');
    var month = document.getElementById('card-month').value.trim();
    var year = document.getElementById('card-year').value.trim();
    var cvv = document.getElementById('card-cvv').value.trim();
    var pin = document.getElementById('card-pin').value.trim();
    var billing = document.getElementById('card-billing').value.trim();
    var notes = document.getElementById('card-notes').value.trim();
    var errEl = document.getElementById('card-error');
    if (!holder || !month || !year) { errEl.textContent = 'Cardholder name, month, and year are required.'; return; }
    if (!editingId && (!number || !cvv || !pin)) { errEl.textContent = 'Card number, CVV, and PIN are required.'; return; }
    var res = editingId
        ? await apiFetch('/api/creditdebitcards/' + editingId, 'PUT',
            { cardHolderName: holder, cardNumber: number || null, expiryMonth: month, expiryYear: year, cvv: cvv || null, pin: pin || null, billingAddress: billing, notes })
        : await apiFetch('/api/creditdebitcards', 'POST',
            { credentialId: credId, cardHolderName: holder, cardNumber: number, expiryMonth: month, expiryYear: year, cvv, pin, billingAddress: billing, notes });
    if (res && res.success) {
        closeModal('modal-card');
        if (editingId && res.data) showCardDetail(res.data);
        if (isMyItems) { loadMyItemsFlat(); } else { loadCards(); }
    }
    else errEl.textContent = (res && res.message) ? res.message : 'Error saving.';
}

// Key modal 
function openAddKeyModal() {
    editingId = null;
    document.getElementById('modal-key-title').textContent = 'Add security key';
    ['key-label', 'key-pin', 'key-notes'].forEach(function (id) { document.getElementById(id).value = ''; });
    document.getElementById('key-pin-hint').textContent = '';
    document.getElementById('key-error').textContent = '';
    openModal('modal-key');
}
function openEditKeyModal(k) {
    editingId = k.securityKeyId;
    document.getElementById('modal-key-title').textContent = 'Edit key';
    document.getElementById('key-label').value = k.label;
    document.getElementById('key-pin').value = '';
    document.getElementById('key-notes').value = k.notes || '';
    document.getElementById('key-pin-hint').textContent = '(leave blank to keep)';
    document.getElementById('key-error').textContent = '';
    openModal('modal-key');
}
async function saveKey() {
    var label = document.getElementById('key-label').value.trim();
    var pin = document.getElementById('key-pin').value;
    var notes = document.getElementById('key-notes').value.trim();
    var errEl = document.getElementById('key-error');
    if (!label) { errEl.textContent = 'Label is required.'; return; }
    if (!editingId && !pin) { errEl.textContent = 'PIN/secret is required.'; return; }
    var res = editingId
        ? await apiFetch('/api/securitykeys/' + editingId, 'PUT', { label, pin: pin || null, notes })
        : await apiFetch('/api/securitykeys', 'POST', { credentialId: credId, label, pin, notes });
    if (res && res.success) {
        closeModal('modal-key');
        if (editingId && res.data) showKeyDetail(res.data);
        if (isMyItems) { loadMyItemsFlat(); } else { loadKeys(); }
    }
    else errEl.textContent = (res && res.message) ? res.message : 'Error saving.';
}

// Folder access tab

async function loadAccess() {
    var res = await apiFetch('/api/access/credential/' + credId);
    var items = (res && res.data) ? res.data : [];
    var isOwner = (userPermission === 'Owner');
    document.getElementById('access-count').textContent =
        items.length + ' user' + (items.length !== 1 ? 's' : '') + ' with access';
    var list = document.getElementById('access-list');
    if (!items.length) { list.innerHTML = emptyState('No one else has access.'); return; }
    list.innerHTML = items.map(function (a) {
        return '<div class="item-row">'
            + '<div class="shared-avatar" style="flex-shrink:0">' + initials(a.sharedToUsername) + '</div>'
            + '<div class="item-info">'
            + '<div class="item-primary">' + escHtml(a.sharedToUsername) + '</div>'
            + '<div class="item-secondary">Shared by ' + escHtml(a.sharedByUsername) + ' · ' + formatDate(a.sharedAt) + '</div>'
            + '</div>'
            + '<div class="item-actions">'
            + '<span class="badge badge-' + (a.permissionLevel === 'Edit' ? 'edit' : 'view') + '">' + a.permissionLevel + '</span>'
            + '<span class="shared-expires' + (a.isExpired ? ' expired' : '') + '">'
            + (a.isExpired ? 'Expired' : 'Expires ' + formatDate(a.expireAt))
            + '</span>'
            + (isOwner
                ? '<button class="icon-btn icon-btn-danger" data-id="' + a.credentialAccessId + '"'
                + ' onclick="revokeAccess(this.dataset.id)">'
                + '<svg width="13" height="13" viewBox="0 0 14 14" fill="none"><path d="M3 3l8 8M11 3l-8 8" stroke="currentColor" stroke-width="1.4" stroke-linecap="round"/></svg>'
                + '</button>'
                : '')
            + '</div></div>';
    }).join('');
}

function openShareModal() {
    document.getElementById('share-username').value = '';
    document.getElementById('share-permission').value = 'View';
    document.getElementById('share-error').textContent = '';
    var d = new Date();
    d.setFullYear(d.getFullYear() + 1);
    document.getElementById('share-expiry').value = d.toISOString().split('T')[0];
    openModal('modal-share');
}
async function grantAccess() {
    var username = document.getElementById('share-username').value.trim();
    var perm = document.getElementById('share-permission').value;
    var expiry = document.getElementById('share-expiry').value;
    var errEl = document.getElementById('share-error');
    if (!username) { errEl.textContent = 'Username is required.'; return; }
    if (!expiry) { errEl.textContent = 'Expiry date is required.'; return; }
    var res = await apiFetch('/api/access/grant', 'POST', {
        credentialId: credId, sharedToUsername: username,
        permissionLevel: perm, expireAt: new Date(expiry).toISOString()
    });
    if (res && res.success) { closeModal('modal-share'); loadAccess(); }
    else errEl.textContent = (res && res.message) ? res.message : 'Error sharing.';
}
async function revokeAccess(accessId) {
    if (!confirm("Revoke this user's access?")) return;
    var res = await apiFetch('/api/access/' + accessId, 'DELETE');
    if (res && res.success) loadAccess();
}

// History
async function loadHistory(type, id, name) {
    openModal('modal-history');
    var body = document.getElementById('history-body');
    body.innerHTML = '<div class="loading-row"><span class="spinner"></span></div>';
    var endpoints = { web: 'webcredentials', card: 'creditdebitcards', key: 'securitykeys' };
    var res = await apiFetch('/api/' + endpoints[type] + '/' + id + '/history');
    var items = (res && res.data) ? res.data : [];
    if (!items.length) { body.innerHTML = '<p style="color:var(--text-muted);font-size:14px">No history yet.</p>'; return; }
    body.innerHTML = '<p style="font-size:12px;color:var(--text-muted);margin-bottom:12px">History for "' + escHtml(name) + '"</p>'
        + items.map(function (h) {
            return '<div class="history-row">'
                + '<div class="history-badge ' + h.changeType.toLowerCase() + '">' + h.changeType + '</div>'
                + '<div class="history-info">'
                + '<div class="history-who">' + (h.changedByName || 'Unknown') + '</div>'
                + '<div class="history-when">' + (h.changedAt ? formatDate(h.changedAt) : '—') + '</div>'
                + '</div></div>';
        }).join('');
}

// Delete
function openDelItem(id, type, name) {
    deletingId = id;
    deletingType = type;
    document.getElementById('del-item-name').textContent = name;
    openModal('modal-del-item');
}
async function confirmItemDelete() {
    var endpoints = { web: 'webcredentials', card: 'creditdebitcards', key: 'securitykeys' };
    var res = await apiFetch('/api/' + endpoints[deletingType] + '/' + deletingId, 'DELETE');
    if (res && res.success) {
        closeModal('modal-del-item');
        if (deletingType === 'web') loadWebLogins();
        else if (deletingType === 'card') loadCards();
        else loadKeys();
    }
}

// Helpers

function emptyState(msg) { return '<div class="empty-state"><p>' + msg + '</p></div>'; }
function toggleReveal(inputId, btn) {
    var inp = document.getElementById(inputId);
    inp.type = (inp.type === 'text') ? 'password' : 'text';
    btn.style.opacity = (inp.type === 'text') ? '1' : '0.5';
}
function safeJson(obj) { return JSON.stringify(obj).replace(/'/g, '&#39;'); }

// add individual items modal

var vActiveItemType = 'web';

function openAddItemModal() {
    vSelectItemType('web');
    ['vai-url', 'vai-username', 'vai-password', 'vai-notes',
        'vai-holder', 'vai-number', 'vai-month', 'vai-year', 'vai-cvv', 'vai-pin', 'vai-billing', 'vai-card-notes',
        'vai-label', 'vai-kpin', 'vai-key-notes'].forEach(function (id) {
            var el = document.getElementById(id);
            if (el) el.value = '';
        });
    document.getElementById('vai-error').textContent = '';
    openModal('modal-add-item');
}

function vSelectItemType(type) {
    vActiveItemType = type;
    ['web', 'card', 'key'].forEach(function (t) {
        var btn = document.getElementById('v-type-btn-' + t);
        var panel = document.getElementById('vai-' + t);
        if (btn) btn.classList.toggle('active', t === type);
        if (panel) panel.style.display = (t === type) ? '' : 'none';
    });
}

async function vSaveAddItem() {
    var errEl = document.getElementById('vai-error');
    errEl.textContent = '';
    var res;

    if (vActiveItemType === 'web') {
        var url = document.getElementById('vai-url').value.trim();
        var user = document.getElementById('vai-username').value.trim();
        var pass = document.getElementById('vai-password').value;
        var notes = document.getElementById('vai-notes').value.trim();
        if (!url || !user || !pass) { errEl.textContent = 'URL, username and password are required.'; return; }
        res = await apiFetch('/api/webcredentials', 'POST', { credentialId: credId, url: url, username: user, password: pass, notes: notes });
        if (res && res.success) { closeModal('modal-add-item'); loadMyItemsFlat(); }

    } else if (vActiveItemType === 'card') {
        var holder = document.getElementById('vai-holder').value.trim();
        var number = document.getElementById('vai-number').value.replace(/\s/g, '');
        var month = document.getElementById('vai-month').value.trim();
        var year = document.getElementById('vai-year').value.trim();
        var cvv = document.getElementById('vai-cvv').value.trim();
        var pin = document.getElementById('vai-pin').value.trim();
        var billing = document.getElementById('vai-billing').value.trim();
        var cnotes = document.getElementById('vai-card-notes').value.trim();
        if (!holder || !number || !month || !year || !cvv || !pin) { errEl.textContent = 'All card fields except billing and notes are required.'; return; }
        res = await apiFetch('/api/creditdebitcards', 'POST', { credentialId: credId, cardHolderName: holder, cardNumber: number, expiryMonth: month, expiryYear: year, cvv: cvv, pin: pin, billingAddress: billing, notes: cnotes });
        if (res && res.success) { closeModal('modal-add-item'); loadMyItemsFlat(); }

    } else {
        var label = document.getElementById('vai-label').value.trim();
        var kpin = document.getElementById('vai-kpin').value;
        var knotes = document.getElementById('vai-key-notes').value.trim();
        if (!label || !kpin) { errEl.textContent = 'Label and PIN/secret are required.'; return; }
        res = await apiFetch('/api/securitykeys', 'POST', { credentialId: credId, label: label, pin: kpin, notes: knotes });
        if (res && res.success) { closeModal('modal-add-item'); loadMyItemsFlat(); }
    }

    if (res && !res.success) errEl.textContent = (res && res.message) ? res.message : 'Error saving item.';
}