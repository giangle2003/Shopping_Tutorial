$(function () {

    if ($("a.confirmDeletion").length) {
        $("a.confirmDeletion").click(() => {
            if (!confirm("Bạn có đồng ý xóa không ?")) return false;
        });
    }

    if ($("div.alert.notification").length) {
        setTimeout(() => {
            $("div.alert.notification").fadeOut();
        }, 2000);
    }

});
document.addEventListener("DOMContentLoaded", function () {
    var deleteLinks = document.querySelectorAll(".delete-link");

    deleteLinks.forEach(function (link) {
        link.addEventListener("click", function (event) {
            event.preventDefault(); // Ngăn chặn điều hướng ngay lập tức

            Swal.fire({
                title: "Bạn có chắc chắn muốn xóa?",
                text: "Dữ liệu này sẽ bị xóa vĩnh viễn!",
                icon: "warning",
                showCancelButton: true,
                confirmButtonColor: "#d33",
                cancelButtonColor: "#3085d6",
                confirmButtonText: "Xóa",
                cancelButtonText: "Hủy"
            }).then((result) => {
                if (result.isConfirmed) {
                    window.location.href = link.href; // Chuyển hướng nếu xác nhận
                }
            });
        });
    });
});
