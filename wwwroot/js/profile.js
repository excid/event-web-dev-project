(function () {
    'use strict';

    // --- State Management ---
    let tempTags = [];
    let tempInterests = [];

    // --- DOM Elements ---
    const profileCard = document.getElementById('profile-card');
    const viewContainer = document.querySelector('.view-mode-container');
    const editContainer = document.querySelector('.edit-mode-container');
    const editForm = document.getElementById('form-edit-profile');

    // Hidden Fields (for syncing state back to the server)
    const hiddenTags = document.getElementById('hidden-tags');
    const hiddenInterests = document.getElementById('hidden-interests');
    const hiddenAvatarUrl = document.getElementById('hidden-avatar-url');

    // View Elements (these are now assumed to be rendered by the server based on current profile data)
    // No longer directly manipulated by JS for rendering view mode.

    // Edit Elements
    const editTagList = document.getElementById('edit-tag-list');
    const inputNewTag = document.getElementById('input-new-tag');
    const avatarInput = document.getElementById('avatar-input');
    const avatarPreview = document.getElementById('edit-avatar-preview');
    const editInterestList = document.getElementById('edit-interest-list');

    // --- Initialization ---
    document.addEventListener('DOMContentLoaded', () => {
        init();
        attachEventListeners();
    });

    function init() {
        // Initialize state from hidden fields populated by Razor
        if (hiddenTags && hiddenTags.value) {
            tempTags = hiddenTags.value.split(',').filter(t => t.trim() !== "");
        } else {
            tempTags = [];
        }
        if (hiddenInterests && hiddenInterests.value) {
            tempInterests = hiddenInterests.value.split(',').filter(t => t.trim() !== "");
        } else {
            tempInterests = [];
        }

        // Initialize avatar preview if a URL is present in the hidden field
        if (hiddenAvatarUrl && hiddenAvatarUrl.value) {
            avatarPreview.src = hiddenAvatarUrl.value;
            avatarPreview.style.display = 'block';
        } else {
            avatarPreview.style.display = 'none';
        }
    }

    // --- Mode Toggling ---
    function toggleEditMode(isEdit) {
        if (isEdit) {
            // Prepare edit form
            renderEditTags();
            updateInterestUI();

            viewContainer.style.display = 'none';
            editContainer.style.display = 'block';
            profileCard.classList.add('is-editing');
        } else {
            viewContainer.style.display = 'block';
            editContainer.style.display = 'none';
            profileCard.classList.remove('is-editing');
        }
    }

    // --- Rendering ---
    // renderView() is removed as view mode is assumed to be server-rendered.

    function renderEditTags() {
        editTagList.innerHTML = tempTags.map((tag, index) => `
            <span class="tag-chip-edit">
                ${tag}
                <button type="button" class="btn-remove-tag" data-index="${index}" title="Remove tag">
                    <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="3">
                        <line x1="18" y1="6" x2="6" y2="18"></line>
                        <line x1="6" y1="6" x2="18" y2="18"></line>
                    </svg>
                </button>
            </span>
        `).join('');

        // Re-attach remove listeners
        editTagList.querySelectorAll('.btn-remove-tag').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const index = parseInt(btn.dataset.index);
                tempTags.splice(index, 1);
                renderEditTags();
                syncState();
            });
        });
    }

    function updateInterestUI() {
        if (!editInterestList) return;
        editInterestList.querySelectorAll('.interest-chip').forEach(chip => {
            const val = chip.dataset.value;
            if (tempInterests.includes(val)) {
                chip.classList.add('active');
            } else {
                chip.classList.remove('active');
            }
        });
    }

    // --- Actions ---
    function syncState() {
        if (hiddenTags) hiddenTags.value = tempTags.join(',');
        if (hiddenInterests) hiddenInterests.value = tempInterests.join(',');
    }

    // handleSave() is removed as form submission handles saving.

    function addTag() {
        const tag = inputNewTag.value.trim();
        if (tag && !tempTags.includes(tag)) {
            tempTags.push(tag);
            renderEditTags();
            inputNewTag.value = '';
            syncState();
        }
    }

    function toggleInterest(chip) {
        const val = chip.dataset.value;
        const idx = tempInterests.indexOf(val);
        if (idx > -1) {
            tempInterests.splice(idx, 1);
            chip.classList.remove('active');
        } else {
            tempInterests.push(val);
            chip.classList.add('active');
        }
        syncState();
    }

    // --- Event Listeners ---
    function attachEventListeners() {
        // Mode switchers
        document.getElementById('btn-edit-start').addEventListener('click', () => {
            // Re-initialize state from hidden fields when entering edit mode
            // to ensure we're working with the current server-side data.
            init();
            toggleEditMode(true);
        });
        document.getElementById('btn-edit-cancel').addEventListener('click', () => {
            // Re-fetch original state from hidden fields if cancelled
            init();
            toggleEditMode(false);
        });
        // The save button is now assumed to be a submit button for the form.
        // document.getElementById('btn-edit-save').addEventListener('click', handleSave);

        // Tag management
        document.getElementById('btn-add-tag').addEventListener('click', addTag);
        inputNewTag.addEventListener('keydown', (e) => {
            if (e.key === 'Enter') {
                e.preventDefault();
                addTag();
            }
        });

        // Interest selection
        if (editInterestList) {
            editInterestList.addEventListener('click', (e) => {
                const chip = e.target.closest('.interest-chip');
                if (chip) toggleInterest(chip);
            });
        }

        // Avatar upload trigger
        const trigger = document.getElementById('avatar-upload-trigger');
        if (trigger) {
            trigger.addEventListener('click', () => avatarInput.click());
        }

        if (avatarInput) {
            avatarInput.addEventListener('change', function () {
                if (this.files && this.files[0]) {
                    const reader = new FileReader();
                    reader.onload = function (e) {
                        avatarPreview.src = e.target.result;
                        avatarPreview.style.display = 'block';
                        // In this mock-to-real hybrid, we store the base64 in the hidden field
                        // In a production app, you'd upload the file and store the URL.
                        if (hiddenAvatarUrl) hiddenAvatarUrl.value = e.target.result;
                    }
                    reader.readAsDataURL(this.files[0]);
                }
            });
        }
    }

    // showToast() is removed as it's no longer part of this module's responsibility.

})();
