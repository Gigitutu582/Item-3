const registerBtn = document.getElementById('registerBtn');
const userNameInput = document.getElementById('userName');
const resultArea = document.getElementById('resultArea');
const greeting = document.getElementById('greeting');
const wishArea = document.querySelector('.wish-area');
const wishText = document.getElementById('wishText');
const sendWishBtn = document.getElementById('sendWishBtn');
const wishStatus = document.getElementById('wishStatus');

let currentUserName = '';
let currentGiftFor = null;

registerBtn.addEventListener('click', async () => {
    const name = userNameInput.value.trim();
    if (!name) {
        alert('Введите ваше имя');
        return;
    }

    try {
        const response = await fetch('/api/santa/register', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ name: name })
        });

        if (!response.ok) {
            const err = await response.json();
            throw new Error(err.error || 'Ошибка регистрации');
        }

        const data = await response.json();
        currentUserName = data.userName;
        currentGiftFor = data.giftFor;

        resultArea.classList.remove('hidden');
        if (currentGiftFor) {
            greeting.innerText = `Привет, ${currentUserName}! Твой тайный друг — ${currentGiftFor}. Не забудь оставить пожелание!`;
            wishArea.classList.remove('hidden');
        } else {
            greeting.innerText = `${currentUserName}, вы зарегистрированы. Как только присоединится ещё хотя бы один участник, вы получите подопечного.`;
            wishArea.classList.add('hidden');
        }
        wishStatus.innerText = '';
        wishText.value = '';
    } catch (error) {
        alert(error.message);
    }
});

sendWishBtn.addEventListener('click', async () => {
    if (!currentUserName) {
        alert('Сначала зарегистрируйтесь');
        return;
    }
    const wish = wishText.value.trim();
    if (!wish) {
        alert('Напишите ваше пожелание');
        return;
    }

    try {
        const response = await fetch('/api/santa/wish', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ name: currentUserName, wish: wish })
        });

        if (!response.ok) {
            const err = await response.json();
            throw new Error(err.error || 'Ошибка сохранения');
        }

        wishStatus.innerText = '✅ Пожелание сохранено!';
        wishText.value = '';
    } catch (error) {
        alert(error.message);
    }
});