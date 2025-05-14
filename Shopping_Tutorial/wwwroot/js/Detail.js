
//CÓ thể quan tâm
//Dùng để chuyển slide ảnh sản phẩm

document.addEventListener("DOMContentLoaded", function () {
		const track = document.querySelector(".carousel-track");
	const items = document.querySelectorAll(".carousel-item");
	const prevButton = document.getElementById("prevSlide");
	const nextButton = document.getElementById("nextSlide");

	let currentIndex = 0;
	const totalItems = items.length;
	const visibleItems = 3;

	function updateSlide() {
			const translateX = -currentIndex * (100 / visibleItems) + "%";
	track.style.transition = "transform 0.5s ease-in-out";
	track.style.transform = "translateX(" + translateX + ")";
		}

	nextButton.addEventListener("click", function (e) {
		e.preventDefault();
	if (currentIndex < totalItems - visibleItems) {
		currentIndex++;
			} else {
		currentIndex = 0; // Quay lại sản phẩm đầu tiên
			}
	updateSlide();
		});

	prevButton.addEventListener("click", function (e) {
		e.preventDefault();
			if (currentIndex > 0) {
		currentIndex--;
			} else {
		currentIndex = totalItems - visibleItems; // Quay về cuối danh sách
			}
	updateSlide();
		});

	// Tự động chạy vòng tròn mỗi 3 giây
	setInterval(function () {
		nextButton.click();
		}, 5000);
	});
