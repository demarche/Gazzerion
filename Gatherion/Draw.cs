using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DxLibDLL;
using System.Drawing;

namespace Gatherion
{
    class Draw
    {
        Size screenSize = new Size(1280,720);
        Size fieldSize;
        Dictionary<int, string> elementDict = new Dictionary<int, string>()
        {
            {0,"火" },{1,"水" },{2,"土" },{3,"木" },{4,"金" }
        };
        Dictionary<int, uint> elementColDict;

        uint LineCol = 0;
        //マス目の一辺の長さ
        double grid_len;
        //開始位置
        Point GridStart;
        //カードの大きさ
        Size cardScale;
        Size cardSize;

        //init
        public Draw(GameManager game)
        {
            fieldSize = game.fieldSize;
            cardSize = game.cardSize;

            //グリッドの色
            LineCol = DX.GetColor(255, 255, 255);

            //グリッドの1マスのサイズ
            grid_len = screenSize.Height / (fieldSize.Height + 2);
            //グリッドの右上座標
            GridStart = new Point((int)(screenSize.Width / 2 - grid_len * fieldSize.Width / 2), (int)grid_len);

            //属性の色
            elementColDict = new Dictionary<int, uint>()
                {
                    {0,DX.GetColor(255,0,0) },{1,DX.GetColor(0,0,255)},{2,DX.GetColor(0,0,0) },{3,DX.GetColor(0,255,0) },{4,DX.GetColor(255,255,0) }
                };

            //カードの大きさ
            cardScale = new Size((int)(grid_len * cardSize.Width), (int)(grid_len * cardSize.Height));
        }

        //勝利メッセージ描画
        public void DrawWinnerMessage(string winnerMsg)
        {
            // 描画する文字列のサイズを設定
            int defaultFontSize = DX.GetFontSize();
            DX.SetFontSize(64);

            int msgWidth = DX.GetDrawStringWidth(winnerMsg, winnerMsg.Length);

            // 文字列の描画
            DX.DrawString(screenSize.Width / 2 - msgWidth / 2, screenSize.Height / 2 - 32, winnerMsg, DX.GetColor(255, 0, 0));

            // サイズ戻す
            DX.SetFontSize(defaultFontSize);
            
        }

