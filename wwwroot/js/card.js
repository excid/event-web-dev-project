const eventsData = [
    {
        time: "10 min ago",
        title: "Looking Football Teammate",
        author: "By Owner",
        category: "Sports",
        applied: "1/3 applied",
        expires: "expires in 2 days",
        status: "Open"
    },
    {
        time: "1 hour ago",
        title: "Board Game Weekend (Avalon)",
        author: "By Admin",
        category: "Entertainment",
        applied: "4/6 applied",
        expires: "expires today",
        status: "Urgent"
    },
    {
        time: "2 days ago",
        title: "UI/UX Designer Meetup",
        author: "By Community",
        category: "Design",
        applied: "15/20 applied",
        expires: "expires in 1 week",
        status: "Open"
    }
];

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
    timeBadge.textContent = event.time; 

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
    authorDiv.textContent = event.author;

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
    const appliedSpan = createMetaSpan('fa-regular fa-square-check', event.applied);
    const expiresSpan = createMetaSpan('fa-regular fa-clock', event.expires);
    
    const statusSpan = document.createElement('span');
    statusSpan.textContent = event.status;

    metaInfo.append(categorySpan, appliedSpan, expiresSpan, statusSpan);

    const btnPrimary = document.createElement('button');
    btnPrimary.classList.add('btn-primary');
    btnPrimary.textContent = 'Event Detail';

    cardFooter.append(metaInfo, btnPrimary);

    card.append(cardHeader, cardBody, cardFooter);

    container.appendChild(card);
});