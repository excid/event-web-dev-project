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

// ── Invitation Accept / Decline ─────────────────────────────────
function handleInvitation(btn, action) {
    const card = btn.closest('.invitation-card');
    const actionsRow = card.querySelector('.invitation-card__actions');
    const statusBadge = card.querySelector('.status-badge');

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
    // Find pending stat value in received panel
    const panel = document.getElementById('subtab-received');
    if (!panel) return;
    const statCards = panel.querySelectorAll('.stat-card');
    // statCards[0]=Total, [1]=Pending, [2]=Accepted, [3]=Rejected
    const pendingEl = statCards[1]?.querySelector('.stat-value');
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
