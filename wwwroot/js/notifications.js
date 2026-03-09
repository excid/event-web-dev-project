/**
 * notifications.js
 * Handles the bottom-right notification popup panel.
 * Polls the server every 30 seconds for new unread notifications.
 * Depends on: anti-forgery token present on the page.
 */

(function () {
    'use strict';

    // ── Config ──────────────────────────────────────────────────────────────
    const POLL_INTERVAL_MS = 1_000;   // check every 30 s
    const MAX_TOASTS       = 3;        // max simultaneous toast popups

    // ── State ───────────────────────────────────────────────────────────────
    let knownIds     = new Set();  // IDs we have already shown / rendered
    let isPanelOpen  = false;
    let activeToasts = 0;

    // ── DOM refs (set in init) ───────────────────────────────────────────────
    let fab, badge, panel, list;

    // ── Anti-forgery ─────────────────────────────────────────────────────────
    function getToken() {
        return document.querySelector('input[name="__RequestVerificationToken"]')?.value ?? '';
    }

    // ── Fetch helpers ────────────────────────────────────────────────────────
    async function fetchUnread() {
        try {
            const res = await fetch('/Notification/GetUnread', { credentials: 'same-origin' });
            if (!res.ok) return [];
            return await res.json();
        } catch {
            return [];
        }
    }

    async function postMarkRead(id) {
        await fetch('/Notification/MarkRead', {
            method: 'POST',
            credentials: 'same-origin',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: new URLSearchParams({ id, __RequestVerificationToken: getToken() })
        });
    }

    async function postMarkAllRead() {
        await fetch('/Notification/MarkAllRead', {
            method: 'POST',
            credentials: 'same-origin',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: new URLSearchParams({ __RequestVerificationToken: getToken() })
        });
    }

    // ── Type → icon SVG ─────────────────────────────────────────────────────
    const ICONS = {
        InvitationReceived:  '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/><polyline points="22,6 12,13 2,6"/></svg>',
        InvitationAccepted:  '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><polyline points="20 6 9 17 4 12"/></svg>',
        InvitationRejected:  '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>',
        ApplicationReceived: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>',
        ApplicationAccepted: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><polyline points="20 6 9 17 4 12"/></svg>',
        ApplicationRejected: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>',
        ReviewReceived:      '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"/></svg>',
        _default:            '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9"/><path d="M13.73 21a2 2 0 0 1-3.46 0"/></svg>'
    };

    function iconFor(type) {
        return ICONS[type] ?? ICONS._default;
    }

    function cssClassFor(type) {
        const map = {
            InvitationReceived:  'type-invitation-received',
            InvitationAccepted:  'type-invitation-accepted',
            InvitationRejected:  'type-invitation-rejected',
            ApplicationReceived: 'type-application-received',
            ApplicationAccepted: 'type-application-accepted',
            ApplicationRejected: 'type-application-rejected',
            ReviewReceived:      'type-review-received'
        };
        return map[type] ?? 'type-default';
    }

    // ── Time formatting ──────────────────────────────────────────────────────
    function parseDate(dateStr) {
        // Try ISO with Z first; fall back to appending Z for UTC strings lacking it
        let d = new Date(dateStr);
        if (isNaN(d.getTime())) d = new Date(dateStr + 'Z');
        return d;
    }

    function timeAgo(dateStr) {
        const diff = (Date.now() - parseDate(dateStr).getTime()) / 1000;
        if (isNaN(diff) || diff < 0) return 'just now';
        if (diff < 60)     return 'just now';
        if (diff < 3600)   return `${Math.floor(diff / 60)}m ago`;
        if (diff < 86400)  return `${Math.floor(diff / 3600)}h ago`;
        return `${Math.floor(diff / 86400)}d ago`;
    }

    // ── Render helpers ───────────────────────────────────────────────────────
    function buildItem(n) {
        const li = document.createElement('li');
        li.className = 'notif-item' + (n.isRead ? '' : ' is-unread');
        li.dataset.id = n.id;

        const actionUrl = n.actionUrl || '#';

        li.innerHTML = `
            <div class="notif-icon ${cssClassFor(n.type)}">${iconFor(n.type)}</div>
            <div class="notif-body">
                <div class="notif-title">${escHtml(n.title)}</div>
                <div class="notif-message">${escHtml(n.message)}</div>
                <div class="notif-time">${timeAgo(n.createdAt)}</div>
            </div>
            <button class="notif-dismiss" title="Mark as read" aria-label="Mark as read">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
            </button>
        `;

        // Clicking the main body navigates if there's a URL
        li.querySelector('.notif-body').addEventListener('click', () => {
            markRead(n.id, li);
            if (actionUrl !== '#') window.location.href = actionUrl;
        });

        li.querySelector('.notif-dismiss').addEventListener('click', (e) => {
            e.stopPropagation();
            markRead(n.id, li);
        });

        return li;
    }

    function escHtml(str) {
        return String(str)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
    }

    // ── Mark one read ────────────────────────────────────────────────────────
    function markRead(id, el) {
        postMarkRead(id);
        if (el) {
            el.classList.remove('is-unread');
            el.querySelector('.notif-dismiss')?.remove();
        }
        knownIds.delete(id);   // allow re-poll to re-count correctly
        refreshBadge();
    }

    // ── Badge update ─────────────────────────────────────────────────────────
    function refreshBadge() {
        const unreadCount = list.querySelectorAll('.notif-item.is-unread').length;
        badge.textContent = unreadCount > 99 ? '99+' : String(unreadCount);
        if (unreadCount === 0) {
            badge.hidden = true;
        } else {
            badge.hidden = false;
        }
    }

    // ── Toast popup ──────────────────────────────────────────────────────────
    function showToast(n) {
        if (activeToasts >= MAX_TOASTS) return;
        activeToasts++;

        const toast = document.createElement('div');
        toast.className = 'notif-toast';
        toast.innerHTML = `
            <div class="toast-icon">${iconFor(n.type)}</div>
            <div class="toast-body">
                <div class="toast-title">${escHtml(n.title)}</div>
                <div class="toast-msg">${escHtml(n.message)}</div>
            </div>
        `;

        if (n.actionUrl && n.actionUrl !== '#') {
            toast.addEventListener('click', () => window.location.href = n.actionUrl);
            toast.style.cursor = 'pointer';
        }

        document.body.appendChild(toast);

        // Move existing toasts up
        const allToasts = document.querySelectorAll('.notif-toast');
        allToasts.forEach((t, i) => {
            const idx = allToasts.length - 1 - i;
            t.style.bottom = `${92 + idx * 76}px`;
        });

        setTimeout(() => {
            toast.style.opacity = '0';
            toast.style.transition = 'opacity 0.3s';
            setTimeout(() => {
                toast.remove();
                activeToasts = Math.max(0, activeToasts - 1);
                // Re-stack remaining toasts
                document.querySelectorAll('.notif-toast').forEach((t, i, arr) => {
                    const idx = arr.length - 1 - i;
                    t.style.bottom = `${92 + idx * 76}px`;
                });
            }, 300);
        }, 4000);
    }

    // ── Poll & update ─────────────────────────────────────────────────────────
    async function poll() {
        const notifications = await fetchUnread();
        if (!Array.isArray(notifications)) return;

        // Detect truly new notifications (not seen in previous poll)
        const fresh = notifications.filter(n => !knownIds.has(n.id));

        // Update known IDs
        notifications.forEach(n => knownIds.add(n.id));

        // Re-render panel list
        renderList(notifications);

        // Show toasts for new ones (only if panel is closed)
        if (!isPanelOpen) {
            fresh.forEach(n => showToast(n));
        }
    }

    function renderList(notifications) {
        list.innerHTML = '';

        if (!notifications.length) {
            const empty = document.createElement('div');
            empty.className = 'notif-empty';
            empty.textContent = 'You\'re all caught up! 🎉';
            list.appendChild(empty);
        } else {
            notifications.forEach(n => list.appendChild(buildItem(n)));
        }

        refreshBadge();
    }

    // ── Panel toggle ──────────────────────────────────────────────────────────
    function togglePanel() {
        isPanelOpen = !isPanelOpen;
        panel.hidden = !isPanelOpen;

        if (isPanelOpen) {
            // Dismiss any queued toasts when panel opens
            document.querySelectorAll('.notif-toast').forEach(t => t.remove());
            activeToasts = 0;
        }
    }

    // ── Init ─────────────────────────────────────────────────────────────────
    function init() {
        fab   = document.getElementById('notif-fab');
        badge = document.getElementById('notif-badge');
        panel = document.getElementById('notif-panel');
        list  = document.getElementById('notif-list');

        if (!fab || !badge || !panel || !list) {
            // Notification UI markup is missing — likely the user is not authenticated
            if (fab || badge || panel || list) {
                console.warn('Notification system: one or more required DOM elements are missing (#notif-fab, #notif-badge, #notif-panel, #notif-list).');
            }
            return;
        }

        fab.addEventListener('click', (e) => {
            e.stopPropagation();
            togglePanel();
        });

        // Mark-all button
        document.getElementById('notif-mark-all')?.addEventListener('click', async () => {
            await postMarkAllRead();
            list.querySelectorAll('.notif-item').forEach(el => {
                el.classList.remove('is-unread');
                el.querySelector('.notif-dismiss')?.remove();
            });
            knownIds.clear();
            refreshBadge();
        });

        // Close panel when clicking outside
        document.addEventListener('click', (e) => {
            if (isPanelOpen && !panel.contains(e.target) && e.target !== fab) {
                isPanelOpen = false;
                panel.hidden = true;
            }
        });

        // Initial poll then recurring
        poll();
        setInterval(poll, POLL_INTERVAL_MS);
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