        //プレイヤーの状態描画
        public void DrawState(GameManager game)
        {
            //山札枚数
            //1P
            uint deckCol = DX.GetColor(255, 255, 255);
            Point deck_1P = new Point(GridStart.X / 2 - cardScale.Width / 2, screenSize.Height - (int)(grid_len * 2) - cardScale.Height);
            string decknum_1P = game.card_1p.Count().ToString();
            int decknumLen_1P = DX.GetDrawStringWidth(decknum_1P, decknum_1P.Length);
            DX.DrawBox(deck_1P.X, deck_1P.Y, deck_1P.X + cardScale.Width, deck_1P.Y + cardScale.Height, deckCol, 1);
            DX.DrawString(deck_1P.X + cardScale.Width / 2 - decknumLen_1P / 2, deck_1P.Y + cardScale.Height / 2- DX.GetFontSize()/2, decknum_1P, DX.GetColor(0, 0, 0));
            //2P
            Point deck_2P = new Point(screenSize.Width/2 + (GridStart.X + (int)(grid_len * fieldSize.Width)) / 2 - cardScale.Width / 2,
                 (int)(grid_len));
            string decknum_2P = game.card_2p.Count().ToString();
            int decknumLen_2P = DX.GetDrawStringWidth(decknum_2P, decknum_2P.Length);
            DX.DrawBox(deck_2P.X, deck_2P.Y, deck_2P.X + cardScale.Width, deck_2P.Y + cardScale.Height, deckCol, 1);
            DX.DrawString(deck_2P.X + cardScale.Width / 2 - decknumLen_2P / 2, deck_2P.Y + cardScale.Height / 2 - DX.GetFontSize() / 2, decknum_2P, DX.GetColor(0, 0, 0));

            //現在のプレイヤー表示
            string msg = (game.is1P ? "1" : "2") + "Pの番です";
            int msg_len = DX.GetDrawStringWidth(msg, msg.Length);
            DX.DrawString(deck_2P.X - msg_len / 2, screenSize.Height / 2, msg, DX.GetColor(0, 255 * (game.is1P ? 1 : 0), 255 * (game.is1P ? 0 : 1)));

            //メッセージ表示
            int showMessage = 4;
            Point msgPt = new Point(GridStart.X + (int)(grid_len * (fieldSize.Width + 1)), deck_2P.Y + cardScale.Height+ DX.GetFontSize());
            DX.DrawBox(msgPt.X, msgPt.Y, msgPt.X + DX.GetFontSize() * 8, msgPt.Y + DX.GetFontSize() * showMessage, DX.GetColor(128, 128, 128), 1);
            foreach(var elem in game.messageList.Reverse<string>().Take(showMessage).Select((v,i) => new { v, i }))
            {
                bool is1PDoc = elem.v.Contains("1P:");
                DX.DrawString(msgPt.X, msgPt.Y + DX.GetFontSize() * elem.i,
                    elem.v,
                    DX.GetColor(0, 255 * (is1PDoc ? 1 : 0), 255 * (is1PDoc ? 0 : 1)));
            }

            //イニシエーション
            string init_1p = "1Pinit:" + (game.initiation[0] ? "可" : "不可");
            string init_2p = "2Pinit:" + (game.initiation[1] ? "可" : "不可");
            string skill_1p = "1PSkill:" + game.skillPt_1p.ToString();
            string skill_2p = "2PSkill:" + game.skillPt_2p.ToString();
            int init_1p_len = DX.GetDrawStringWidth(init_1p, init_1p.Length);
            int init_2p_len = DX.GetDrawStringWidth(init_2p, init_2p.Length);
            int skill_1p_len = DX.GetDrawStringWidth(skill_1p, skill_1p.Length);
            int skill_2p_len = DX.GetDrawStringWidth(skill_2p, skill_2p.Length);
            DX.DrawString(GridStart.X / 2 - init_1p_len / 2, screenSize.Height / 2-40, init_1p, DX.GetColor(255, 255, 255));
            DX.DrawString(GridStart.X / 2 - init_2p_len / 2, screenSize.Height / 2- 20, init_2p, DX.GetColor(255, 255, 255));
            DX.DrawString(GridStart.X / 2 - skill_1p_len / 2, screenSize.Height / 2 + 20, skill_1p, DX.GetColor(255, 255, 255));
            DX.DrawString(GridStart.X / 2 - skill_2p_len / 2, screenSize.Height / 2 + 40, skill_2p, DX.GetColor(255, 255, 255));
        }

        //グリッド線描画
        //重い
        public void DrawGrid()
        {
            for(int x = 0; x <= fieldSize.Width; x++)
            {
                DX.DrawLine((int)(GridStart.X + x * grid_len), GridStart.Y, (int)(GridStart.X + x * grid_len), (int)(GridStart.Y + grid_len * fieldSize.Height), LineCol);
            }

            for (int y = 0; y <= fieldSize.Height; y++)
            {
                DX.DrawLine(GridStart.X, (int)(GridStart.Y + y * grid_len), (int)(GridStart.X + grid_len * fieldSize.Width), (int)(GridStart.Y + y * grid_len), LineCol);
            }
        }

        //アシスト表示
        public void DrawAssist(GameManager game, List<Card> candidates, int handCardCur)
        {
            if (handCardCur < 0) return;
            foreach (var card in candidates)
            {
                if (card.handCardID != (game.is1P ? game.handCard_1p : game.handCard_2p)[handCardCur].handCardID
                    || card.turn != (game.is1P ? game.handCard_1p : game.handCard_2p)[handCardCur].turn)
                    continue;

                Size mycardSize = new Size(game.cardSize.Width, game.cardSize.Height);
                if (card.turn % 2 == 1) mycardSize = new Size(mycardSize.Height, mycardSize.Width);

                DX.DrawBox(GridStart.X + (int)(grid_len * card.point.X), GridStart.Y + (int)(grid_len * card.point.Y),
                    GridStart.X + (int)(grid_len * (card.point.X + mycardSize.Width)), GridStart.Y + (int)(grid_len * (card.point.Y + mycardSize.Height)),
                    DX.GetColor(0, 128, 0), 1);
            }
        }

