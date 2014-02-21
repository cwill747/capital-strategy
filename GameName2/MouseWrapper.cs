using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace GameName2
{
    public class MouseWrapper
    {
        public Boolean isOverGrid { get; set; }
        public int row { get; set; }
        public int col { get; set; }
        public MouseState mouseState { get; set; }
        public MouseState pastState { get; set; }
        public Board board { get; set; }


        public MouseWrapper(Board board, MouseState mouseState)
        {
            this.board = board;
            this.mouseState = mouseState;
            update(mouseState);
            
        }

        public void update(MouseState mouseState)
        {
            this.pastState = this.mouseState;
            this.mouseState = mouseState;
            this.row = mouseState.Y / (board.height / board.rows);
            this.col = mouseState.X / (board.width / board.cols);
            this.isOverGrid = mouseState.X < board.width && mouseState.Y < board.height && mouseState.X >= 0 && mouseState.Y >= 0;
        }

        public Boolean wasClicked()
        {
            return this.mouseState.LeftButton == ButtonState.Pressed && this.pastState.LeftButton == ButtonState.Released;
        }
    }
}
