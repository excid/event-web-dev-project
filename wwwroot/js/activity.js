(function () {
    'use strict';
    if (!window.eventData) {
        console.error("eventData not found");
        return;
    }
    let ACTIVITY = mapActivity(window.eventData);
    let APPLICATIONS = window.eventData.applications || [];

    


    function mapActivity(data) {
        if (!data) return null;

        return {
            id: data.id,
            category: (data.category || '').toLowerCase(),
            categoryLabel: data.category,
            status: (data.status || '').toLowerCase(),
            title: data.title,
            poster: {
                name: data.postedBy,
                href: data.ownerId ? '/Profile/Index?userId=' + data.ownerId : '#'
            },
            postedAt: data.postedAt,
            description: data.description,
            location: data.location,
            expires: data.expiresAt,
            activityDate: data.activityDate,
            membersJoined: data.currentMembers,
            membersTotal: data.maxMembers,
            applicationMode: data.applicationMode,
            applications: data.applications || []
        };
    }

    /* ---- State ---- */
    let currentState = window.userState;
    let openChatId = null; // which application's chat is expanded
    let chatOpen = false; // whether the "Chat with Organizer" panel is open (pending/accepted view)

    /* ---- Init ---- */
    document.addEventListener('DOMContentLoaded', function () {
        render();
        buildDevSwitcher();
    });

    /* ---- Helpers ---- */

    function setState(s) {
        currentState = s;
        openChatId = null;
        chatOpen = false;
        render();
        // update url without reload
        const url = new URL(window.location);
        url.searchParams.set('state', s);
        history.replaceState({}, '', url);
    }

    /* ---- Render ---- */
    function render() {
        const root = document.getElementById('activity-root');
        if (!root) return;
        root.innerHTML = buildPage();
        attachHandlers();
    }

    function buildPage() {
        return `
        <div class="page-wrapper" style="padding-top: 80px;">
            <a href="#" class="back-link">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-linecap="round" stroke-linejoin="round">
                    <polyline points="15 18 9 12 15 6"></polyline>
                </svg>
                Back
            </a>

            <div class="activity-layout">
                ${buildMainCard()}
                ${buildSidebar()}
            </div>
        </div>`;
    }

    /* ---- Main Card ---- */
    function buildMainCard() {
        const spotsLeft = ACTIVITY.membersTotal - ACTIVITY.membersJoined;
        const catClass = 'badge-' + ACTIVITY.category;

        return `
        <article class="activity-card">
            <header class="card-header">
                <div class="card-top-row">
                    <span class="badge ${catClass}">${ACTIVITY.categoryLabel}</span>
                    <span class="status-badge status-${ACTIVITY.status}">${cap(ACTIVITY.status)}</span>
                </div>
                <h1 class="activity-title">${ACTIVITY.title}</h1>
                <p class="poster-line">
                    Posted by <a href="${ACTIVITY.poster.href}">${ACTIVITY.poster.name}</a>
                    &bull; ${ACTIVITY.postedAt}
                </p>
            </header>

            <section aria-label="Description">
                <h2 class="section-title">Description</h2>
                <p class="description-text">${ACTIVITY.description}</p>
            </section>

            <div class="meta-grid">
                <div class="meta-item">
                    <span class="meta-item-row">
                        ${iconPin()} Location
                    </span>
                    <span class="meta-value">${ACTIVITY.location}</span>
                </div>
                <div class="meta-item">
                    <span class="meta-item-row">
                        ${iconUsers()} Members
                    </span>
                    <span class="meta-value">
                        ${ACTIVITY.membersJoined} / ${ACTIVITY.membersTotal} joined
                        <a href="#" class="spots-left">(${spotsLeft} spots left)</a>
                    </span>
                </div>
                <div class="meta-item">
                    <span class="meta-item-row">
                        ${iconCalendar()} Expires
                    </span>
                    <span class="meta-value">${ACTIVITY.expires}</span>
                </div>
                <div class="meta-item">
                    <span class="meta-item-row">
                        ${iconCalendar()} Activity Date
                    </span>
                    <span class="meta-value">${ACTIVITY.activityDate}</span>
                </div>
                <div class="meta-item">
                    <span class="meta-item-row">
                        ${iconClock()} Application Mode
                    </span>
                    <span class="meta-value">${ACTIVITY.applicationMode}</span>
                </div>
            </div>

            <hr class="card-divider">

            ${buildCardFooter()}
        </article>`;
    }

    function buildCardFooter() {
        switch (currentState) {
            case 'apply': return buildApplyForm();
            case 'pending': return buildBanner('pending');
            case 'accepted': return buildBanner('accepted');
            case 'rejected': return buildBanner('rejected');
            case 'owner': return ''; // owner sees no footer
            case 'Login': return '<p class="login-prompt">Please <a href="/">log in</a> to apply for this activity.</p>';
            default: return currentState;
        }
    }

    function buildApplyForm() {
        return `
        <section class="apply-section" aria-label="Apply to join">
            <h2 class="section-title">Apply to Join</h2>
            <textarea
                id="apply-message"
                placeholder="Tell the organizer why you'd like to join..."
                maxlength="500"
            ></textarea>
            <div class="apply-actions">
                <button id="btn-submit-apply" class="btn-submit" disabled>Submit Application</button>
                <button id="btn-cancel-apply" class="btn-cancel">Cancel</button>
            </div>
        </section>`;
    }

    function buildBanner(type) {
        const map = {
            pending: {
                cls: 'banner-pending',
                icon: iconClock(),
                title: '⏳ Application Pending',
                body: 'Your application is currently under review. The organizer will notify you once they make a decision. Please be patient!',
            },
            accepted: {
                cls: 'banner-accepted',
                icon: iconCheck(),
                title: '✓ Application Accepted',
                body: 'Congratulations! Your application has been accepted. The organizer will contact you with more details about the activity.',
            },
            rejected: {
                cls: 'banner-rejected',
                icon: iconX(),
                title: '✗ Application Rejected',
                body: "Unfortunately, your application was not accepted this time. Don't be discouraged – there are many other activities to join!",
            },
        };
        const d = map[type];
        return `
        <aside class="status-banner ${d.cls}" role="status" aria-live="polite">
            ${d.icon}
            <div>
                <p class="banner-title">${d.title}</p>
                <p class="banner-body">${d.body}</p>
            </div>
        </aside>`;
    }

    /* ---- Sidebar ---- */
    function buildSidebar() {
        const apps = buildApplicationsList();
        const chat = buildChatPanel();

        return `
        <aside class="sidebar" aria-label="Applications and chat">
            <section class="sidebar-card" aria-label="Applications">
                <h2 class="sidebar-title">Applications (${APPLICATIONS.length})</h2>
                <div class="application-list">
                    ${apps}
                </div>
            </section>
            ${chat ? `<section class="sidebar-card chat-card" aria-label="Chat">${chat}</section>` : ''}
        </aside>`;
    }

    function buildApplicationsList() {
        let visible = APPLICATIONS;

        return visible.map(function (app) {
            const statusClass = {
                accepted: 'app-status-accepted',
                pending: 'app-status-pending',
                rejected: 'app-status-rejected',
            }[app.status] || '';

            const showChatToggle = (currentState === 'owner') ||
                (currentState === 'pending' && app.status === 'pending');

            const isOpen = openChatId === app.id;

            return `
            <article class="application-item" data-appid="${app.Id}">
                <div class="application-item-top">
                    <div>
                        <span class="applicant-name">${app.applicantName}</span>
                        <div class="applicant-date">${app.appliedAt}</div>
                    </div>
                    <span class="app-status ${statusClass}">${cap(app.status)}</span>
                </div>
                <p class="applicant-message">${app.message}</p>
                ${showChatToggle ? `
                <button class="btn-open-chat" data-chatid="${app.Id}">
                    ${iconChat()} ${isOpen ? 'Hide Chat' : 'Open Chat'}
                    ${isOpen ? '' : ''}
                </button>` : ''}
            </article>`;
        }).join('');
    }

    function buildChatPanel() {
        // "Chat with Organizer" panel shown when pending state and chat is requested
        if (currentState === 'pending' && chatOpen) {
            const app = APPLICATIONS.find(function (a) { return a.id === openChatId; });
            const messages = app ? app.chatMessages : [];
            return buildChatPanelContent('Chat with Organizer', 'Ask questions about the activity.', messages, 'organizer');
        }

        // Owner opening a specific applicant chat
        if (currentState === 'owner' && openChatId) {
            const app = APPLICATIONS.find(function (a) { return a.id === openChatId; });
            const messages = app ? app.chatMessages : [];
            return buildChatPanelContent('Chat with ' + (app ? app.name : ''), '', messages, openChatId);
        }

        return '';
    }

    function buildChatPanelContent(title, intro, messages, panelId) {
        const msgHtml = messages.map(function (m) {
            return `
            <div class="msg-bubble ${m.side === 'right' ? 'right' : 'left'}">
                <span class="bubble-text">${m.text}</span>
                <span class="bubble-time">${m.time}</span>
            </div>`;
        }).join('');

        return `
        <h2 class="sidebar-title">${title}</h2>
        ${intro ? `<p class="chat-intro">${intro}</p>` : ''}
        <div class="chat-messages" id="chat-messages-${panelId}">
            ${msgHtml}
        </div>
        <div class="chat-input-row">
            <input
                type="text"
                id="chat-input"
                class="chat-input"
                placeholder="Type your message..."
                aria-label="Chat message"
            >
            <button id="btn-send-chat" class="btn-send" data-panel="${panelId}">
                ${iconSend()} Send
            </button>
        </div>`;
    }

    /* ---- Event Handlers ---- */
    function attachHandlers() {
        /* Apply form: enable submit when textarea has content */
        const textarea = document.getElementById('apply-message');
        if (textarea) {
            textarea.addEventListener('input', function () {
                const btn = document.getElementById('btn-submit-apply');
                if (btn) {
                    btn.disabled = textarea.value.trim().length === 0;
                    btn.classList.toggle('active', !btn.disabled);
                }
            });
        }

        /* Submit application */
        const submitBtn = document.getElementById('btn-submit-apply');
        if (submitBtn) {
            submitBtn.addEventListener('click', async function () {
                const message = document.getElementById('apply-message').value.trim();
                submitBtn.disabled = true;
                submitBtn.textContent = 'Submitting...';

                try {
                    const response = await fetch('/Event/SubmitApplication', {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json',
                            'RequestVerificationToken': window.antiForgeryToken
                        },
                        body: JSON.stringify({
                            postId:        window.eventData.id,
                            applicantName: 'Me',
                            message:       message
                        })
                    });

                    const result = await response.json();

                    if (result.success) {
                        if (result.accepted) {
                            showToast('SUCCESS', 'You have been instantly accepted! Welcome to the activity.');
                            setState('accepted');
                        } else {
                            showToast('SUCCESS', 'Application submitted! The organizer will review it.');
                            setState('pending');
                        }
                    } else {
                        showToast('ERROR', result.error ?? 'Something went wrong');
                        submitBtn.disabled = false;
                        submitBtn.textContent = 'Submit Application';
                    }
                } catch (err) {
                    showToast('ERROR', 'Network error, please try again');
                    submitBtn.disabled = false;
                    submitBtn.textContent = 'Submit Application';
                }
            });
        }

        /* Cancel apply */
        const cancelBtn = document.getElementById('btn-cancel-apply');
        if (cancelBtn) {
            cancelBtn.addEventListener('click', function () {
                const textarea = document.getElementById('apply-message');
                if (textarea) textarea.value = '';
                const sub = document.getElementById('btn-submit-apply');
                if (sub) { sub.disabled = true; sub.classList.remove('active'); }
            });
        }

        /* Open / Close chat toggles */
        document.querySelectorAll('.btn-open-chat').forEach(function (btn) {
            btn.addEventListener('click', function () {
                const id = btn.dataset.chatid;
                if (openChatId === id && chatOpen) {
                    openChatId = null;
                    chatOpen = false;
                } else {
                    openChatId = id;
                    chatOpen = true;
                }
                render();
            });
        });

        /* Send chat message */
        const sendBtn = document.getElementById('btn-send-chat');
        if (sendBtn) {
            sendBtn.addEventListener('click', sendChatMessage);
        }
        const chatInput = document.getElementById('chat-input');
        if (chatInput) {
            chatInput.addEventListener('keydown', function (e) {
                if (e.key === 'Enter') sendChatMessage();
            });
        }

        /* Back link: no real nav, just prevent default */
        document.querySelector('.back-link').addEventListener('click', function (e) {
            e.preventDefault();
            history.back();
        });
    }

    function sendChatMessage() {
        const input = document.getElementById('chat-input');
        if (!input || input.value.trim() === '') return;
        const text = input.value.trim();
        const now = new Date().toLocaleString('en-US', { month: 'short', day: 'numeric', hour: 'numeric', minute: '2-digit', hour12: true });

        // Add to mock data
        const app = APPLICATIONS.find(function (a) { return a.id === openChatId; });
        if (app) {
            app.chatMessages.push({ side: 'right', text: text, time: now });
        }
        render();

        // scroll to bottom
        setTimeout(function () {
            const msgs = document.getElementById('chat-messages-' + openChatId) ||
                document.getElementById('chat-messages-organizer');
            if (msgs) msgs.scrollTop = msgs.scrollHeight;
        }, 50);
    }

    /* ---- Toast ---- */
    function showToast(title, message) {
        let container = document.getElementById('toast-container');
        if (!container) {
            container = document.createElement('div');
            container.id = 'toast-container';
            container.className = 'toast-container';
            document.body.appendChild(container);
        }

        const toast = document.createElement('div');
        toast.className = 'toast toast-success';
        toast.innerHTML = `
            <span class="toast-icon">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-linecap="round" stroke-linejoin="round">
                    <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"></path>
                    <polyline points="22 4 12 14.01 9 11.01"></polyline>
                </svg>
            </span>
            <div class="toast-content">
                <p class="toast-title">${title}</p>
                <p class="toast-msg">${message}</p>
            </div>
            <button class="toast-close" aria-label="Close">&times;</button>`;

        toast.querySelector('.toast-close').addEventListener('click', function () {
            container.removeChild(toast);
        });

        container.appendChild(toast);
        setTimeout(function () {
            if (container.contains(toast)) container.removeChild(toast);
        }, 5000);
    }

    function iconPin() { return '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-linecap="round" stroke-linejoin="round"><path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"></path><circle cx="12" cy="10" r="3"></circle></svg>'; }
    function iconUsers() { return '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-linecap="round" stroke-linejoin="round"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path><circle cx="9" cy="7" r="4"></circle><path d="M23 21v-2a4 4 0 0 0-3-3.87"></path><path d="M16 3.13a4 4 0 0 1 0 7.75"></path></svg>'; }
    function iconCalendar() { return '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="4" width="18" height="18" rx="2" ry="2"></rect><line x1="16" y1="2" x2="16" y2="6"></line><line x1="8" y1="2" x2="8" y2="6"></line><line x1="3" y1="10" x2="21" y2="10"></line></svg>'; }
    function iconClock() { return '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"></circle><polyline points="12 6 12 12 16 14"></polyline></svg>'; }
    function iconCheck() { return '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"></polyline></svg>'; }
    function iconX() { return '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"></line><line x1="6" y1="6" x2="18" y2="18"></line></svg>'; }
    function iconChat() { return '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-linecap="round" stroke-linejoin="round"><path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"></path></svg>'; }
    function iconSend() { return '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-linecap="round" stroke-linejoin="round"><line x1="22" y1="2" x2="11" y2="13"></line><polygon points="22 2 15 22 11 13 2 9 22 2"></polygon></svg>'; }

    /* ---- Utility ---- */
    function cap(str) { return str.charAt(0).toUpperCase() + str.slice(1); }

    /* ---- SVG Icons ---- */
    

})();
