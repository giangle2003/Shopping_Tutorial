    const stars = document.querySelectorAll('.star-wrapper .fa');
    const ratingInput = document.getElementById('ratingValue');

    stars.forEach(star => {
        star.addEventListener('click', () => {
            const rating = parseInt(star.getAttribute('data-value'));
            ratingInput.value = rating;

            stars.forEach((s, i) => {
                s.className = i < rating ? 'fa fa-star selected' : 'fa fa-star-o';
            });
        });

        star.addEventListener('mouseover', () => {
            const hoverValue = parseInt(star.getAttribute('data-value'));
            stars.forEach((s, i) => {
                if (i < hoverValue) s.classList.add('hovered');
            });
        });

        star.addEventListener('mouseout', () => {
        stars.forEach(s => s.classList.remove('hovered'));
        });
    });
