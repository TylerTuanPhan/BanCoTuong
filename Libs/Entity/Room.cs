using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Libs.Entity
{
    public class Room
    {
        public Guid Id { get; set; } // Unique identifier for the room
        public string Name { get; set; } // Room name
        public List<Player> Players { get; set; } // List of players in the room
        public List<MoveChess> Moves { get; set; } // List of moves made in the room
        public List<ChessNode> ChessNodes { get; set; } // Current state of the chess pieces on the board
        public Room()
        {
            Players = new List<Player>();
            Moves = new List<MoveChess>();
            ChessNodes = new List<ChessNode>();
        }
    }
}
