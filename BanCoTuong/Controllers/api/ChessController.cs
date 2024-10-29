using BanCoTuong.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.IO;
using System.Linq;

namespace BanCoTuong.Controllers.api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChessController : Controller
    {
        private readonly IWebHostEnvironment webHostEnvironment;
        private List<ChessNode> chessBoard;

        public ChessController(IWebHostEnvironment webHostEnvironment)
        {
            this.webHostEnvironment = webHostEnvironment;
            chessBoard = InitChessBoard();
        }

        // Khởi tạo bàn cờ từ file JSON
        private List<ChessNode> InitChessBoard()
        {
            string chessJsonPath = System.IO.Path.Combine(webHostEnvironment.ContentRootPath, "Data", "ChessJson.txt");

            if (System.IO.File.Exists(chessJsonPath))
            {
                string chessJson = System.IO.File.ReadAllText(chessJsonPath);
                return JsonSerializer.Deserialize<List<ChessNode>>(chessJson) ?? new List<ChessNode>();
            }
            return new List<ChessNode>();
        }

        // API load bàn cờ
        [HttpGet("loadChessBoard")]
        public IActionResult GetChessBoard()
        {
            var maxtrix = GenerateChessBoardMatrix();
            return Ok(new { status = true, message = "", maxtrix, chessNode = chessBoard });
        }

        private List<List<PointModel>> GenerateChessBoardMatrix()
        {
            List<List<PointModel>> matrix = new List<List<PointModel>>();
            for (int i = 0; i <= 9; i++)
            {
                int top = 61 + i * 74;
                List<PointModel> points = new List<PointModel>();
                for (int j = 0; j <= 8; j++)
                {
                    int left = 106 + j * 74;
                    PointModel point = new PointModel { top = top, left = left, id = "" };
                    ChessNode chess = chessBoard.FirstOrDefault(s => s.top == top && s.left == left);
                    if (chess != null)
                    {
                        point.id = chess.id;
                    }
                    points.Add(point);
                }
                matrix.Add(points);
            }
            return matrix;
        }

        // Thực hiện nước đi mới
        [HttpPost("movePiece")]
        public IActionResult MoveChess([FromBody] MoveChess move)
        {
            if (move == null)
            {
                return BadRequest("Dữ liệu không hợp lệ.");
            }

            // Tìm quân cờ theo ID
            var node = chessBoard.FirstOrDefault(n => n.id == move.id);
            if (node == null)
            {
                return NotFound("Quân cờ không tồn tại.");
            }

            // Kiểm tra tính hợp lệ của nước đi
            if (!IsValidMove(node, move.toi, move.toj))
            {
                return BadRequest("Nước đi không hợp lệ.");
            }

            // Cập nhật vị trí mới
            node.top = move.toi; // Cập nhật tọa độ
            node.left = move.toj;

            // Lưu lại trạng thái mới của bàn cờ vào file JSON
            SaveChessBoard();

            return Ok(new { status = true, message = "Nước đi đã được thực hiện.", chessNode = chessBoard });
        }

        // Phương thức kiểm tra tính hợp lệ của nước đi
        private bool IsValidMove(ChessNode node, int targetI, int targetJ)
        {
            // Kiểm tra quân cờ loại gì và gọi hàm tương ứng
            if (node.id.Contains("tuong"))
            {
                return IsValidGeneralMove(node, targetI, targetJ);
            }
            else if (node.id.Contains("ma"))
            {
                return IsValidHorseMove(node, targetI, targetJ);
            }
            // Thêm các loại quân cờ khác ở đây...

            return false; // Nếu không hợp lệ
        }

        // Ví dụ về hàm kiểm tra di chuyển của quân Tướng
        private bool IsValidGeneralMove(ChessNode node, int targetI, int targetJ)
        {
            var gapI = Math.Abs(targetI - node.top);
            var gapJ = Math.Abs(targetJ - node.left);

            // Quân Tướng chỉ có thể di chuyển một ô theo chiều ngang hoặc chiều dọc
            if (!((gapI == 1 && gapJ == 0) || (gapI == 0 && gapJ == 1)))
            {
                return false;
            }

            // Kiểm tra quân Tướng không ra ngoài khu vực của mình
            if (targetI < 7 || targetI > 9 || targetJ < 3 || targetJ > 5)
            {
                return false;
            }

            // Không được ăn quân đỏ
            if (IsEnemyPiece(targetI, targetJ))
            {
                return false;
            }

            return true; // Nước đi hợp lệ
        }

        // Ví dụ về hàm kiểm tra di chuyển của quân Mã
        private bool IsValidHorseMove(ChessNode node, int targetI, int targetJ)
        {
            var gapI = Math.Abs(targetI - node.top);
            var gapJ = Math.Abs(targetJ - node.left);

            // Mã có thể di chuyển theo hình chữ L (2 ô theo một chiều và 1 ô theo chiều khác)
            if (!((gapI == 2 && gapJ == 1) || (gapI == 1 && gapJ == 2)))
            {
                return false;
            }

            // Kiểm tra có quân chắn không
            if (gapI == 2 && gapJ == 1)
            {
                if (targetI > node.top && chessBoard.FirstOrDefault(n => n.top == node.top + 1 && n.left == node.left) != null) return false;
                if (targetI < node.top && chessBoard.FirstOrDefault(n => n.top == node.top - 1 && n.left == node.left) != null) return false;
            }
            else if (gapI == 1 && gapJ == 2)
            {
                if (targetJ > node.left && chessBoard.FirstOrDefault(n => n.top == node.top && n.left == node.left + 1) != null) return false;
                if (targetJ < node.left && chessBoard.FirstOrDefault(n => n.top == node.top && n.left == node.left - 1) != null) return false;
            }

            // Không được ăn quân đỏ
            if (IsEnemyPiece(targetI, targetJ))
            {
                return false;
            }

            return true; // Nước đi hợp lệ
        }

        // Kiểm tra quân cờ đối phương
        private bool IsEnemyPiece(int targetI, int targetJ)
        {
            var targetNode = chessBoard.FirstOrDefault(n => n.top == targetI && n.left == targetJ);
            return targetNode != null && targetNode.id.Contains("do"); // "do" cho quân đỏ
        }
        // Phương thức lưu bàn cờ vào file JSON
        private void SaveChessBoard()
        {
            // Đường dẫn đến file JSON nơi lưu trạng thái bàn cờ
            string chessJsonPath = System.IO.Path.Combine(webHostEnvironment.ContentRootPath, "Data", "ChessJson.txt");

            try
            {
                // Kiểm tra và tạo file nếu chưa tồn tại
                if (!System.IO.File.Exists(chessJsonPath))
                {
                    using (var fs = System.IO.File.Create(chessJsonPath))
                    {
                        // Tạo file nếu không tồn tại, không cần thêm gì vào đây
                    }
                }

                // Ghi nội dung bàn cờ vào file
                string updatedJson = JsonSerializer.Serialize(chessBoard, new JsonSerializerOptions
                {
                    WriteIndented = true // Định dạng JSON đẹp
                });

                // Ghi nội dung vào file
                System.IO.File.WriteAllText(chessJsonPath, updatedJson);
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu có
                // Có thể log lỗi hoặc thông báo cho người dùng
                Console.WriteLine($"Lỗi khi lưu bàn cờ: {ex.Message}");
            }
        }

        [HttpPost("resetChessBoard")]
        public IActionResult ResetChessBoard()
        {
            // Khôi phục bàn cờ từ file JSON
            chessBoard = InitChessBoard();

            if (chessBoard == null || !chessBoard.Any())
            {
                return BadRequest(new { status = false, message = "Không thể khôi phục bàn cờ. Dữ liệu không hợp lệ." });
            }

            return Ok(new { status = true, message = "Bàn cờ đã được reset.", chessNode = chessBoard });
        }

        [HttpGet("Board")]
        public IActionResult Board()
        {
            return View(); // Trả về view Board.cshtml
        }
    }
}
