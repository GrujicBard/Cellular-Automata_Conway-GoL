using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace IS_naloga_2
{
    public class Game
    {
        public Cell[,] Cells { get; set; }
        public Cell[,] NCells { get; set; }
        const int width = 50;
        const int height = 50;
        public int Columns { get { return Cells.GetLength(0); } }
        public int Rows { get { return Cells.GetLength(1); } }

        public Game(bool[,] caveMap)
        {
            Cells = new Cell[width, height];
            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    Cells[x, y] = new Cell();
                    if (caveMap[x, y]) { Cells[x, y].SetNextState(Cell.State.CaveWall); }
                }
            }
            foreach (var cell in Cells) { cell.Refresh(); }
            NCells = Cells.Clone() as Cell[,];

        }

        public void Advance()
        {
            DetermineNextState();
            Refresh();
            Cells = NCells.Clone() as Cell[,];
        }

        public void DetermineNextState()
        {
            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    switch (Element(x, y).GetState())
                    {
                        case Cell.State.Wood:
                            if (IfElementAround(x, y, Cell.State.Fire) || IfElementAround(x, y, Cell.State.WoodOnFire))
                            {
                                Element(x, y).SetNextState(Cell.State.WoodOnFire);
                                break;
                            }
                            if (IfBottomElement(x, y, Cell.State.EmptyCell))
                            {
                                MoveCellsDown(x, y);
                            }
                            break;
                        case Cell.State.Water:
                            if (Element(x, y).Volume == 0.00)
                            {
                                Element(x, y).SetNextState(Cell.State.EmptyCell);
                                break;
                            }
                            if (IfElementAround(x, y, Cell.State.Fire))
                            {
                                Element(x, y).SetNextState(Cell.State.WhiteSmoke);
                                break;
                            }
                            if (IfBottomElement(x, y, Cell.State.EmptyCell))
                            {
                                MoveCellsDown(x, y);
                            }
                            else if (!IfSpaceDown(x, y))
                            {
                                SplitWaterMiddle(x, y, SumWaterMiddle(x, y));
                                if (BottomElement(x, y).GetState() == Cell.State.Water && Element(x, y).Volume <= 100.00)
                                {
                                    SpreadWater(BottomElement(x, y), Element(x, y).Volume);
                                    Element(x, y).Volume = 0.00;
                                }
                                if(Element(x, y).Volume > 100.00)
                                {
                                    if (IfTopElement(x, y, Cell.State.Wood))
                                    {
                                        NCells[x, y - 1] = Cells[x, y - 2];
                                        NCells[x, y - 2] = Cells[x, y - 1];
                                    }
                                    //Split leftover water TopLeft, Top, TopRight
                                    double volumeOverLimit = (Element(x, y).Volume - 100.0);
                                    double leftover = Element(x, y).Volume - volumeOverLimit;
                                    SpreadWater(TopElement(x, y), volumeOverLimit);
                                    Element(x, y).Volume = leftover;
                                }
                            }
                            break;
                        case Cell.State.Fire:
                            if (IfElementAround(x, y, Cell.State.Sand))
                            {
                                Element(x, y).SetNextState(Cell.State.WhiteSmoke);
                                break;
                            }
                            if (IfElementAround(x, y, Cell.State.Wood) || IfElementAround(x, y, Cell.State.Water))
                            {
                                Element(x, y).SetNextState(Cell.State.EmptyCell);
                                break;
                            }
                            if (IfBottomElement(x, y, Cell.State.CaveWall))
                            {
                                Element(x, y).Counter++;
                            }
                            if (Element(x, y).Counter == 1)
                            {
                                Element(x, y).SetNextState(Cell.State.EmptyCell);
                                break;
                            }
                            if (IfBottomElement(x, y, Cell.State.EmptyCell))
                            {
                                MoveCellsDown(x, y);
                            }
                            break;
                        case Cell.State.Sand:
                            Random rnd = new Random();
                            if (IfBottomElement(x, y, Cell.State.EmptyCell) || IfBottomElement(x, y, Cell.State.Water))
                            {
                                MoveCellsDown(x, y);
                            }
                            else if ((rnd.Next(0, 2) != 0))
                            {
                                if (IfBottomRightElement(x, y, Cell.State.EmptyCell) || IfBottomRightElement(x, y, Cell.State.Water))
                                {
                                    MoveCellRightDown(x, y);
                                }
                                else if (IfBottomLeftElement(x, y, Cell.State.EmptyCell) || IfBottomLeftElement(x, y, Cell.State.Water))
                                {
                                    MoveCellLeftDown(x, y);
                                }
                            }
                            else
                            {
                                if (IfBottomLeftElement(x, y, Cell.State.EmptyCell) || IfBottomLeftElement(x, y, Cell.State.Water))
                                {
                                    MoveCellLeftDown(x, y);
                                }
                                else if (IfBottomRightElement(x, y, Cell.State.EmptyCell) || IfBottomRightElement(x, y, Cell.State.Water))
                                {
                                    MoveCellRightDown(x, y);
                                }
                            }
                            break;
                        case Cell.State.WhiteSmoke:
                        case Cell.State.BlackSmoke:
                            if (IfTopNotElement(x, y, Cell.State.CaveWall) && IfTopNotElement(x, y, Cell.State.WhiteSmoke) && IfTopNotElement(x, y, Cell.State.BlackSmoke))
                            {
                                MoveCellsUp(x, y);
                            }
                            if (Element(x, y).Counter == 3)
                            {
                                Element(x, y).SetNextState(Cell.State.EmptyCell);
                                break;
                            }
                            Element(x, y).Volume = Element(x, y).Volume - 25;

                            Element(x, y).Counter++;
                            break;
                        case Cell.State.WoodOnFire:
                            if (IfBottomElement(x, y, Cell.State.EmptyCell))
                            {
                                MoveCellsDown(x, y);
                            }
                            if (Element(x, y).Counter == 1)
                            {
                                Element(x, y).SetNextState(Cell.State.BlackSmoke);
                                break;
                            }
                            Element(x, y).Counter++;
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public void Refresh()
        {
            foreach (var cell in NCells)
            {
                cell.Refresh();
            }
        }

        public double SumWaterMiddle(int x, int y)
        {
            double volume = Cells[x, y].Volume;
            int counter = 1;
            int index = 1;

            while (true)
            {
                if (Cells[x - index, y].GetState() == Cell.State.Water || Cells[x - index, y].GetState() == Cell.State.EmptyCell)
                {
                    counter++;
                }
                if (Cells[x - index, y].GetState() == Cell.State.Water)
                {
                    volume += Cells[x - index, y].Volume;
                }
                if (Cells[x - index, y].GetState() == Cell.State.CaveWall || Cells[x - index, y].GetState() == Cell.State.Sand
                    || Cells[x - index, y].GetState() == Cell.State.Wood) break;  //Check left of element untill obstacle
                if (Cells[x - index, y + 1].GetState() == Cell.State.EmptyCell || Cells[x - index, y + 1].GetState() == Cell.State.Water
                    || Cells[x - index, y + 1].GetState() == Cell.State.Sand && Cells[x - (index - 1), y + 1].GetState() == Cell.State.CaveWall) break; //Check bottomright of element untill emptycell
                index++;
            }
            index = 1;
            while (true)
            {
                if (Cells[x + index, y].GetState() == Cell.State.Water || Cells[x + index, y].GetState() == Cell.State.EmptyCell)
                {
                    counter++;
                }
                if (Cells[x + index, y].GetState() == Cell.State.Water)
                {
                    volume += Cells[x + index, y].Volume;
                }
                if (Cells[x + index, y].GetState() == Cell.State.CaveWall || Cells[x + index, y].GetState() == Cell.State.Sand
                    || Cells[x + index, y].GetState() == Cell.State.Wood) break;  //Check right of element untill obstacle
                if (Cells[x + index, y + 1].GetState() == Cell.State.EmptyCell || Cells[x + index, y + 1].GetState() == Cell.State.Water
                    || Cells[x + index, y + 1].GetState() == Cell.State.Sand && Cells[x + (index - 1), y + 1].GetState() == Cell.State.CaveWall) break; //Check bottomright of element untill emptycell 
                index++;
            }
            return volume / counter;
        }

        public void SplitWaterMiddle(int x, int y, double volume)
        {
            if (volume >0.00)
            {
                int index = 1;
                while (true)
                {
                    if (Cells[x - index, y].GetState() == Cell.State.Water)
                    {
                        Cells[x - index, y].Volume = volume;
                    }
                    else if (Cells[x - index, y].GetState() == Cell.State.EmptyCell)
                    {
                        Cells[x - index, y].SetNextState(Cell.State.Water);
                        Cells[x - index, y].Volume = volume;
                    }
                    if (Cells[x - index, y].GetState() == Cell.State.CaveWall || Cells[x - index, y].GetState() == Cell.State.Sand
                        || Cells[x - index, y].GetState() == Cell.State.Wood) break;  //Check left of element untill obstacle //Check right of element untill wall
                    if (Cells[x - index, y + 1].GetState() == Cell.State.EmptyCell || Cells[x - index, y + 1].GetState() == Cell.State.Water
                        || Cells[x - index, y + 1].GetState() == Cell.State.Sand && Cells[x - (index - 1), y + 1].GetState() == Cell.State.CaveWall) break; //Check bottomright of element untill emptycell
                    index++;
                }
                index = 1;
                while (true)
                {
                    if (Cells[x + index, y].GetState() == Cell.State.Water)
                    {
                        Cells[x + index, y].Volume = volume;
                    }
                    else if (Cells[x + index, y].GetState() == Cell.State.EmptyCell)
                    {
                        Cells[x + index, y].SetNextState(Cell.State.Water);
                        Cells[x + index, y].Volume = volume;
                    }
                    if (Cells[x + index, y].GetState() == Cell.State.CaveWall || Cells[x + index, y].GetState() == Cell.State.Sand
                        || Cells[x + index, y].GetState() == Cell.State.Wood) break;  //Check right of element untill obstacle
                    if (Cells[x + index, y + 1].GetState() == Cell.State.EmptyCell || Cells[x + index, y + 1].GetState() == Cell.State.Water
                        || Cells[x + index, y + 1].GetState() == Cell.State.Sand && Cells[x + (index - 1), y + 1].GetState() == Cell.State.CaveWall) break; //Check bottomright of element untill emptycell 
                    index++;
                }
                Cells[x, y].Volume = volume;
            }
        }

        public int CountWater(int x, int y)
        {
            int counter = 1;

            if (TopLeftElement(x, y).GetState() == Cell.State.Water || TopLeftElement(x, y).GetState() == Cell.State.EmptyCell) counter++;
            if (TopElement(x, y).GetState() == Cell.State.Water || TopElement(x, y).GetState() == Cell.State.EmptyCell) counter++;
            if (TopRightElement(x, y).GetState() == Cell.State.Water || TopRightElement(x, y).GetState() == Cell.State.EmptyCell) counter++;
            if (LeftElement(x, y).GetState() == Cell.State.Water || LeftElement(x, y).GetState() == Cell.State.EmptyCell) counter++;
            if (RightElement(x, y).GetState() == Cell.State.Water || RightElement(x, y).GetState() == Cell.State.EmptyCell) counter++;
            if (BottomLeftElement(x, y).GetState() == Cell.State.Water || BottomLeftElement(x, y).GetState() == Cell.State.EmptyCell) counter++;
            if (BottomElement(x, y).GetState() == Cell.State.Water || BottomElement(x, y).GetState() == Cell.State.EmptyCell) counter++;
            if (BottomRightElement(x, y).GetState() == Cell.State.Water || BottomRightElement(x, y).GetState() == Cell.State.EmptyCell) counter++;

            return counter;
        }

        public int CountWaterMinusTop(int x, int y)
        {
            int counter = 1;

            if (LeftElement(x, y).GetState() == Cell.State.Water || LeftElement(x, y).GetState() == Cell.State.EmptyCell) counter++;
            if (RightElement(x, y).GetState() == Cell.State.Water || RightElement(x, y).GetState() == Cell.State.EmptyCell) counter++;
            if (BottomLeftElement(x, y).GetState() == Cell.State.Water || BottomLeftElement(x, y).GetState() == Cell.State.EmptyCell) counter++;
            if (BottomElement(x, y).GetState() == Cell.State.Water || BottomElement(x, y).GetState() == Cell.State.EmptyCell) counter++;
            if (BottomRightElement(x, y).GetState() == Cell.State.Water || BottomRightElement(x, y).GetState() == Cell.State.EmptyCell) counter++;
            return counter;
        }

        public int CountWaterBottom(int x, int y)
        {
            int counter = 1;

            if (BottomLeftElement(x, y).GetState() == Cell.State.Water || BottomLeftElement(x, y).GetState() == Cell.State.EmptyCell) counter++;
            if (BottomElement(x, y).GetState() == Cell.State.Water || BottomElement(x, y).GetState() == Cell.State.EmptyCell) counter++;
            if (BottomRightElement(x, y).GetState() == Cell.State.Water || BottomRightElement(x, y).GetState() == Cell.State.EmptyCell) counter++;
            return counter;
        }

        public int CountWaterBottomIfNotFull(int x, int y)
        {
            int counter = 0;

            if (BottomLeftElement(x, y).GetState() == Cell.State.Water && BottomLeftElement(x, y).Volume < 100.00) counter++;
            if (BottomElement(x, y).GetState() == Cell.State.Water && BottomElement(x, y).Volume < 100.00) counter++;
            if (BottomRightElement(x, y).GetState() == Cell.State.Water && BottomRightElement(x, y).Volume < 100.00) counter++;
            return counter;
        }

        public int CountWaterTop(int x, int y)
        {
            int counter = 0;

            if (TopLeftElement(x, y).GetState() == Cell.State.Water || TopLeftElement(x, y).GetState() == Cell.State.EmptyCell) counter++;
            if (TopElement(x, y).GetState() == Cell.State.Water || TopElement(x, y).GetState() == Cell.State.EmptyCell) counter++;
            if (TopRightElement(x, y).GetState() == Cell.State.Water || TopRightElement(x, y).GetState() == Cell.State.EmptyCell) counter++;
            return counter;
        }

        public int CountWaterMiddle(int x, int y)
        {
            int counter = 1;

            if (LeftElement(x, y).GetState() == Cell.State.Water || LeftElement(x, y).GetState() == Cell.State.EmptyCell) counter++;
            if (RightElement(x, y).GetState() == Cell.State.Water || RightElement(x, y).GetState() == Cell.State.EmptyCell) counter++;
            return counter;
        }

        public void SpreadWater(Cell element, double volume)
        {
            if (volume > 0.00)
            {
                if (element.GetState() == Cell.State.EmptyCell && element.Volume == 100.00) //MAYBE WRONG  && volume >= 1.0
                {
                    element.SetNextState(Cell.State.Water);
                    element.Volume = 0.00;
                    element.Volume += volume;
                }
                else if (element.GetState() == Cell.State.Water)
                {
                    element.Volume += volume;
                }
            }
        }

        public bool IfSpaceDown(int x, int y)
        {
            bool result = false;
            int index = 1;
            Cell cell = new Cell();
            while (cell.GetState() != Cell.State.CaveWall)
            {
                if (Cells[x, y + index].GetState() == Cell.State.EmptyCell) result = true;
                cell = Cells[x, y + index];
                index++;
            }
            return result;
        }

        public Cell Element(int x, int y)
        {
            return Cells[x, y];
        }

        public Cell TopLeftElement(int x, int y)
        {
            return Cells[x - 1, y - 1];
        }

        public Cell LeftElement(int x, int y)
        {
            return Cells[x - 1, y];
        }

        public Cell BottomLeftElement(int x, int y)
        {
            return Cells[x - 1, y + 1];
        }

        public Cell TopElement(int x, int y)
        {
            return Cells[x, y - 1];
        }

        public Cell BottomElement(int x, int y)
        {
            return Cells[x, y + 1];
        }
        public Cell TopRightElement(int x, int y)
        {
            return Cells[x + 1, y - 1];
        }

        public Cell RightElement(int x, int y)
        {
            return Cells[x + 1, y];
        }
        public Cell BottomRightElement(int x, int y)
        {
            return Cells[x + 1, y + 1];
        }

        public bool IfBottomNotElement(int x, int y, Cell.State element)
        {
            return BottomElement(x, y).GetState() != element;
        }

        public bool IfTopNotElement(int x, int y, Cell.State element)
        {
            return TopElement(x, y).GetState() != element;
        }

        public bool IfBottomLeftNotElement(int x, int y, Cell.State element)
        {
            return BottomLeftElement(x, y).GetState() != element;
        }

        public bool IfBottomRightNotElement(int x, int y, Cell.State element)
        {
            return BottomRightElement(x, y).GetState() != element;
        }

        public bool IfBottomLeftElement(int x, int y, Cell.State element)
        {
            return BottomLeftElement(x, y).GetState() == element;
        }

        public bool IfBottomRightElement(int x, int y, Cell.State element)
        {
            return BottomRightElement(x, y).GetState() == element;
        }

        public bool IfBottomElement(int x, int y, Cell.State element)
        {
            return BottomElement(x, y).GetState() == element;
        }
        public bool IfTopElement(int x, int y, Cell.State element)
        {
            return TopElement(x, y).GetState() == element;
        }

        public bool IfLeftElement(int x, int y, Cell.State element)
        {
            return LeftElement(x, y).GetState() == element;
        }

        public bool IfRightElement(int x, int y, Cell.State element)
        {
            return RightElement(x, y).GetState() == element;
        }

        public bool IfElementAround(int x, int y, Cell.State element)
        {
            bool b = false;
            if (TopLeftElement(x, y).GetState() == element || TopElement(x, y).GetState() == element || TopRightElement(x, y).GetState() == element
                || LeftElement(x, y).GetState() == element || RightElement(x, y).GetState() == element || BottomLeftElement(x, y).GetState() == element
                || BottomElement(x, y).GetState() == element || BottomRightElement(x, y).GetState() == element)
            {
                b = true;
            }
            return b;
        }

        public void MoveCellsDown(int x, int y)
        {
            NCells[x, y] = BottomElement(x, y);
            NCells[x, y + 1] = Element(x, y);
        }

        public void MoveCellsUp(int x, int y)
        {
            NCells[x, y] = TopElement(x, y);
            NCells[x, y - 1] = Element(x, y);
        }

        public void MoveCellRightDown(int x, int y)
        {
            NCells[x, y] = BottomRightElement(x, y);
            NCells[x + 1, y + 1] = Element(x, y);
        }

        public void MoveCellLeftDown(int x, int y)
        {
            NCells[x, y] = BottomLeftElement(x, y);
            NCells[x - 1, y + 1] = Element(x, y);
        }
    }
}
