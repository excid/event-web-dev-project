// myboard.js

// ── Main Tab Switching ──────────────────────────────────────────
(function () {
    const tabBtns = document.querySelectorAll('.tab-btn');
    const tabPanels = document.querySelectorAll('.tab-panel');

    tabBtns.forEach(btn => {
        btn.addEventListener('click', () => {
            const target = btn.dataset.tab;

            tabBtns.forEach(b => b.classList.remove('active'));
            tabPanels.forEach(p => p.classList.remove('active'));

            btn.classList.add('active');
            const panel = document.getElementById('tab-' + target);
            if (panel) panel.classList.add('active');
        });
    });
})();

// ── Sub-Tab Switching (Invitations: Received / Sent) ───────────
(function () {
    const subtabBtns = document.querySelectorAll('.subtab-btn');
    const subtabPanels = document.querySelectorAll('.subtab-panel');

    subtabBtns.forEach(btn => {
        btn.addEventListener('click', () => {
            const target = btn.dataset.subtab;

            subtabBtns.forEach(b => b.classList.remove('active'));
            subtabPanels.forEach(p => p.classList.remove('active'));

            btn.classList.add('active');
            const panel = document.getElementById('subtab-' + target);
            if (panel) panel.classList.add('active');
        });
    });
})();

// ── Confirmation Modal ──────────────────────────────────────────
let _pendingAction = null;
let _pendingBtn    = null;

function showConfirmModal(btn, action) {
    const card  = btn.closest('.invitation-card');
    const title = card.querySelector('.invitation-card__title')?.textContent.trim() || 'this invitation';

    _pendingAction = action;
    _pendingBtn    = btn;

    const modal       = document.getElementById('confirm-modal');
    const modalIcon   = document.getElementById('confirm-modal-icon');
    const modalTitle  = document.getElementById('confirm-modal-title');
    const modalBody   = document.getElementById('confirm-modal-body');
    const confirmBtn  = document.getElementById('confirm-modal-confirm');

    if (action === 'accept') {
        modalIcon.innerHTML = `<svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="#16a34a" stroke-width="2.5"><polyline points="20 6 9 17 4 12"/></svg>`;
        modalIcon.className = 'confirm-modal__icon confirm-modal__icon--accept';
        modalTitle.textContent = 'Accept Invitation';
        modalBody.innerHTML = `Are you sure you want to accept the invitation for <strong>"${title}"</strong>? The organizer will be notified and will provide you with more details.`;
        confirmBtn.textContent = 'Accept';
        confirmBtn.className = 'btn-modal-confirm btn-modal-confirm--accept';
    } else {
        modalIcon.innerHTML = `<svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="#dc2626" stroke-width="2.5"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>`;
        modalIcon.className = 'confirm-modal__icon confirm-modal__icon--decline';
        modalTitle.textContent = 'Decline Invitation';
        modalBody.innerHTML = `Are you sure you want to decline the invitation for <strong>"${title}"</strong>? This action cannot be undone.`;
        confirmBtn.textContent = 'Decline';
        confirmBtn.className = 'btn-modal-confirm btn-modal-confirm--decline';
    }

    modal.classList.add('active');
    document.body.style.overflow = 'hidden';
}

function closeConfirmModal() {
    const modal = document.getElementById('confirm-modal');
    modal.classList.remove('active');
    document.body.style.overflow = '';
    _pendingAction = null;
    _pendingBtn    = null;
}

function confirmModalAction() {
    if (_pendingAction && _pendingBtn) {
        handleInvitation(_pendingBtn, _pendingAction);
    }
    closeConfirmModal();
}

// Close modal on backdrop click
document.addEventListener('DOMContentLoaded', () => {
    const modal = document.getElementById('confirm-modal');
    if (modal) {
        modal.addEventListener('click', (e) => {
            if (e.target === modal) closeConfirmModal();
        });
    }

    // Close on Escape key
    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape') closeConfirmModal();
    });
});

// ── Invitation Accept / Decline ─────────────────────────────────
async function handleInvitation(btn, action) {
    const card = btn.closest('.invitation-card');
    const invitationId = card?.dataset.invitationId;
    const actionsRow = card.querySelector('.invitation-card__actions');
    const statusBadge = card.querySelector('.status-badge');

    // Disable buttons while submitting
    card.querySelectorAll('button').forEach(b => b.disabled = true);

    try {
        const token = document.getElementById('invitation-antiforgery-token')?.value ?? '';
        const body = new URLSearchParams({ invitationId, action });
        const resp = await fetch('/Invitation/Respond', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'RequestVerificationToken': token,
            },
            body,
        });
        const result = await resp.json();

        if (!result.success) {
            card.querySelectorAll('button').forEach(b => b.disabled = false);
            return;
        }
    } catch {
        card.querySelectorAll('button').forEach(b => b.disabled = false);
        return;
    }

    // Replace action buttons with a notice
    if (actionsRow) actionsRow.remove();

    // Update status badge
    if (statusBadge) {
        statusBadge.className = 'status-badge';
        if (action === 'accept') {
            statusBadge.classList.add('status-badge--accepted');
            statusBadge.innerHTML = `
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><polyline points="20 6 9 17 4 12"/></svg>
                <span>Accepted</span>`;
        } else {
            statusBadge.classList.add('status-badge--rejected');
            statusBadge.innerHTML = `
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
                <span>Rejected</span>`;
        }
    }

    // Append notice
    const notice = document.createElement('div');
    if (action === 'accept') {
        notice.className = 'invitation-card__notice status-badge--accepted';
        notice.textContent = '✓ You accepted this invitation. The organizer will contact you with more details.';
    } else {
        notice.className = 'invitation-card__notice status-badge--rejected';
        notice.textContent = 'You declined this invitation.';
    }
    notice.style.animation = 'fadeIn 0.25s ease';
    card.appendChild(notice);

    // Update pending count in subtab stats (visual only)
    updateInvitationCounts(action);
}

function updateInvitationCounts(action) {
    const panel = document.getElementById('subtab-received');
    if (!panel) return;
    const statCards = panel.querySelectorAll('.stat-card');
    const pendingEl  = statCards[1]?.querySelector('.stat-value');
    const acceptedEl = statCards[2]?.querySelector('.stat-value');
    const rejectedEl = statCards[3]?.querySelector('.stat-value');

    if (pendingEl) {
        const cur = parseInt(pendingEl.textContent) || 0;
        pendingEl.textContent = Math.max(0, cur - 1);
    }
    if (action === 'accept' && acceptedEl) {
        const cur = parseInt(acceptedEl.textContent) || 0;
        acceptedEl.textContent = cur + 1;
    } else if (action === 'decline' && rejectedEl) {
        const cur = parseInt(rejectedEl.textContent) || 0;
        rejectedEl.textContent = cur + 1;
    }
}