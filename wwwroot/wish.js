const getWishBtn = document.getElementById('getWishBtn');
const friendNameInput = document.getElementById('friendName');
const wishResult = document.getElementById('wishResult');
const wishDisplay = document.getElementById('wishDisplay');

getWishBtn.addEventListener('click', async () => {
    const name = friendNameInput.value.trim();
    if (!name) {
        alert('Введите имя друга');
        return;
    }

    try {
        const response = await fetch(`/api/santa/wish/${encodeURIComponent(name)}`);
        if (!response.ok) {
            const err = await response.json();
            throw new Error(err.error || 'Не удалось получить пожелание');
        }

        const data = await response.json();
        wishResult.classList.remove('hidden');
        if (data.wish && data.wish !== '') {
            wishDisplay.innerText = `🎁 Пожелание ${data.name}: «${data.wish}»`;
        } else {
            wishDisplay.innerText = `😞 ${data.name} ещё не оставил(а) пожелание. Загляните позже!`;
        }
    } catch (error) {
        alert(error.message);
        wishResult.classList.add('hidden');
    }
});
