// 1. Selection: ดึง Elements
const postForm       = document.querySelector('form');
const titleInput     = document.getElementById('act_name');
const categorySelect = document.getElementById('category');
const descriptionInput = document.getElementById('description');
const locationInput  = document.getElementById('location');
const participantInput = document.getElementById('participant');
const dateInput      = document.getElementById('end-date');
const timeInput      = document.getElementById('end-time');
const activityDateInput = document.getElementById('activity-date');
const activityTimeInput = document.getElementById('activity-time');
const createBtn      = document.querySelector('.createpost-btn');
const cancelBtn      = document.querySelector('.canclepost-btn');

const requiredFields = [
    titleInput, categorySelect, descriptionInput,
    locationInput, participantInput, dateInput, timeInput,
    activityDateInput, activityTimeInput
];

// Set minimum date to today
const setMinDate = () => {
    const now = new Date();
    const todayStr = now.toISOString().split('T')[0];
    dateInput.setAttribute('min', todayStr);
    activityDateInput.setAttribute('min', todayStr);
};
setMinDate();

// When expiration date changes, update activity date minimum
dateInput.addEventListener('change', function () {
    if (dateInput.value) {
        activityDateInput.setAttribute('min', dateInput.value);
        // If activity date is now before expiration date, clear it
        if (activityDateInput.value && activityDateInput.value < dateInput.value) {
            activityDateInput.value = '';
        }
    }
});

dateInput.addEventListener('input', function () {
    if (!dateInput.value) return;
    const parts = dateInput.value.split('-'); // "yyyy-MM-dd"
    if (parts[0] && parts[0].length > 4) {
        parts[0] = parts[0].slice(0, 4);
        dateInput.value = parts.join('-');
    }
});

activityDateInput.addEventListener('input', function () {
    if (!activityDateInput.value) return;
    const parts = activityDateInput.value.split('-'); // "yyyy-MM-dd"
    if (parts[0] && parts[0].length > 4) {
        parts[0] = parts[0].slice(0, 4);
        activityDateInput.value = parts.join('-');
    }
});

function triggerError(inputElement) {
    inputElement.classList.add('input-error');
    setTimeout(() => inputElement.classList.remove('input-error'), 3000);
}

function isPastDateTime(dateStr, timeStr) {
    if (!dateStr || !timeStr) return false;
    return new Date(`${dateStr}T${timeStr}`) < new Date();
}

// Reads the anti-forgery token — same pattern as events.js
function getAntiForgeryToken() {
    return document.querySelector('input[name="__RequestVerificationToken"]')?.value ?? '';
}

createBtn.addEventListener('click', async function(e) {
    e.preventDefault();

    // Validate all fields
    let isFormValid = true;
    requiredFields.forEach(input => {
        if (!input.value || input.value.trim() === '') {
            triggerError(input);
            isFormValid = false;
        }
    });

    const memberCount = parseInt(participantInput.value);
    if (isNaN(memberCount) || memberCount < 1 || memberCount > 100) {
        alert('จำนวนสมาชิกต้องอยู่ระหว่าง 1 ถึง 100 คน');
        triggerError(participantInput);
        isFormValid = false;
    }

    if (!isFormValid) return;

    if (isPastDateTime(dateInput.value, timeInput.value)) {
        alert('ไม่สามารถเลือกเวลาในอดีตได้!');
        triggerError(dateInput);
        triggerError(timeInput);
        return;
    }

    const expirationDateTime = new Date(`${dateInput.value}T${timeInput.value}`);
    const activityDateTime   = new Date(`${activityDateInput.value}T${activityTimeInput.value}`);
    if (activityDateTime < expirationDateTime) {
        alert('Activity date must be on or after the expiration date.');
        triggerError(activityDateInput);
        triggerError(activityTimeInput);
        return;
    }

    const selectedMode = document.querySelector('input[name="join_mode"]:checked').value;

    const eventData = {
        title:        titleInput.value.trim(),
        category:     categorySelect.value,
        description:  descriptionInput.value.trim(),
        location:     locationInput.value.trim(),
        maxMembers:   memberCount,
        deadline:     `${dateInput.value}T${timeInput.value}`,
        activityDate: `${activityDateInput.value}T${activityTimeInput.value}`,
        mode:         selectedMode
    };

    // Disable button while submitting
    createBtn.disabled = true;
    createBtn.textContent = 'Creating...';

    try {
        const response = await fetch('/ActivityPost/Create', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getAntiForgeryToken()
            },
            body: JSON.stringify(eventData)
        });

        const result = await response.json();

        if (result.success) {
            // Redirect to the newly created post
            window.location.href = `/ActivityPost/Index?id=${result.postId}`;
        } else {
            alert('เกิดข้อผิดพลาด: ' + (result.error ?? 'Unknown error'));
            createBtn.disabled = false;
            createBtn.textContent = 'Create Post';
        }
    } catch (err) {
        alert('เกิดข้อผิดพลาด กรุณาลองใหม่อีกครั้ง');
        createBtn.disabled = false;
        createBtn.textContent = 'Create Post';
    }
});

// Cancel button — go back
cancelBtn.addEventListener('click', () => {
    window.location.href = '/Home/Index';
});
