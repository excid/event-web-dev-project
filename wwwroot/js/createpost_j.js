// 1. Selection: ดึง Elements
const postForm = document.querySelector('form');
const titleInput = document.getElementById('act_name');
const categorySelect = document.getElementById('category');
const descriptionInput = document.getElementById('description');
const locationInput = document.getElementById('location');
const participantInput = document.getElementById('participant'); 
const dateInput = document.getElementById('end-date');
const timeInput = document.getElementById('end-time');

const createBtn = document.querySelector('.createpost-btn');
const cancelBtn = document.querySelector('.canclepost-btn');

const requiredFields = [
    titleInput, categorySelect, descriptionInput, 
    locationInput, participantInput, dateInput, timeInput
];

const setMinDate = () => {
    const now = new Date();
    const minDateStr = now.toISOString().split('T')[0];
    dateInput.setAttribute('min', minDateStr); 
};
setMinDate();

function triggerError(inputElement) {
    inputElement.classList.add('input-error');
    setTimeout(() => {
        inputElement.classList.remove('input-error');
    }, 3000); 
}

function isPastDateTime(dateStr, timeStr) {
    if (!dateStr || !timeStr) return false;
    const now = new Date();
    const selectedDateTime = new Date(`${dateStr}T${timeStr}`);
    return selectedDateTime < now;
}

createBtn.addEventListener('click', function(e) {
    e.preventDefault(); 

    let isFormValid = true;

    requiredFields.forEach(input => {
        if (!input.value || input.value.trim() === "") {
            triggerError(input);
            isFormValid = false;
        }
    });

    const memberCount = parseInt(participantInput.value);
    if (isNaN(memberCount) || memberCount < 1 || memberCount > 100) {
        alert("จำนวนสมาชิกต้องอยู่ระหว่าง 1 ถึง 100 คน");
        triggerError(participantInput);
        isFormValid = false;
    }

    if (!isFormValid) return;

    if (isPastDateTime(dateInput.value, timeInput.value)) {
        alert("ไม่สามารถเลือกเวลาในอดีตได้!");
        triggerError(dateInput);
        triggerError(timeInput);
        return;
    }

    const selectedMode = document.querySelector('input[name="join_mode"]:checked').value;
    const eventData = {
        title: titleInput.value.trim(),
        category: categorySelect.value,
        description: descriptionInput.value.trim(),
        location: locationInput.value.trim(),
        maxMembers: memberCount,
        deadline: `${dateInput.value}T${timeInput.value}`,
        mode: selectedMode,
        createdAt: new Date().toISOString()
    };

    console.log("Success:", eventData);
    alert("สร้างโพสต์กิจกรรมสำเร็จ!");
});

