// events.js
// Handles all user actions via fetch() AJAX — no page reloads needed.
// Depends on: icons.js (must be loaded first)

const EventBoard = (() => {

    // ─── Helpers ────────────────────────────────────────────────────────────

    // Reads the ASP.NET anti-forgery token from the hidden input on the page.
    // Every POST request must include this or the server will reject it (403).
    function getAntiForgeryToken() {
        return document.querySelector('input[name="__RequestVerificationToken"]')?.value ?? '';
    }

    // Sends a POST request to the controller and returns the parsed JSON response.
    async function post(url, data) {
        const body = new URLSearchParams({
            ...data,
            __RequestVerificationToken: getAntiForgeryToken()
        });

        const res = await fetch(url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body
        });

        if (!res.ok) throw new Error(`Server error: ${res.status}`);
        return res.json();
    }

    // Temporarily disables a button and shows a spinner while a request is in flight.
    function setLoading(btn, loading) {
        if (loading) {
            btn.dataset.originalHtml = btn.innerHTML;
            btn.innerHTML = Icons.get('spinner') + ' Working...';
            btn.disabled = true;
        } else {
            btn.innerHTML = btn.dataset.originalHtml ?? btn.innerHTML;
            btn.disabled = false;
        }
    }

    // Shows a brief toast notification at the bottom of the screen.
    function toast(message, type = 'success') {
        const existing = document.getElementById('eb-toast');
        if (existing) existing.remove();

        const el = document.createElement('div');
        el.id = 'eb-toast';
        el.textContent = message;
        el.style.cssText = `
            position: fixed; bottom: 28px; left: 50%; transform: translateX(-50%);
            background: ${type === 'error' ? '#ef4444' : '#111827'};
            color: #fff; padding: 10px 20px; border-radius: 8px;
            font-size: 0.88rem; font-weight: 500; z-index: 9999;
            box-shadow: 0 4px 12px rgba(0,0,0,0.2);
            animation: fadeInUp 0.2s ease;
        `;
        document.body.appendChild(el);
        setTimeout(() => el.remove(), 3000);
    }

    // Shows a custom confirm dialog. Returns a Promise<boolean>.
    // Replaces the ugly browser confirm() popup.
    function confirm(message, confirmLabel = 'Confirm', danger = false) {
        return new Promise(resolve => {
            const overlay = document.createElement('div');
            overlay.style.cssText = `
                position:fixed; inset:0; background:rgba(0,0,0,0.4);
                display:flex; align-items:center; justify-content:center; z-index:10000;
            `;

            overlay.innerHTML = `
                <div style="background:#fff; border-radius:12px; padding:28px; max-width:380px;
                            width:90%; box-shadow:0 20px 40px rgba(0,0,0,0.15);">
                    <p style="font-size:0.95rem; color:#111827; margin:0 0 20px; line-height:1.5;">
                        ${message}
                    </p>
                    <div style="display:flex; gap:10px; justify-content:flex-end;">
                        <button id="eb-cancel" style="padding:8px 18px; border:1px solid #e5e7eb;
                            border-radius:7px; background:#fff; cursor:pointer; font-size:0.88rem;">
                            Cancel
                        </button>
                        <button id="eb-confirm" style="padding:8px 18px; border:none; border-radius:7px;
                            background:${danger ? '#ef4444' : '#111827'}; color:#fff;
                            cursor:pointer; font-size:0.88rem; font-weight:500;">
                            ${confirmLabel}
                        </button>
                    </div>
                </div>
            `;

            document.body.appendChild(overlay);

            overlay.querySelector('#eb-cancel').onclick = () => { overlay.remove(); resolve(false); };
            overlay.querySelector('#eb-confirm').onclick = () => { overlay.remove(); resolve(true); };
            overlay.onclick = (e) => { if (e.target === overlay) { overlay.remove(); resolve(false); } };
        });
    }

    // ─── Actions ────────────────────────────────────────────────────────────

    // Accept an application — updates the badge and hides the action buttons
    async function acceptApplication(btn) {
        const card = btn.closest('.application-card');
        const applicationId = btn.dataset.applicationId;
        const postId = btn.dataset.postId;

        setLoading(btn, true);
        try {
            const res = await post('/ActivityPost/AcceptApplication', { applicationId, postId });
            if (res.success) {
                // Swap badge
                card.querySelector('.badge').className = 'badge badge-accepted';
                card.querySelector('.badge').textContent = 'Accepted';
                // Remove action buttons
                card.querySelector('.application-actions')?.remove();
                // Update member count in the header
                updateMemberCount(res.currentMembers, res.maxMembers);
                toast('Application accepted');
            }
        } catch {
            toast('Something went wrong', 'error');
            setLoading(btn, false);
        }
    }

    // Reject an application — updates the badge and hides the action buttons
    async function rejectApplication(btn) {
        const card = btn.closest('.application-card');
        const applicationId = btn.dataset.applicationId;
        const postId = btn.dataset.postId;

        setLoading(btn, true);
        try {
            const res = await post('/ActivityPost/RejectApplication', { applicationId, postId });
            if (res.success) {
                card.querySelector('.badge').className = 'badge badge-rejected';
                card.querySelector('.badge').textContent = 'Rejected';
                card.querySelector('.application-actions')?.remove();
                toast('Application rejected');
            }
        } catch {
            toast('Something went wrong', 'error');
            setLoading(btn, false);
        }
    }

    // Close a post — soft-deletes it and updates the UI
    async function closePost(btn) {
        const postId = btn.dataset.postId;
        const ok = await confirm('Close this post? It will be hidden from the board and moved to the archive.', 'Close Post', true);
        if (!ok) return;
        
        const res = await post('/ActivityPost/ClosePost', { id: postId });

        setLoading(btn, true);
        try {
            
            if (res.success) {
                // Swap status badge to Closed
                const statusBadge = document.querySelector('.badge-open');
                if (statusBadge) {
                    statusBadge.className = 'badge badge-closed';
                    statusBadge.textContent = 'Closed';
                }
                // Hide the close button
                btn.closest('form')?.remove() ?? btn.remove();
                // Hide accept/reject buttons on all pending applications
                document.querySelectorAll('.application-actions').forEach(el => el.remove());
                toast('Post closed and archived');
            }
        } catch {
            toast('Something went wrong', 'error');
            setLoading(btn, false);
        }
    }

    // Restore a post from the archive page
    async function restorePost(btn) {
        const postId = btn.dataset.postId;
        const ok = await confirm('Restore this post? It will become visible on the board again.', 'Restore');
        if (!ok) return;

        setLoading(btn, true);
        try {
            const res = await post('/ActivityPost/RestorePost', { id: postId });
            if (res.success) {
                // Fade out and remove the card from the archive list
                const card = btn.closest('.application-card');
                card.style.transition = 'opacity 0.3s, transform 0.3s';
                card.style.opacity = '0';
                card.style.transform = 'translateX(20px)';
                setTimeout(() => card.remove(), 300);
                toast('Post restored to board');
            }
        } catch {
            toast('Something went wrong', 'error');
            setLoading(btn, false);
        }
    }

    // Hard delete a post from the archive page — permanent
    async function hardDeletePost(btn) {
        const postId = btn.dataset.postId;
        const title = btn.dataset.postTitle;
        const ok = await confirm(`Permanently delete "<strong>${title}</strong>"?<br>This cannot be undone.`, 'Delete Forever', true);
        if (!ok) return;

        setLoading(btn, true);
        try {
            const res = await post('/ActivityPost/HardDeletePost', { id: postId });
            if (res.success) {
                const card = btn.closest('.application-card');
                card.style.transition = 'opacity 0.3s, transform 0.3s';
                card.style.opacity = '0';
                card.style.transform = 'translateX(20px)';
                setTimeout(() => card.remove(), 300);
                toast('Post permanently deleted');
            }
        } catch {
            toast('Something went wrong', 'error');
            setLoading(btn, false);
        }
    }

    // ─── Member count helper ─────────────────────────────────────────────────

    function updateMemberCount(current, max) {
        const el = document.getElementById('member-count');
        if (!el) return;
        const spotsLeft = max - current;
        el.innerHTML = `${current} / ${max} joined <span class="meta-spots">(${spotsLeft} spot${spotsLeft !== 1 ? 's' : ''} left)</span>`;
    }

    // ─── Init ────────────────────────────────────────────────────────────────

    // Wire up all button clicks using event delegation on the document.
    // This means buttons added dynamically later still work.
    function init() {
        Icons.init();

        document.addEventListener('click', e => {
            const btn = e.target.closest('[data-action]');
            if (!btn) return;

            e.preventDefault();

            switch (btn.dataset.action) {
                case 'accept':       acceptApplication(btn); break;
                case 'reject':       rejectApplication(btn); break;
                case 'close-post':   closePost(btn);         break;
                case 'restore':      restorePost(btn);       break;
                case 'hard-delete':  hardDeletePost(btn);    break;
            }
        });
    }

    return { init };
})();