        //バースト表示
        public void DrawBurst(GameManager game, List<bool> isBurst)
        {
            if (isBurst == null || isBurst.Count(t => t) == 0) return;

            List<int> burstNum = Enumerable.Range(0, isBurst.Count()).Where(t => isBurst[t]).ToList();

            for (int x = 0; x < fieldSize.Width; x++)
            {
                for (int y = 0; y < fieldSize.Height; y++)
                {
                    Card card = game.field[x, y].card;
                    if (card != null && burstNum.Contains(game.field[x, y].group))
                    {
                        Size mycardSize = new Size(game.cardSize.Width, game.cardSize.Height);
                        if (card.turn % 2 == 1) mycardSize = new Size(mycardSize.Height, mycardSize.Width);

                        DX.DrawBox(GridStart.X + (int)(grid_len * x), GridStart.Y + (int)(grid_len * y),
                            GridStart.X + (int)(grid_len * (x + mycardSize.Width)), GridStart.Y + (int)(grid_len * (y + mycardSize.Height)),
                            DX.GetColor(255, 0, 0), 1);
                    }
                }
            }
        }

        //手札描画
        public int DrawHandCard(GameManager game, int hand_cur, Point mouse)
        {
            int cardOnMouseNum = -1;
            bool is1P = game.is1P;
            List<Card> card_1p = game.handCard_1p;
            List<Card> card_2p = game.handCard_2p;

            int handCard_width = GridStart.X / game.handCardNum;
            Size handCard_size = new Size(handCard_width, handCard_width / 2 * 3);

            Point HandCardStart_1P = new Point((int)(GridStart.X + grid_len * fieldSize.Width) + 1, (int)(GridStart.Y + grid_len * fieldSize.Height - handCard_size.Height));
            for (int i = 0; i < card_1p.Count(); i++)
            {
                int x = HandCardStart_1P.X + handCard_size.Width * i;
                int y=HandCardStart_1P.Y;

                if (is1P && mouse.X >= x && mouse.X < x + handCard_size.Width && mouse.Y >= y && mouse.Y < y + handCard_size.Height) cardOnMouseNum = i;
                if (is1P && i == hand_cur) continue;
                DrawCard(x, y, card_1p[i], handCard_size, available: game.handCard_Available[0, game.handCard_1p[i].handCardID]);
            }

            Point HandCardStart_2P = new Point(0, (int)grid_len);
            for (int i = 0; i < card_2p.Count(); i++)
            {
                int x = HandCardStart_2P.X + handCard_size.Width * i;
                int y = HandCardStart_2P.Y;
                if (!is1P && mouse.X >= x && mouse.X < x + handCard_size.Width && mouse.Y >= y && mouse.Y < y + handCard_size.Height) cardOnMouseNum = i;
                if (!is1P && i == hand_cur) continue;
                DrawCard(x, y, card_2p[i], handCard_size, available: game.handCard_Available[1, game.handCard_2p[i].handCardID]);
            }

            return cardOnMouseNum;
        }

        //移動中のカード描画
        public Point DrawMovingCard(GameManager game, int hand_cur, Point mouse)
        {
            bool is1P = game.is1P;
            Card card = (is1P ? game.handCard_1p : game.handCard_2p)[hand_cur];
            int x = 0, y = 0, fix_x = 0, fix_y = 0;
            Point fieldPt = new Point(-1, -1);

            //カードの中心座標を取得
            x = mouse.X - cardScale.Width / 2;
            y = mouse.Y - cardScale.Height / 2;
            fix_x = cardSize.Width - 1;
            fix_y = cardSize.Height - 1;
            if (card.turn % 2 == 1)
            {
                x = mouse.X - cardScale.Height / 2;
                y = mouse.Y - cardScale.Width / 2;
                fix_x = cardSize.Height - 1;
                fix_y = cardSize.Width - 1;
            }

            //グリッドにフィットさせる
            if (x >= GridStart.X && y >= GridStart.Y &&
                x < GridStart.X + grid_len * (fieldSize.Width - fix_x) && y < GridStart.Y + grid_len * (fieldSize.Height - fix_y))
            {
                fieldPt = new Point((int)((x - GridStart.X) / grid_len), (int)((y - GridStart.Y) / grid_len));
                x = fieldPt.X * (int)grid_len + GridStart.X;
                y = fieldPt.Y * (int)grid_len + GridStart.Y;
            }
            DrawCard(x, y, card);
            return fieldPt;

        }

