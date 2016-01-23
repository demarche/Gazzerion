using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gatherion
{
    class CPU
    {
        //自分のプレイヤー番号
        public int me = 0;
        Size cardSize;
        //gameの親
        GameManager superGame = null;

        public CPU(int now_Player)
        {
            me = now_Player;
        }

        //手の解
        public class resultSet
        {
            public double score;
            public Card card;
            public List<resultSet> innerResult;
            
            public resultSet() { score = 1.0; card = new Card(); }

        }

        /// <summary>
        /// 再帰で最適解を求める
        /// </summary>
        /// <param name="game"></param>
        /// <param name="nest"></param>
        /// <param name="isMe"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        List<resultSet> getScore(GameManager game, int nest = 0, bool isMe = true, int step = 1)
        {
            List<Card> myHandCard = (game.now_Player == 0 ? game.handCard_1p : game.handCard_2p);
            List<Card> yourHandCard = (game.now_Player == 1 ? game.handCard_1p : game.handCard_2p);
            //手札がない
            if (myHandCard.Count() == 0 && yourHandCard.Count() == 0)
            {
                return new List<resultSet>() { new resultSet(), new resultSet() };
            }

            //手の候補取得
            List<Card> candidates = Field.getCandidates(game);
            
            Field[,] field = game.field;
            Size fieldSize = game.fieldSize;

            //候補のスコア
            List<List<resultSet>> scores = new List<List<resultSet>>();

            int myPatternNum = 0;

            foreach (var card_vi in candidates.Select((v,i)=>new { v,i}))
            {
                Card card = card_vi.v;
                if (nest == 0&&isMe)
                {
                    //パーセンテージを表示
                    Console.Write("{0, 4:f0}%", (double)(card_vi.i + 1) / candidates.Count() * 100);
                    Console.SetCursorPosition(0, Console.CursorTop);
                }
                if (card == null) continue;

                //カードサイズを回転に合わせる
                Size mycardSize = new Size(cardSize.Width, cardSize.Height);
                if (card.turn % 2 == 1) mycardSize = new Size(mycardSize.Height, mycardSize.Width);

                //コネクターグループ設置用
                List<bool> tmpInit = new List<bool>(game.initiation);
                if (!Field.canPut(game.field, card.point, card, mycardSize, game.is1P, game.initiation))
                    throw new Exception("checkmate fatal error");

                //手札を減らす
                List<Card> innerHandCard = new List<Card>(myHandCard);
                innerHandCard[card.handCardID] = null;

                //フィールドに手札を置く
                GameManager innerGame = new GameManager(game,
                     game.now_Player == 0 ? innerHandCard : yourHandCard,
                     game.now_Player == 1 ? innerHandCard : yourHandCard);
                int tmpConnect = Field.fillCard(innerGame.field, card.point, card, mycardSize);
                innerGame.next();

                List<resultSet> cand = new List<resultSet>() { new resultSet(), new resultSet() };
                if (Field.isBurst(innerGame, cardSize, tmpConnect))
                {
                    //バーストチェック
                    int burstNum = Field.Burst(innerGame, cardSize, tmpConnect);
                    if (burstNum >= 3)
                    {
                        //3以上のバーストのみスコア
                        cand[isMe ? 0 : 1].score += (double)(100 + (burstNum - 3) * 10) / (double)(nest + 1);
                        var tmpcand = getScore(innerGame, nest: nest + (isMe ? 0 : 1), isMe: !isMe, step: 1);
                        cand[isMe ? 0 : 1].innerResult = tmpcand;
                    }
                }
                else
                {
                    //バーストしない場合再帰
                    if (!(superGame.initiation.Count(t => t == true) >= 1 && !isMe && nest >= 0))
                        cand = getScore(innerGame, nest: nest + (isMe ? 0 : 1), isMe: !isMe, step: 1);
                }
                //カード情報を候補に追加
                cand[isMe ? 0 : 1].card = new Card(card);
                scores.Add(cand);

                //イニシエーション戻す
                game.initiation = new bool[2] { tmpInit[0], tmpInit[1] };

                myPatternNum++;

            }

            //候補から最善手を予測
            List<resultSet> bestPattern = scores.Count() > 0 ?
                scores.Select(t => new { v = t, score = isMe ? (t[0].score / t[1].score) : (t[1].score / t[0].score) }).Where(t => t.v[isMe ? 0 : 1].innerResult == null
                || t.score < 100 || t.score >= 100 && t.v[isMe ? 0 : 1].innerResult[!isMe ? 0 : 1].score < 100).OrderByDescending(t => t.score).First().v :
                new List<resultSet>() { new resultSet(), new resultSet() };

            //自分の手数を追加
            bestPattern[isMe ? 0 : 1].score += myPatternNum;

            //相手が詰みの場合ボーナス
            if (!isMe && myPatternNum == 0 && yourHandCard.Count() > 0)
            {
                bestPattern[0].score = 100.0 / (nest + 1);
                bestPattern[1].score = 1.0;
            }
            //自分が詰みの場合ペナルティ
            if (isMe && myPatternNum == 0 && myHandCard.Count() > 0)
            {
                bestPattern[0].score = 1.0;
                bestPattern[1].score = 100.0 / (nest + 1);
            }
            /*
            if (nest <= 0)
            {
                string write = "";
                for (int i = 0; i < nest * 2; i++) write += "\t";
                if (!isMe) write += "\t";
                write += string.Format("{0}/{1}", bestPattern[0].score, bestPattern[1].score);
                if (bestPattern[isMe ? 0 : 1].innerResult != null) write += string.Format(" {0}/{1}", bestPattern[isMe ? 0 : 1].innerResult[0].score, bestPattern[isMe ? 0 : 1].innerResult[1].score);
                Console.WriteLine(write);
            }*/

            return bestPattern;
        }

        /// <summary>
        /// 手を選ぶ
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        public resultSet choice(GameManager game)
        {
            cardSize = game.cardSize;
            GameManager newGame = new GameManager(game, game.handCard_1p, game.handCard_2p);
            superGame = game;
            int step = 1;
            //if (game.initiation[0] == true && game.initiation[1] == true) step = 4;
            List<resultSet> bestPat = getScore(newGame, step: step);
            Console.WriteLine(string.Format("{0}/{1}   ", bestPat[0].score, bestPat[1].score));

            return bestPat[0];
        }
    }
}