// CSS for the spinner and toast animations (injected once)
const style = document.createElement('style');
style.textContent = `
    @keyframes spin { to { transform: rotate(360deg); } }
    @keyframes fadeInUp {
        from { opacity: 0; transform: translateX(-50%) translateY(8px); }
        to   { opacity: 1; transform: translateX(-50%) translateY(0); }
    }
    .badge-closed { background:#f3f4f6; color:#6b7280; border:1px solid #e5e7eb; }
`;
document.head.appendChild(style);

document.addEventListener('DOMContentLoaded', () => EventBoard.init());

// ─── Review Modal (added for review feature) ────────────────────────────────

const ReviewModal = (() => {
    let selectedStars = 0;
    let currentPostId = null;
    let currentRevieweeId = null;
    let currentRevieweeName = null;

    const starLabels = ['', 'Poor', 'Fair', 'Good', 'Very Good', 'Excellent'];

    function open(btn) {
        currentPostId = btn.dataset.postId;
        currentRevieweeId = btn.dataset.revieweeId ?? '';
        currentRevieweeName = btn.dataset.revieweeName ?? '';
        selectedStars = 0;

        document.getElementById('modal-reviewee-name').textContent = currentRevieweeName;
        document.getElementById('review-comment').value = '';
        document.getElementById('star-label').textContent = 'Select a rating';

        // Reset stars
        document.querySelectorAll('.star-pick').forEach(s => s.classList.remove('active'));

        document.getElementById('review-modal').style.display = 'flex';
    }

    function close() {
        document.getElementById('review-modal').style.display = 'none';
        selectedStars = 0;
    }

    async function submit() {
        if (selectedStars === 0) {
            // Shake the stars to hint user to pick one
            const picker = document.getElementById('star-picker');
            picker.style.animation = 'none';
            picker.offsetHeight; // reflow
            picker.style.animation = 'shake 0.3s ease';
            return;
        }

        const comment = document.getElementById('review-comment').value.trim();
        const btn = document.querySelector('[data-action="submit-review"]');

        if (!currentRevieweeId) {
            EventBoard.toast('Cannot submit review: participant has no linked account', 'error');
            close();
            return;
        }

        try {
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value ?? '';
            const body = new URLSearchParams({
                postId: currentPostId,
                revieweeId: currentRevieweeId,
                revieweeName: currentRevieweeName,
                rating: selectedStars,
                comment: comment,
                isAnonymous: 'false',
                __RequestVerificationToken: token
            });

            const res = await fetch('/Review/Submit', {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body
            });

            const data = await res.json();

            close();

            if (data.success) {
                // Find the Rate button for this person and replace with a "done" state
                const rateBtn = document.querySelector(
                    `[data-action="open-review"][data-reviewee-name="${currentRevieweeName}"]`
                );
                if (rateBtn) {
                    rateBtn.outerHTML = `<span class="btn-rate-done">
                        ${'★'.repeat(selectedStars)}${'☆'.repeat(5 - selectedStars)}
                    </span>`;
                }
                EventBoard.toast(`Review submitted for ${currentRevieweeName}!`);
            } else {
                EventBoard.toast(data.error ?? 'Could not submit review', 'error');
            }
        } catch {
            close();
            EventBoard.toast('Something went wrong submitting the review', 'error');
        }
    }

    function handleStarClick(star) {
        selectedStars = parseInt(star.dataset.value);
        document.querySelectorAll('.star-pick').forEach(s => {
            s.classList.toggle('active', parseInt(s.dataset.value) <= selectedStars);
        });
        document.getElementById('star-label').textContent = starLabels[selectedStars];
    }

    return { open, close, submit, handleStarClick };
})();

