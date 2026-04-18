let editingCredentialId = null;
let deletingCredentialId = null;
var myFolders = [];
var activeItemType = 'web';   
var activeSubTab = 'logins';
var myItemsLoaded = false;
var DEFAULT_FOLDER = 'My items'; // Default folder for individual items


document.addEventListener('DOMContentLoaded', function () {
    loadDashboard();
});

async function loadDashboard() {
    var mine = await apiFetch('/api/credentials');
    var sharedFolders = await apiFetch('/api/access/shared-with-me');
    var sharedItems = await apiFetch('/api/access/shared-items');

    myFolders = (mine && mine.data) ? mine.data : [];
    var folderShares = (sharedFolders && sharedFolders.data) ? sharedFolders.data : [];
    var itemShares = (sharedItems && sharedItems.data) ? sharedItems.data : [];

    // Exclude the auto-created "My items" folder from the visible credentials grid
    var visibleFolders = myFolders.filter(function (f) { return f.credentialName !== DEFAULT_FOLDER; });

    var soon = 7 * 86400000;
    var expiring = [...folderShares, ...itemShares].filter(function (s) {
        return (new Date(s.expireAt) - Date.now()) < soon;
    }).length;

    document.getElementById('st-folders').textContent = visibleFolders.length;
    document.getElementById('st-shared').textContent = folderShares.length + itemShares.length;
    document.getElementById('st-expiring').textContent = expiring;
    document.getElementById('dash-stats').textContent =
        visibleFolders.length + ' folder' + (visibleFolders.length !== 1 ? 's' : '')
        + ' · ' + (folderShares.length + itemShares.length) + ' shared with you';

    renderCredentials(visibleFolders);
    renderSharedFolders(folderShares);
}

//Tab Switch

function switchTab(tab, btn) {
    if (tab === 'shared-items') { window.location.href = '/SharedItems'; return; }
    if (tab === 'my-items') {
        // Find the default folder and navigate into it
        var folder = myFolders.find(function (f) { return f.credentialName === DEFAULT_FOLDER; });
        if (folder) {
            window.location.href = '/Vault?id=' + folder.credentialId + '&name=' + encodeURIComponent(folder.credentialName);
        } else {
            // If folder doesn't exist, create it and find it
            apiFetch('/api/credentials', 'POST', { credentialName: DEFAULT_FOLDER }).then(function (res) {
                if (res && res.success && res.data) {
                    myFolders.push(res.data);
                    window.location.href = '/Vault?id=' + res.data.credentialId + '&name=' + encodeURIComponent(res.data.credentialName);
                }
            });
        }
        return;
    }
    document.querySelectorAll('.tab-btn').forEach(function (b) { b.classList.remove('active'); });
    btn.classList.add('active');
    document.getElementById('tab-mine').style.display = (tab === 'mine') ? '' : 'none';
    document.getElementById('tab-shared-folders').style.display = (tab === 'shared-folders') ? '' : 'none';
}

function switchSubTab(sub, btn) {
    document.querySelectorAll('.sub-tab-btn').forEach(function (b) { b.classList.remove('active'); });
    btn.classList.add('active');
    activeSubTab = sub;
    document.getElementById('subtab-logins').style.display = (sub === 'logins') ? '' : 'none';
    document.getElementById('subtab-cards').style.display = (sub === 'cards') ? '' : 'none';
    document.getElementById('subtab-keys').style.display = (sub === 'keys') ? '' : 'none';
    loadSubTab(sub);
}

// Modals

function openAddItemModal() {
    selectItemType('web');
    ['aiw-url', 'aiw-username', 'aiw-password', 'aiw-notes',
        'aic-holder', 'aic-number', 'aic-month', 'aic-year', 'aic-cvv', 'aic-pin', 'aic-billing', 'aic-notes',
        'aik-label', 'aik-pin', 'aik-notes'].forEach(function (id) {
            var el = document.getElementById(id);
            if (el) el.value = '';
        });
    document.getElementById('add-item-error').textContent = '';
    openModal('modal-add-item');
}

function selectItemType(type) {
    activeItemType = type;
    ['web', 'card', 'key'].forEach(function (t) {
        document.getElementById('type-btn-' + t).classList.toggle('active', t === type);
        document.getElementById('add-item-' + t).style.display = (t === type) ? '' : 'none';
    });
}

// Check default folder exists
async function ensureDefaultFolder() {
    var existing = myFolders.find(function (f) { return f.credentialName === DEFAULT_FOLDER; });
    if (existing) return existing.credentialId;
    var res = await apiFetch('/api/credentials', 'POST', { credentialName: DEFAULT_FOLDER });
    if (res && res.success && res.data) {
        myFolders.push(res.data);
        return res.data.credentialId;
    }
    return null;
}

