// Use real data from DB injected by Index.cshtml
// Falls back to empty array if somehow not set
const eventsData = window.eventData || [];

const ITEMS_PER_PAGE = 4;
let currentPage = 1;

const container = document.getElementById('event-list-container');
const paginationContainer = document.getElementById('pagination');

function createMetaSpan(iconClass, text) {
    const span = document.createElement('span');
    const icon = document.createElement('i');
    iconClass.split(' ').forEach(cls => icon.classList.add(cls));
    icon.classList.add('icon-primary');
    span.appendChild(icon);
    span.append(` ${text}`);
    return span;
}

function createCard(event, index) {
    const card = document.createElement('div');
    card.classList.add('event-card');
    card.classList.add('fade-in-up');
    card.style.animationDelay = `${index * 0.1}s`;

    const cardHeader = document.createElement('div');
    cardHeader.classList.add('card-header');

    const timeBadge = document.createElement('span');
    timeBadge.classList.add('time-badge');
    timeBadge.textContent = event.postedAt;

    const bookmarkBtn = document.createElement('button');
    bookmarkBtn.classList.add('bookmark-btn');

    const bookmarkIcon = document.createElement('i');
    bookmarkIcon.classList.add('fa-regular', 'fa-bookmark');
    bookmarkBtn.appendChild(bookmarkIcon);

    cardHeader.append(timeBadge, bookmarkBtn);

    const cardBody = document.createElement('div');
    cardBody.classList.add('card-body');

    const titleDiv = document.createElement('h2');
    titleDiv.textContent = event.title;

    const authorDiv = document.createElement('p');
    authorDiv.classList.add('author');
    authorDiv.textContent = `By ${event.postedBy}`;

    cardBody.append(titleDiv, authorDiv);

    const cardFooter = document.createElement('div');
    cardFooter.classList.add('card-footer');

    const metaInfo = document.createElement('div');
    metaInfo.classList.add('meta-info');

    const categorySpan = createMetaSpan('fa-solid fa-briefcase', event.category);
    const appliedSpan  = createMetaSpan('fa-regular fa-square-check', `${event.currentMembers}/${event.maxMembers} applied`);
    const expiresSpan  = createMetaSpan('fa-regular fa-clock', `expires ${event.expiresAt}`);

    const statusSpan = document.createElement('span');
    statusSpan.textContent = event.status;

    metaInfo.append(categorySpan, appliedSpan, expiresSpan, statusSpan);

    const btnPrimary = document.createElement('button');
    btnPrimary.classList.add('btn-primary');
    btnPrimary.textContent = 'Event Detail';
    btnPrimary.onclick = () => {
        window.location.href = `/Event/Join?id=${event.id}`;
    };

    cardFooter.append(metaInfo, btnPrimary);

    card.append(cardHeader, cardBody, cardFooter);
    return card;
}

function renderPagination(page) {
    const totalPages = Math.ceil(eventsData.length / ITEMS_PER_PAGE);
    paginationContainer.innerHTML = '';

    if (totalPages <= 1) return;

    // Prev button
    if (page > 1) {
        const prevBtn = document.createElement('button');
        prevBtn.classList.add('page-btn', 'next');
        prevBtn.innerHTML = '<i class="fa-solid fa-chevron-left"></i> Prev';
        prevBtn.onclick = () => renderPage(page - 1);
        paginationContainer.appendChild(prevBtn);
    }

    // Numbered page buttons
    for (let i = 1; i <= totalPages; i++) {
        const btn = document.createElement('button');
        btn.classList.add('page-btn');
        if (i === page) btn.classList.add('active');
        btn.textContent = i;
        btn.onclick = () => renderPage(i);
        paginationContainer.appendChild(btn);
    }

    // Next button
    if (page < totalPages) {
        const nextBtn = document.createElement('button');
        nextBtn.classList.add('page-btn', 'next');
        nextBtn.innerHTML = 'Next <i class="fa-solid fa-chevron-right"></i>';
        nextBtn.onclick = () => renderPage(page + 1);
        paginationContainer.appendChild(nextBtn);
    }
}

function renderPage(page) {
    currentPage = page;
    container.innerHTML = '';

    const start = (page - 1) * ITEMS_PER_PAGE;
    const pageEvents = eventsData.slice(start, start + ITEMS_PER_PAGE);

    pageEvents.forEach((event, index) => {
        container.appendChild(createCard(event, index));
    });

    renderPagination(page);
}

renderPage(currentPage);
