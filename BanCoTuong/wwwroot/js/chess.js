// Tạo bảng cờ và xử lý các quân cờ
document.addEventListener('DOMContentLoaded', function () {
    const board = document.getElementById('chess-board');
    const cells = board.querySelectorAll('.cell');

    cells.forEach(cell => {
        cell.addEventListener('click', function () {
            const row = this.getAttribute('data-row');
            const col = this.getAttribute('data-col');
            console.log(`Bạn đã nhấn vào ô: Hàng ${row}, Cột ${col}`);
        });
    });
});