        //フィールド上のカード描画
        public void DrawFieldCard(GameManager game)
        {
            for (int x = 0; x < fieldSize.Width; x++)
            {
                for (int y = 0; y < fieldSize.Height; y++)
                {
                    if (game.field[x, y].card != null)
                    {
                        DrawCard((int)(GridStart.X + grid_len * x), (int)(GridStart.Y + grid_len * y), game.field[x, y].card);
                    }
                    
                    //デバッグ用描画処理
                    /*
                    
                    if (game.field[x, y].group != -1)
                    {

                        DX.DrawBox((int)(GridStart.X + grid_len * x), (int)(GridStart.Y + grid_len * y),
                            (int)(GridStart.X + grid_len * (x + 1)), (int)(GridStart.Y + grid_len * (y + 1)),
                            game.field[x, y].group==0?DX.GetColor(0, 0, 0): DX.GetColor(255, 255, 255), 1);
                    }
                    if (game.field[x, y].connector != -1)
                    {
                        DX.DrawBox((int)(GridStart.X + grid_len * x), (int)(GridStart.Y + grid_len * y),
                            (int)(GridStart.X + grid_len * (x + 1)), (int)(GridStart.Y + grid_len * (y + 1)),
                            elementColDict[game.field[x, y].connector], 1);
                    }*/
                }
            }
        }

        //タイトル描画
        public void DrawTitle(bool isCPU)
        {
            int defaultSize = DX.GetFontSize();
            DX.SetFontSize(64);
            string title = "Gatherion";
            int title_len = DX.GetDrawStringWidth(title, title.Length);

            DX.DrawString(screenSize.Width / 2 - title_len / 2, screenSize.Height / 2 - 32, title, DX.GetColor(255,255,255));

            //CPU
            DX.SetFontSize(32);
            string CPUstr = "player vs player";
            if (isCPU) CPUstr = "player vs CPU";
            int cpu_len = DX.GetDrawStringWidth(CPUstr, CPUstr.Length);
            DX.DrawString(screenSize.Width / 2 - cpu_len / 2, screenSize.Height / 4 * 3 - 16, CPUstr, DX.GetColor(255,255,255));

            DX.SetFontSize(defaultSize);

        }

        //設定描画
        int DeckIndex_1P=0;
        int DeckIndex_2P=0;
        public void DrawSetting(Point mouse, int wheel, ref string deck_1p, ref string deck_2p)
        {
            List<string> deckList = new List<string>() {"ランダム" };
            deckList.AddRange(Card.deckList.Keys);
            int fontSize = 32;
            int defaultFontSize = DX.GetFontSize();
            DX.SetFontSize(fontSize);
            Point settingStart = new Point(fieldSize.Width / 3, fieldSize.Height / 3);
            DX.DrawString(settingStart.X, settingStart.Y, "1Pデッキ", DX.GetColor(255, 255, 255));
            DX.DrawString(settingStart.X, settingStart.Y + fontSize * 2, "2Pデッキ", DX.GetColor(255, 255, 255));

            Point deck_1p_start = new Point(settingStart.X, settingStart.Y + fontSize);
            Point deck_2p_start = new Point(settingStart.X, settingStart.Y + fontSize * 3);
            deck_1p = deckList[DeckIndex_1P];
            deck_2p = deckList[DeckIndex_2P];
            Size deck_1p_size = new Size(DX.GetDrawStringWidth(deck_1p, deck_1p.Length), fontSize);
            Size deck_2p_size = new Size(DX.GetDrawStringWidth(deck_2p, deck_2p.Length), fontSize);

            //move index
            if (mouse.X >= deck_1p_start.X && mouse.X < deck_1p_start.X + deck_1p_size.Width &&
                mouse.Y >= deck_1p_start.Y && mouse.Y < deck_1p_start.Y + deck_1p_size.Height)
            {
                if (wheel > 0) DeckIndex_1P = (DeckIndex_1P + 1) % deckList.Count();
                if (wheel < 0) DeckIndex_1P = (DeckIndex_1P + deckList.Count() - 1) % deckList.Count();
            }
            if (mouse.X >= deck_2p_start.X && mouse.X < deck_2p_start.X + deck_2p_size.Width &&
                mouse.Y >= deck_2p_start.Y && mouse.Y < deck_2p_start.Y + deck_2p_size.Height)
            {
                if (wheel > 0) DeckIndex_2P = (DeckIndex_2P + 1) % deckList.Count();
                if (wheel < 0) DeckIndex_2P = (DeckIndex_2P + deckList.Count() - 1) % deckList.Count();
            }
            DX.DrawString(deck_1p_start.X, deck_1p_start.Y, deck_1p, DX.GetColor(255, 0, 255));
            DX.DrawString(deck_2p_start.X, deck_2p_start.Y, deck_2p, DX.GetColor(255, 0, 255));

            if (deck_1p == "ランダム")
                deck_1p = "";
            if (deck_2p == "ランダム")
                deck_2p = "";

            DX.SetFontSize(defaultFontSize);

        }

