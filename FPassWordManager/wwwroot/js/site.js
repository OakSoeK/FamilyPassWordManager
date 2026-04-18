

async function apiFetch(url, method, body) {
    method = method || 'GET';
    body = body || null;

    var opts = {
        method: method,
        headers: { 'Content-Type': 'application/json' },
        credentials: 'same-origin'
    };
    if (body) opts.body = JSON.stringify(body);

    try {
        var res = await fetch(url, opts);

        if (res.status === 401 || res.url.indexOf('/Account/Login') !== -1) {
            window.location.href = '/Identity/Account/Login';
            return null;
        }
        if (res.status === 204) return { success: true };
        if (res.status === 403) return { success: false, message: 'You do not have permission.' };
        if (res.status === 404) return { success: false, message: 'Not found.' };

        var contentType = res.headers.get('content-type') || '';
        if (contentType.indexOf('application/json') === -1) {
            return { success: false, message: 'Server error (' + res.status + ')' };
        }

        var text = await res.text();
        if (!text || !text.trim()) return { success: res.ok };

        var data = JSON.parse(text);
        if (!res.ok) return { success: false, message: (data && data.message) ? data.message : 'Error ' + res.status };
        return data;

    } catch (e) {
        console.error('apiFetch error:', e);
        return { success: false, message: 'Network error. Please try again.' };
    }
}

function openModal(id) {
    var el = document.getElementById(id);
    if (!el) return;
    el.classList.add('active');
    document.body.style.overflow = 'hidden';
}

function closeModal(id) {
    var el = document.getElementById(id);
    if (!el) return;
    el.classList.remove('active');
    document.body.style.overflow = '';
}

document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape') {
        document.querySelectorAll('.modal-backdrop.active').forEach(function (m) {
            m.classList.remove('active');
            document.body.style.overflow = '';
        });
    }
});

function timeAgo(dateStr) {
    if (!dateStr) return '—';
    var diff = Date.now() - new Date(dateStr).getTime();
    var m = Math.floor(diff / 60000);
    if (m < 1) return 'just now';
    if (m < 60) return m + 'm ago';
    var h = Math.floor(m / 60);
    if (h < 24) return h + 'h ago';
    var d = Math.floor(h / 24);
    if (d < 30) return d + 'd ago';
    var mo = Math.floor(d / 30);
    if (mo < 12) return mo + 'mo ago';
    return Math.floor(mo / 12) + 'y ago';
}

function formatDate(dateStr) {
    if (!dateStr) return '—';
    return new Date(dateStr).toLocaleDateString('en-GB', {
        day: 'numeric', month: 'short', year: 'numeric'
    });
}

function initials(name) {
    if (!name) return '?';
    return name.split(' ').map(function (p) { return p[0]; }).join('').toUpperCase().slice(0, 2);
}

function escHtml(str) {
    if (!str) return '';
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}
