// Use real data from DB injected by Index.cshtml
// Falls back to empty array if somehow not set
const eventsData = window.eventData || [];

const container = document.getElementById('event-list-container');

eventsData.forEach((event, index) => {

    const card = document.createElement('div');
    card.classList.add('event-card');

    card.classList.add('fade-in-up');
    card.style.animationDelay = `${index * 0.3}s`;

    const cardHeader = document.createElement('div');
    cardHeader.classList.add('card-header');

    const timeBadge = document.createElement('span');
    timeBadge.classList.add('time-badge');
    timeBadge.textContent = event.postedAt;  // was: event.time

    const bookmarkBtn = document.createElement('button');
    bookmarkBtn.classList.add('bookmark-btn');

    const bookmarkIcon = document.createElement('i');
    bookmarkIcon.classList.add('fa-regular', 'fa-bookmark');
    bookmarkBtn.appendChild(bookmarkIcon);

    cardHeader.append(timeBadge, bookmarkBtn);

    const cardBody = document.createElement('div');
    cardBody.classList.add('card-body');

    const titleDiv = document.createElement('h2');
    titleDiv.textContent = event.title;  // same field name

    const authorDiv = document.createElement('p');
    authorDiv.classList.add('author');
    authorDiv.textContent = `By ${event.postedBy}`;  // was: event.author

    cardBody.append(titleDiv, authorDiv);

    const cardFooter = document.createElement('div');
    cardFooter.classList.add('card-footer');

    const metaInfo = document.createElement('div');
    metaInfo.classList.add('meta-info');

    function createMetaSpan(iconClass, text) {
        const span = document.createElement('span');
        const icon = document.createElement('i');
        iconClass.split(' ').forEach(cls => icon.classList.add(cls));
        icon.classList.add('icon-primary');
        span.appendChild(icon);
        span.append(` ${text}`);
        return span;
    }

    const categorySpan = createMetaSpan('fa-solid fa-briefcase', event.category);
    const appliedSpan  = createMetaSpan('fa-regular fa-square-check', `${event.currentMembers}/${event.maxMembers} applied`);  // was: event.applied
    const expiresSpan  = createMetaSpan('fa-regular fa-clock', `expires ${event.expiresAt}`);  // was: event.expires

    const statusSpan = document.createElement('span');
    statusSpan.textContent = event.status;  // same field name

    metaInfo.append(categorySpan, appliedSpan, expiresSpan, statusSpan);

    // "Event Detail" button links to the actual post detail page
    const btnPrimary = document.createElement('button');
    btnPrimary.classList.add('btn-primary');
    btnPrimary.textContent = 'Event Detail';
    btnPrimary.onclick = () => {
        window.location.href = `/ActivityPost/Index?id=${event.id}`;
    };

    cardFooter.append(metaInfo, btnPrimary);

    card.append(cardHeader, cardBody, cardFooter);

    container.appendChild(card);
});