        //カード描画
        public void DrawCard(int x, int y, Card card, Size cardScale = new Size(), bool available = true)
        {
            List<int> elems = card.elems;
            int turn = card.turn;

            if (cardScale == new Size()) cardScale = new Size(this.cardScale.Width + 1, this.cardScale.Height + 1);
            Size cardScaleOrg = new Size((Point)cardScale);
            if (turn % 2 == 1) cardScale = new Size(cardScale.Height, cardScale.Width);

            uint col = DX.GetColor(128, 128, 128);
            if (card.imgPath == "")
                DX.DrawBox(x, y, x + cardScale.Width, y + cardScale.Height, col, 1);
            else
            {
                int myx = x;
                int myy = y;
                if (turn >= 2 && turn <= 3) myy += cardScale.Height;
                if (turn >= 1 && turn <= 2) myx += cardScale.Width;
                int h, w;
                DX.GetGraphSize(Card.CardGraphDic[card.imgPath], out w, out h);
                DX.DrawRotaGraph3(myx, myy, 0, 0, (double)cardScaleOrg.Width / w, (double)cardScaleOrg.Height / h, turn * Math.PI / 2.0, Card.CardGraphDic[card.imgPath], 0);
            }
            DX.DrawBox(x, y, x + cardScale.Width, y + cardScale.Height, DX.GetColor(256, 256, 256), 0);

            int defaultFontSize = DX.GetFontSize();
            int myFontSize = (int)Math.Sqrt(cardScale.Width * cardScale.Height / 32);
            DX.SetFontSize(myFontSize);

            for (int i = 0; i < elems.Count(); i++)
            {
                if (elems[i] == -1) continue;

                int sx = 0, sy = 0;
                int strwidth = DX.GetDrawStringWidth(elementDict[elems[i]], elementDict[elems[i]].Length);
                switch ((i + turn) % 4)
                {
                    case 0:
                        sx = x + cardScale.Width / 2 - strwidth / 2;
                        sy = y;
                        break;
                    case 1:
                        sx = x + cardScale.Width - strwidth;
                        sy = y + cardScale.Height / 2 - strwidth / 2;
                        break;
                    case 2:
                        sx = x + cardScale.Width / 2 - strwidth / 2;
                        sy = y + cardScale.Height - strwidth;
                        break;
                    case 3:
                        sx = x;
                        sy = y + cardScale.Height / 2 - strwidth / 2;
                        break;
                }

                int fontSize = DX.GetFontSize();
                DX.DrawBox(sx, sy, sx + fontSize + 1, sy + fontSize + 1, DX.GetColor(255, 255, 255) - elementColDict[elems[i]], 0);
                DX.DrawBox(sx, sy, sx + fontSize, sy + fontSize, elementColDict[elems[i]], 1);
                DX.DrawString(sx + 1, sy + 1, elementDict[elems[i]], DX.GetColor(0, 0, 0));
                DX.DrawString(sx, sy, elementDict[elems[i]], DX.GetColor(255, 255, 255));
            }
            DX.SetFontSize(defaultFontSize);

            if (!available)
            {
                DX.DrawQuadrangle(x, y, x + (int)grid_len, y, x + cardScale.Width, y + cardScale.Height, x + cardScale.Width - (int)grid_len, y + cardScale.Height, DX.GetColor(255, 0, 0), 1);
                DX.DrawQuadrangle(x + cardScale.Width - (int)grid_len, y, x + cardScale.Width, y, x + (int)grid_len, y + cardScale.Height, x, y + cardScale.Height, DX.GetColor(255, 0, 0), 1);
            }
        }
    }
}
