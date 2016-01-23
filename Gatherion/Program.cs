using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DxLibDLL;
using System.Drawing;

namespace Gatherion
{
    class Program
    {
        static int getMouse(out Point mouse)
        {
            int x,y,m;
            if (DX.GetMouseInputLog(out m, out x, out y) == 0)
            {
                mouse = new Point(x, y);
                return m;
            }
            mouse = new Point();
            return 0;
        }

        static bool clickedLeft(ref int mouse_state)
        {
            if ((mouse_state & DX.MOUSE_INPUT_LEFT) != 0)
            {
                mouse_state = 0;
                return true;
            }
            return false;
        }

        static bool clickedRight(ref int mouse_state)
        {
            if ((mouse_state & DX.MOUSE_INPUT_RIGHT) != 0)
            {
                mouse_state = 0;
                return true;
            }
            return false;
        }

        static void waitAndUpdate(Draw draw, GameManager game, int wait, int state)
        {
            DX.ClearDrawScreen();
            if (state >= 0)
            {
                draw.DrawGrid();
                draw.DrawFieldCard(game);
                draw.DrawHandCard(game, -1, new Point());
                draw.DrawState(game);
            }
            DX.ScreenFlip();
            DX.WaitVSync(wait);
        }

        static void Main(string[] args)
        {
            Size fieldSize = new Size(18, 24);
            Size cardSize = new Size(4, 6);
            int deckNum = 15;
            int handCardNum = 3;
            int wait = 60;
            bool isCPU = false;
            Draw draw= new Draw(fieldSize, cardSize);
            GameManager game = new GameManager(deckNum, handCardNum, fieldSize, cardSize);
            CPU cpu = new CPU(0);

            DX.ChangeWindowMode(1);
            DX.DxLib_Init();
            DX.SetGraphMode(1280, 720, 32);
            DX.SetDrawScreen(DX.DX_SCREEN_BACK);

            //山札のロード
            Card.LoadDeck();

            //ゲームの状態
            int state = -1;
            //移動中の手札の番号
            int moving_hand_cur = -1;
            //前の人が詰んでいたか
            bool isAlreadyCheckmate = false;
            int mouse_state = 0;
            int wheel;
            string deck_1P = "", deck_2P = "";
            //置ける場所の候補
            List<Card> candidates = new List<Card>();

            while (DX.ScreenFlip() == 0 && DX.ProcessMessage() == 0 && DX.ClearDrawScreen() == 0)
            {
                Point mousePoint;
                int hand_cur = -1;
                mouse_state = getMouse(out mousePoint);
                if(mousePoint==new Point())
                {
                    int mx = 0, my = 0;
                    DX.GetMousePoint(out mx, out my);
                    mousePoint = new Point(mx, my);
                }

                if (state >= 0)
                {
                    draw.DrawGrid();
                    draw.DrawFieldCard(game);
                    draw.DrawAssist(game, candidates, moving_hand_cur);
                    hand_cur = draw.DrawHandCard(game, moving_hand_cur, mousePoint);
                }

                switch (state)
                {
                    case -2://設定
                        wheel = DX.GetMouseWheelRotVol();
                        draw.DrawSetting(mousePoint, wheel, ref deck_1P, ref deck_2P);
                        if (clickedLeft(ref mouse_state))
                            state = -1;
                        break;
                    case -1://タイトル
                        wheel = DX.GetMouseWheelRotVol();
                        if (wheel != 0) isCPU = !isCPU;
                        draw.DrawTitle(isCPU);
                        if (clickedLeft(ref mouse_state))
                            state = 0;
                        if (clickedRight(ref mouse_state))
                            state = -2;
                        break;
                    case 0://初期化
                        draw = new Draw(fieldSize, cardSize);
                        game = new GameManager(deckNum, handCardNum, fieldSize, cardSize, deck_1P, deck_2P);
                        if (isCPU) cpu = new CPU(1);
                        //置ける場所の候補取得
                        candidates = Field.getCandidates(game);
                        state = 1;
                        break;
                    case 1://手札選択
                        if (isCPU && game.now_Player == cpu.me)
                        {
                            state = 5;
                        }
                        if (clickedLeft(ref mouse_state)&&hand_cur!=-1)
                        {
                            moving_hand_cur = hand_cur;
                            state = 2;
                        }
                        break;
                    case 2://手札移動
                        Point fieldPt = draw.DrawMovingCard(game, moving_hand_cur, mousePoint);
                        bool clicked = clickedLeft(ref mouse_state);

                        //回転
                        wheel = DX.GetMouseWheelRotVol();
                        if (game.is1P)
                        {
                            if (wheel > 0) game.handCard_1p[moving_hand_cur].turn = (game.handCard_1p[moving_hand_cur].turn + 1) % 4;
                            if (wheel < 0) game.handCard_1p[moving_hand_cur].turn = (game.handCard_1p[moving_hand_cur].turn + 3) % 4;
                        }
                        else
                        {
                            if (wheel > 0) game.handCard_2p[moving_hand_cur].turn = (game.handCard_2p[moving_hand_cur].turn + 1) % 4;
                            if (wheel < 0) game.handCard_2p[moving_hand_cur].turn = (game.handCard_2p[moving_hand_cur].turn + 3) % 4;
                        }

                        //フィールドに配置
                        if (clicked && fieldPt != new Point(-1,-1))
                        {
                            if (!game.handToField(fieldPt, moving_hand_cur))
                                break;
                            moving_hand_cur = -1;

                            //勝利判定
                            if (game.card_1p.Count() == 0 && game.handCard_1p.Count() == 0 ||
                                game.card_2p.Count() == 0 && game.handCard_2p.Count() == 0)
                            {
                                state = 4;
                                break;
                            }

                            //手番交代
                            waitAndUpdate(draw, game, wait, state);
                            game.next();

                            state = 3;
                            break;
                        }
                        //移動をやめる
                        if (clicked && hand_cur != -1)
                        {
                            moving_hand_cur = -1;
                            state = 1;
                        }
                        break;
                    case 3://詰み・バーストチェック

                        //バースト
                        bool[] isBurst = new bool[] { false, false };
                        for (int i = 0; i < 2; i++)
                        {
                            if (Field.isBurst(game, cardSize, i))
                            {
                                isBurst[i] = true;
                            }
                        }
                        if (isBurst.Count(t => t) != 0)
                        {
                            waitAndUpdate(draw, game, wait, state);
                        }
                        for (int i = 0; i < 2; i++)
                        {
                            if (isBurst[i])
                            {
                                game.burst(i);
                            }
                        }
                        if (isBurst.Count(t => t == true) != 0)
                        {
                            //手番を戻す
                            game.next();
                            game.insertInfo("バースト");
                            state = 1;
                        }

                        //ドロー
                        game.draw();
                        //詰み
                        candidates = Field.getCandidates(game);
                        if (candidates.Count()==0)
                        {
                            if (isAlreadyCheckmate)
                            {
                                game.insertInfo("両詰み");
                                waitAndUpdate(draw, game, wait, state);
                                isAlreadyCheckmate = false;
                                game.clearField();
                                candidates = Field.getCandidates(game);
                                state = 1;
                            }
                            else
                            {
                                game.insertInfo("詰み");
                                isAlreadyCheckmate = true;
                                //手番を戻す
                                waitAndUpdate(draw, game, wait, state);
                                game.next();
                                state = 3;
                            }
                        }
                        else
                        {
                            isAlreadyCheckmate = false;
                            state = 1;
                        }
                        break;
                    case 4://勝利
                        draw.DrawWinnerMessage((game.is1P ? "1" : "2") + "P WIN!!!!");
                        if (clickedLeft(ref mouse_state))
                        {
                            state = -1;
                        }
                        break;
                    case 5://CPU選択

                        var cpu_res = cpu.choice(game);
                        if (cpu.me == 0)
                            game.handCard_1p[cpu_res.card.handCardID].turn = cpu_res.card.turn;
                        else
                            game.handCard_2p[cpu_res.card.handCardID].turn = cpu_res.card.turn;
                        game.handToField(cpu_res.card.point, cpu_res.card.handCardID);

                        //勝利判定
                        if (game.card_1p.Count() == 0 && game.handCard_1p.Count() == 0 ||
                            game.card_2p.Count() == 0 && game.handCard_2p.Count() == 0)
                        {
                            state = 4;
                            break;
                        }

                        //手番交代
                        waitAndUpdate(draw, game, wait, state);
                        game.next();

                        state = 3;
                        break;

                }

                if (state >= 0)
                    draw.DrawState(game);
            }

            DX.DxLib_End();
        }
    }
}