// Add shake animation for the star picker
const reviewStyle = document.createElement('style');
reviewStyle.textContent = `
    @keyframes shake {
        0%, 100% { transform: translateX(0); }
        25%       { transform: translateX(-6px); }
        75%       { transform: translateX(6px); }
    }
`;
document.head.appendChild(reviewStyle);

// Hook into the existing EventBoard click delegation
// We extend the switch statement by patching the init
const _originalInit = EventBoard.init;
document.addEventListener('click', e => {
    const btn = e.target.closest('[data-action]');
    if (!btn) return;
    switch (btn.dataset.action) {
        case 'open-review':         ReviewModal.open(btn);         break;
        case 'close-review-modal':  ReviewModal.close();           break;
        case 'submit-review':       ReviewModal.submit();          break;
    }
    // Star picker clicks
    if (btn.classList.contains('star-pick')) {
        ReviewModal.handleStarClick(btn);
    }
});

// Star hover effect
document.addEventListener('mouseover', e => {
    const star = e.target.closest('.star-pick');
    if (!star) return;
    const val = parseInt(star.dataset.value);
    document.querySelectorAll('.star-pick').forEach(s => {
        s.style.color = parseInt(s.dataset.value) <= val ? '#f59e0b' : '';
    });
});

document.addEventListener('mouseout', e => {
    if (!e.target.closest('.star-picker')) return;
    document.querySelectorAll('.star-pick').forEach(s => {
        s.style.color = '';
    });
});
