
var allItems = [];
var activeFilter = 'all';

document.addEventListener('DOMContentLoaded', function () {
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
        document.getElementById('page-content').style.display = '';
        loadItems();
    } else {
        err.textContent = (res && res.message) ? res.message : 'Incorrect PIN.';
        document.getElementById('pin-input').value = '';
        document.getElementById('pin-input').focus();
    }
}


async function loadItems() {
    var res = await apiFetch('/api/access/shared-items');
    allItems = (res && res.data) ? res.data : [];
    // Deduplicate by itemId in case of any server-side duplicates
    var seen = {};
    allItems = allItems.filter(function (i) {
        if (seen[i.itemId]) return false;
        seen[i.itemId] = true;
        return true;
    });
    renderItems();
}

function setFilter(type, btn) {
    activeFilter = type;
    document.querySelectorAll('.filter-btn').forEach(function (b) { b.classList.remove('active'); });
    btn.classList.add('active');
    renderItems();
}

function renderItems() {
    var filtered = activeFilter === 'all'
        ? allItems
        : allItems.filter(function (i) { return i.itemType === activeFilter; });

    var countEl = document.getElementById('items-count');
    if (countEl) countEl.textContent = filtered.length + ' item' + (filtered.length !== 1 ? 's' : '');

    var list = document.getElementById('items-list');
    if (!filtered.length) {
        list.innerHTML = '<div class="empty-state"><p>No items here yet.</p></div>';
        return;
    }

    var typeIcons = {
        web: '<svg width="15" height="15" viewBox="0 0 16 16" fill="none"><circle cx="8" cy="8" r="6.5" stroke="currentColor" stroke-width="1.3" fill="none"/><path d="M8 1.5C6.5 4.5 6 6 6 8s.5 3.5 2 6.5M8 1.5C9.5 4.5 10 6 10 8s-.5 3.5-2 6.5M1.5 8h13" stroke="currentColor" stroke-width="1.1" stroke-linecap="round"/></svg>',
        card: '<svg width="15" height="15" viewBox="0 0 16 16" fill="none"><rect x="1" y="3.5" width="14" height="9" rx="1.5" stroke="currentColor" stroke-width="1.3" fill="none"/><path d="M1 7h14" stroke="currentColor" stroke-width="1.3"/><rect x="2.5" y="9" width="3" height="1.5" rx="0.5" fill="currentColor" opacity="0.5"/></svg>',
        key: '<svg width="15" height="15" viewBox="0 0 16 16" fill="none"><circle cx="6" cy="6" r="3.5" stroke="currentColor" stroke-width="1.3" fill="none"/><path d="M8.5 8.5L14 14M11.5 11l1.5 1.5" stroke="currentColor" stroke-width="1.3" stroke-linecap="round"/></svg>'
    };
    var typeLabels = { web: 'Web login', card: 'Card', key: 'Security key' };
    var histEp = { web: 'webcredentials', card: 'creditdebitcards', key: 'securitykeys' };

    list.innerHTML = filtered.map(function (item) {
        var safeItem = JSON.stringify(item).replace(/'/g, '&#39;');
        var canEdit = (item.permissionLevel === 'Edit');

        var html = '<div class="item-row" style="cursor:pointer" data-item=\'' + safeItem + '\'>'
            + '<div class="item-icon item-icon-' + item.itemType + '">' + typeIcons[item.itemType] + '</div>'
            + '<div class="item-info">'
            + '<div class="item-primary">' + escHtml(item.itemTitle) + '</div>'
            + '<div class="item-secondary">'
            + '<span class="shared-item-type-tag">' + typeLabels[item.itemType] + '</span>'
            + ' · ' + escHtml(item.itemSubtitle)
            + '</div>'
            + '</div>'
            + '<div class="item-actions" onclick="event.stopPropagation()">'
            + '<span class="badge badge-' + (canEdit ? 'edit' : 'view') + '">' + item.permissionLevel + '</span>'
            + '<span class="shared-expires' + (item.isExpired ? ' expired' : '') + '" style="font-size:12px;margin-left:6px">'
            + (item.isExpired ? 'Expired' : 'Exp. ' + formatDate(item.expireAt))
            + '</span>'
            // History button
            + '<button class="icon-btn" title="History"'
            + ' data-type="' + item.itemType + '" data-id="' + item.itemId + '" data-name="' + escHtml(item.itemTitle) + '"'
            + ' onclick="loadSharedHistory(this.dataset.type,this.dataset.id,this.dataset.name)">'
            + '<svg width="13" height="13" viewBox="0 0 14 14" fill="none"><circle cx="7" cy="7" r="5.5" stroke="currentColor" stroke-width="1.2" fill="none"/><path d="M7 4v3l2 1.5" stroke="currentColor" stroke-width="1.2" stroke-linecap="round"/></svg>'
            + '</button>';

        // Edit button
        if (canEdit) {
            html += '<button class="icon-btn" title="Edit" data-item=\'' + safeItem + '\' onclick="openSharedEdit(JSON.parse(this.dataset.item))">'
                + '<svg width="13" height="13" viewBox="0 0 14 14" fill="none"><path d="M9.5 2.5l2 2L4 12H2v-2L9.5 2.5z" stroke="currentColor" stroke-width="1.3" stroke-linecap="round" stroke-linejoin="round"/></svg>'
                + '</button>';
        }

        html += '<span class="item-owner-tag">from ' + escHtml(item.ownerName) + '</span>'
            + '</div>'
            + '</div>';
        return html;
    }).join('');

    list.onclick = function (e) {
        if (e.target.closest('.icon-btn, button')) return;
        var row = e.target.closest('.item-row');
        if (!row || !row.dataset.item) return;
        openItemDetail(JSON.parse(row.dataset.item));
    };
}

// Detail modal 

async function openItemDetail(item) {
    var typeLabels = { web: 'Web login', card: 'Credit / debit card', key: 'Security key' };
    document.getElementById('detail-title').textContent = typeLabels[item.itemType];
    document.getElementById('detail-body').innerHTML = '<div class="loading-row"><span class="spinner"></span></div>';
    openModal('modal-detail');

    // Fetch full item to get all fields including notes
    var ep = { web: 'webcredentials', card: 'creditdebitcards', key: 'securitykeys' }[item.itemType];
    var res = await apiFetch('/api/' + ep + '/' + item.itemId);
    var full = (res && res.data) ? res.data : null;

    var rows = [];
    if (item.itemType === 'web') {
        rows = [
            { label: 'Username', value: escHtml(full ? full.username : item.itemTitle) },
            { label: 'URL', value: '<a href="' + escHtml(full ? full.url : item.itemSubtitle) + '" target="_blank" rel="noopener">' + escHtml(full ? full.url : item.itemSubtitle) + '</a>' },
            { label: 'Password', value: revealBtn('/api/webcredentials/' + item.itemId + '/reveal', 'password') },
            { label: 'Notes', value: (full && full.notes) ? escHtml(full.notes) : '—', muted: !(full && full.notes) },
            { label: 'Shared by', value: escHtml(item.ownerName), muted: true },
            { label: 'Expires', value: formatDate(item.expireAt), muted: true }
        ];
    } else if (item.itemType === 'card') {
        rows = [
            { label: 'Cardholder', value: escHtml(full ? full.cardHolderName : item.itemTitle) },
            { label: 'Expiry', value: full ? (full.expiryMonth + ' / ' + full.expiryYear) : '—', mono: true },
            { label: 'Card number', value: revealBtn('/api/creditdebitcards/' + item.itemId + '/reveal', 'cardNumber') },
            { label: 'CVV', value: revealBtn('/api/creditdebitcards/' + item.itemId + '/reveal', 'cvv') },
            { label: 'PIN', value: revealBtn('/api/creditdebitcards/' + item.itemId + '/reveal', 'pin') },
            { label: 'Billing', value: (full && full.billingAddress) ? escHtml(full.billingAddress) : '—', muted: !(full && full.billingAddress) },
            { label: 'Notes', value: (full && full.notes) ? escHtml(full.notes) : '—', muted: !(full && full.notes) },
            { label: 'Shared by', value: escHtml(item.ownerName), muted: true },
            { label: 'Expires', value: formatDate(item.expireAt), muted: true }
        ];
    } else {
        rows = [
            { label: 'Label', value: escHtml(full ? full.label : item.itemTitle) },
            { label: 'Secret', value: revealBtn('/api/securitykeys/' + item.itemId + '/reveal', 'pin') },
            { label: 'Notes', value: (full && full.notes) ? escHtml(full.notes) : '—', muted: !(full && full.notes) },
            { label: 'Shared by', value: escHtml(item.ownerName), muted: true },
            { label: 'Expires', value: formatDate(item.expireAt), muted: true }
        ];
    }

    document.getElementById('detail-body').innerHTML = rows.map(function (r) {
        return '<div class="detail-row">'
            + '<div class="detail-label">' + escHtml(r.label) + '</div>'
            + '<div class="detail-value' + (r.mono ? ' detail-mono' : '') + (r.muted ? ' detail-muted' : '') + '">' + r.value + '</div>'
            + '</div>';
    }).join('');
}

function revealBtn(endpoint, field) {
    return '<button class="btn-ghost btn-sm" style="padding:3px 10px;font-size:12px"'
        + ' onclick="revealField(this,\'' + endpoint + '\',\'' + field + '\')">Click to reveal</button>';
}

async function revealField(btn, endpoint, field) {
    btn.textContent = '…'; btn.disabled = true;
    var res = await apiFetch(endpoint);
    if (res && res.success && res.data) {
        btn.outerHTML = '<span class="detail-mono">' + escHtml(res.data[field]) + '</span>';
    } else { btn.textContent = 'Error'; btn.disabled = false; }
}

// History modal 

async function loadSharedHistory(type, id, name) {
    openModal('modal-history');
    var body = document.getElementById('history-body');
    body.innerHTML = '<div class="loading-row"><span class="spinner"></span></div>';
    var ep = { web: 'webcredentials', card: 'creditdebitcards', key: 'securitykeys' }[type];
    var res = await apiFetch('/api/' + ep + '/' + id + '/history');
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

//  Edit modal

var editingSharedItem = null;

async function openSharedEdit(item) {
    editingSharedItem = item;
    var type = item.itemType;

    document.getElementById('se-web-panel').style.display = (type === 'web') ? '' : 'none';
    document.getElementById('se-card-panel').style.display = (type === 'card') ? '' : 'none';
    document.getElementById('se-key-panel').style.display = (type === 'key') ? '' : 'none';

    var titles = { web: 'Edit login', card: 'Edit card', key: 'Edit key' };
    document.getElementById('se-modal-title').textContent = titles[type];
    document.getElementById('se-error').textContent = '';

    // Fetch full item data
    var ep = { web: 'webcredentials', card: 'creditdebitcards', key: 'securitykeys' }[type];
    var res = await apiFetch('/api/' + ep + '/' + item.itemId);
    var full = (res && res.data) ? res.data : null;

    if (type === 'web') {
        document.getElementById('se-url').value = (full ? full.url : item.itemSubtitle) || '';
        document.getElementById('se-username').value = (full ? full.username : item.itemTitle) || '';
        document.getElementById('se-password').value = '';
        document.getElementById('se-notes').value = (full ? full.notes : '') || '';
    } else if (type === 'card') {
        document.getElementById('se-holder').value = (full ? full.cardHolderName : item.itemTitle) || '';
        document.getElementById('se-month').value = (full ? full.expiryMonth : '') || '';
        document.getElementById('se-year').value = (full ? full.expiryYear : '') || '';
        document.getElementById('se-number').value = '';
        document.getElementById('se-cvv').value = '';
        document.getElementById('se-pin').value = '';
        document.getElementById('se-billing').value = (full ? full.billingAddress : '') || '';
        document.getElementById('se-cnotes').value = (full ? full.notes : '') || '';
    } else {
        document.getElementById('se-label').value = (full ? full.label : item.itemTitle) || '';
        document.getElementById('se-kpin').value = '';
        document.getElementById('se-knotes').value = (full ? full.notes : '') || '';
    }
    openModal('modal-shared-edit');
}

async function saveSharedEdit() {
    var item = editingSharedItem;
    var type = item.itemType;
    var errEl = document.getElementById('se-error');
    errEl.textContent = '';
    var res;

    if (type === 'web') {
        var url = document.getElementById('se-url').value.trim();
        var user = document.getElementById('se-username').value.trim();
        var pass = document.getElementById('se-password').value;
        var notes = document.getElementById('se-notes').value.trim();
        if (!url || !user) { errEl.textContent = 'URL and username are required.'; return; }
        res = await apiFetch('/api/webcredentials/' + item.itemId, 'PUT',
            { url: url, username: user, password: pass || null, notes: notes });

    } else if (type === 'card') {
        var holder = document.getElementById('se-holder').value.trim();
        var month = document.getElementById('se-month').value.trim();
        var year = document.getElementById('se-year').value.trim();
        var number = document.getElementById('se-number').value.replace(/\s/g, '');
        var cvv = document.getElementById('se-cvv').value.trim();
        var pin = document.getElementById('se-pin').value.trim();
        var billing = document.getElementById('se-billing').value.trim();
        var cnotes = document.getElementById('se-cnotes').value.trim();
        if (!holder || !month || !year) { errEl.textContent = 'Cardholder, month and year are required.'; return; }
        res = await apiFetch('/api/creditdebitcards/' + item.itemId, 'PUT',
            {
                cardHolderName: holder, cardNumber: number || null, expiryMonth: month, expiryYear: year,
                cvv: cvv || null, pin: pin || null, billingAddress: billing, notes: cnotes
            });

    } else {
        var label = document.getElementById('se-label').value.trim();
        var kpin = document.getElementById('se-kpin').value;
        var knotes = document.getElementById('se-knotes').value.trim();
        if (!label) { errEl.textContent = 'Label is required.'; return; }
        res = await apiFetch('/api/securitykeys/' + item.itemId, 'PUT',
            { label: label, pin: kpin || null, notes: knotes });
    }

    if (res && res.success) {
        closeModal('modal-shared-edit');

        // Update the matching item
        if (res.data) {
            var updated = res.data;
            allItems = allItems.map(function (i) {
                if (i.itemId !== item.itemId) return i;
                var patched = Object.assign({}, i);
                if (type === 'web') {
                    patched.itemTitle = updated.username || i.itemTitle;
                    patched.itemSubtitle = updated.url || i.itemSubtitle;
                } else if (type === 'card') {
                    patched.itemTitle = updated.cardHolderName || i.itemTitle;
                    patched.itemSubtitle = '**** **** **** ???? · ' + (updated.expiryMonth || '') + '/' + (updated.expiryYear || '');
                } else {
                    patched.itemTitle = updated.label || i.itemTitle;
                    patched.itemSubtitle = updated.notes || i.itemSubtitle || 'Security key';
                }
                return patched;
            });
        }

        renderItems(); // re-render with updated data
        // background reload for sync
        loadItems();
    } else {
        errEl.textContent = (res && res.message) ? res.message : 'Error saving.';
    }
}