async function saveAddItem() {
    var errEl = document.getElementById('add-item-error');
    errEl.textContent = '';

    var folderId = await ensureDefaultFolder();
    if (!folderId) { errEl.textContent = 'Could not create default folder. Try again.'; return; }

    var res;
    if (activeItemType === 'web') {
        var url = document.getElementById('aiw-url').value.trim();
        var user = document.getElementById('aiw-username').value.trim();
        var pass = document.getElementById('aiw-password').value;
        var notes = document.getElementById('aiw-notes').value.trim();
        if (!url || !user || !pass) { errEl.textContent = 'URL, username and password are required.'; return; }
        res = await apiFetch('/api/webcredentials', 'POST', { credentialId: folderId, url: url, username: user, password: pass, notes: notes });

    } else if (activeItemType === 'card') {
        var holder = document.getElementById('aic-holder').value.trim();
        var number = document.getElementById('aic-number').value.replace(/\s/g, '');
        var month = document.getElementById('aic-month').value.trim();
        var year = document.getElementById('aic-year').value.trim();
        var cvv = document.getElementById('aic-cvv').value.trim();
        var pin = document.getElementById('aic-pin').value.trim();
        var billing = document.getElementById('aic-billing').value.trim();
        var cnotes = document.getElementById('aic-notes').value.trim();
        if (!holder || !number || !month || !year || !cvv || !pin) { errEl.textContent = 'All card fields except billing and notes are required.'; return; }
        res = await apiFetch('/api/creditdebitcards', 'POST', { credentialId: folderId, cardHolderName: holder, cardNumber: number, expiryMonth: month, expiryYear: year, cvv: cvv, pin: pin, billingAddress: billing, notes: cnotes });

    } else {
        var label = document.getElementById('aik-label').value.trim();
        var kpin = document.getElementById('aik-pin').value;
        var knotes = document.getElementById('aik-notes').value.trim();
        if (!label || !kpin) { errEl.textContent = 'Label and PIN/secret are required.'; return; }
        res = await apiFetch('/api/securitykeys', 'POST', { credentialId: folderId, label: label, pin: kpin, notes: knotes });
    }

    if (res && res.success) {
        closeModal('modal-add-item');
        myItemsLoaded = false; // force reload::refresh
        // If My items tab is active, reload 
        var activeTab = document.querySelector('.tab-btn.active');
        if (activeTab && activeTab.textContent.trim().startsWith('My items')) {
            myItemsLoaded = true;
            var subMap = { web: 'logins', card: 'cards', key: 'keys' };
            loadSubTab(subMap[activeItemType]);
        }
        // Refresh folder list
        var fresh = await apiFetch('/api/credentials');
        if (fresh && fresh.data) myFolders = fresh.data;
        document.getElementById('st-folders').textContent = myFolders.length;
    } else {
        errEl.textContent = (res && res.message) ? res.message : 'Error saving item.';
    }
}

// Render func

function renderCredentials(folders) {
    var grid = document.getElementById('cred-grid');
    if (!folders.length) {
        grid.innerHTML = '<div class="empty-state">'
            + '<div class="empty-icon"><svg width="28" height="28" viewBox="0 0 28 28" fill="none">'
            + '<rect x="3" y="13" width="22" height="13" rx="3" fill="currentColor" opacity="0.5"/>'
            + '<path d="M8 13V9a6 6 0 0 1 12 0v4" stroke="currentColor" stroke-width="2" stroke-linecap="round" fill="none"/>'
            + '</svg></div>'
            + '<p>Your vault is empty</p>'
            + '<button class="btn-ghost btn-sm" onclick="openNewCredentialModal()">Create your first folder</button>'
            + '</div>';
        return;
    }
    grid.innerHTML = folders.map(function (f, i) {
        return '<div class="cred-card" style="animation-delay:' + (i * 40) + 'ms"'
            + ' data-id="' + f.credentialId + '"'
            + ' data-name="' + escHtml(f.credentialName) + '">'
            + '<div class="cred-card-top">'
            + '<div class="cred-folder-icon">'
            + '<svg width="18" height="18" viewBox="0 0 18 18" fill="none"><path d="M2 5.5A1.5 1.5 0 0 1 3.5 4H7l2 2h5.5A1.5 1.5 0 0 1 16 7.5v6A1.5 1.5 0 0 1 14.5 15h-11A1.5 1.5 0 0 1 2 13.5V5.5z" fill="currentColor" opacity="0.7"/></svg>'
            + '</div>'
            + '<div class="cred-card-actions">'
            + '<button class="icon-btn" title="Rename"'
            + ' data-id="' + f.credentialId + '" data-name="' + escHtml(f.credentialName) + '"'
            + ' onclick="event.stopPropagation(); openEditCredentialModal(this.dataset.id, this.dataset.name)">'
            + '<svg width="13" height="13" viewBox="0 0 14 14" fill="none"><path d="M9.5 2.5l2 2L4 12H2v-2L9.5 2.5z" stroke="currentColor" stroke-width="1.3" stroke-linecap="round" stroke-linejoin="round"/></svg>'
            + '</button>'
            + '<button class="icon-btn icon-btn-danger" title="Delete"'
            + ' data-id="' + f.credentialId + '" data-name="' + escHtml(f.credentialName) + '"'
            + ' onclick="event.stopPropagation(); openDeleteModal(this.dataset.id, this.dataset.name)">'
            + '<svg width="13" height="13" viewBox="0 0 14 14" fill="none"><path d="M2 3.5h10M5.5 3.5V2h3v1.5M3 3.5l.75 8.5h6.5L11 3.5" stroke="currentColor" stroke-width="1.3" stroke-linecap="round" stroke-linejoin="round"/></svg>'
            + '</button>'
            + '</div></div>'
            + '<div class="cred-card-name">' + escHtml(f.credentialName) + '</div>'
            + '<div class="cred-card-meta">Edited ' + timeAgo(f.lastEditedAt) + '</div>'
            + '</div>';
    }).join('');

    grid.onclick = function (e) {
        var card = e.target.closest('.cred-card');
        if (!card || e.target.closest('.cred-card-actions')) return;
        window.location.href = '/Vault?id=' + card.dataset.id + '&name=' + encodeURIComponent(card.dataset.name);
    };
}

function renderSharedFolders(items) {
    var list = document.getElementById('shared-folders-list');
    if (!items.length) {
        list.innerHTML = '<div class="empty-state"><p>No folders shared with you yet.</p></div>';
        return;
    }
    list.innerHTML = items.map(function (s) {
        return '<div class="shared-row"'
            + ' data-id="' + s.credentialId + '"'
            + ' data-name="' + escHtml(s.credentialName) + '">'
            + '<div class="shared-row-left">'
            + '<div class="shared-avatar">' + initials(s.ownerName) + '</div>'
            + '<div>'
            + '<div class="shared-row-name">' + escHtml(s.credentialName) + '</div>'
            + '<div class="shared-row-meta">from ' + escHtml(s.ownerName) + '</div>'
            + '</div></div>'
            + '<div class="shared-row-right">'
            + '<span class="badge badge-' + (s.permissionLevel === 'Edit' ? 'edit' : 'view') + '">' + s.permissionLevel + '</span>'
            + '<span class="shared-expires' + (s.isExpired ? ' expired' : '') + '">'
            + (s.isExpired ? 'Expired' : 'Expires ' + formatDate(s.expireAt)) + '</span>'
            + '</div></div>';
    }).join('');
    list.onclick = function (e) {
        var row = e.target.closest('.shared-row');
        if (!row) return;
        window.location.href = '/Vault?id=' + row.dataset.id + '&name=' + encodeURIComponent(row.dataset.name);
    };
}

// Modals

function openNewCredentialModal() {
    editingCredentialId = null;
    document.getElementById('modal-cred-title').textContent = 'New folder';
    document.getElementById('cred-name-input').value = '';
    document.getElementById('cred-error').textContent = '';
    openModal('modal-credential');
}

function openEditCredentialModal(id, name) {
    editingCredentialId = id;
    document.getElementById('modal-cred-title').textContent = 'Rename folder';
    document.getElementById('cred-name-input').value = name;
    document.getElementById('cred-error').textContent = '';
    openModal('modal-credential');
}

async function saveCredential() {
    var name = document.getElementById('cred-name-input').value.trim();
    var errEl = document.getElementById('cred-error');
    if (!name) { errEl.textContent = 'Name is required.'; return; }
    var url = editingCredentialId ? '/api/credentials/' + editingCredentialId : '/api/credentials';
    var method = editingCredentialId ? 'PUT' : 'POST';
    var res = await apiFetch(url, method, { credentialName: name });
    if (res && res.success) { closeModal('modal-credential'); loadDashboard(); }
    else errEl.textContent = (res && res.message) ? res.message : 'Error saving.';
}

function openDeleteModal(id, name) {
    deletingCredentialId = id;
    document.getElementById('delete-cred-name').textContent = name;
    openModal('modal-delete');
}

async function confirmDelete() {
    var res = await apiFetch('/api/credentials/' + deletingCredentialId, 'DELETE');
    if (res && res.success) { closeModal('modal-delete'); loadDashboard(); }
}

function toggleReveal(inputId, btn) {
    var inp = document.getElementById(inputId);
    inp.type = (inp.type === 'text') ? 'password' : 'text';
    btn.style.opacity = (inp.type === 'text') ? '1' : '0.5';
}