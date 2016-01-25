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
        Random rnd = new Random();

        public CPU(int now_Player)
        {
            me = now_Player;
        }

        //手の解
        public class resultSet
        {
            public double score;
            public Card card;
            public resultSet[] innerResult;
            
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
        resultSet[] getScore(GameManager game, int nest = 0, int step = 1)
        {
            //手札がない
            if (Enumerable.Range(0, game.max_Player).Select(player => game.handCard[player].Count()).Count(num => num != 0) == 0)
            {
                return new resultSet[game.max_Player];
            }

            //手の候補取得
            List<Card> candidates = Field.getCandidates(game);
            
            Field[,] field = game.field;
            Size fieldSize = game.fieldSize;

            //候補のスコア
            List<resultSet[]> scores = new List<resultSet[]>();

            int myPatternNum = 0;

            foreach (var card_vi in candidates.Select((v,i)=>new { v,i}))
            {
                Card card = card_vi.v;
                if (nest == 0 && game.now_Player == me)
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
                if (!Field.canPut(game.field, card.point, card, mycardSize, game.now_Player, game.initiation))
                    throw new Exception("checkmate fatal error");

                //手札を減らす
                /*
                List<Card> innerHandCard = new List<Card>(myHandCard);
                var rmIndex = innerHandCard.IndexOf(innerHandCard.Where(t => t != null && t.handCardID == card.handCardID).First());
                innerHandCard[rmIndex] = null;*/
                List<Card>[] innerHandCard = game.handCard.Select(cards => cards.Select(t => t == null ? null : new Card(t)).ToList()).ToArray();
                var rmIndex = innerHandCard[game.now_Player].IndexOf(innerHandCard[game.now_Player].Where(t => t != null && t.handCardID == card.handCardID).First());
                innerHandCard[game.now_Player][rmIndex] = null;

                //フィールドに手札を置く
                GameManager innerGame = new GameManager(game, innerHandCard);
                int tmpConnect = Field.fillCard(innerGame.field, card.point, card, mycardSize);
                innerGame.next();

                resultSet[] cand = new resultSet[game.max_Player];
                for(int i = 0; i < game.max_Player; i++)
                {
                    cand[i] = new resultSet();
                }
                if (Field.isBurst(innerGame, cardSize, tmpConnect))
                {
                    //バーストチェック
                    int burstNum = Field.Burst(innerGame, cardSize, tmpConnect);
                    if (burstNum >= 3)
                    {
                        //3以上のバーストのみスコア
                        cand[game.now_Player].score += (double)(100 + (burstNum - 3) * 10) / (nest / game.max_Player + 1);
                        var tmpcand = getScore(innerGame, nest: nest + 1, step: 1);
                        cand[game.now_Player].innerResult = tmpcand;
                    }
                }
                else
                {
                    //バーストしない場合再帰
                    if (!(superGame.initiation.Count(t => t == true) >= 1 && nest >= 1))
                        cand = getScore(innerGame, nest: nest + 1, step: 1);
                }
                //カード情報を候補に追加
                cand[game.now_Player].card = new Card(card);
                scores.Add(cand);

                //イニシエーション戻す
                game.initiation = new bool[game.max_Player];
                for (int i = 0; i < game.max_Player; i++)
                {
                    game.initiation[i] = tmpInit[i];
                }

                myPatternNum++;

            }

            //候補から最善手を予測
            var noPlayerRange = Enumerable.Range(0, game.max_Player).Where(player => player != game.now_Player);
            resultSet[] bestPattern = new resultSet[game.max_Player];
            if (scores.Count() > 0)
            {
                var scorePair = scores.Select(result => new { v = result, score = (result[game.now_Player].score / noPlayerRange.Select(player => result[player].score).Sum()) });
                var filtered = scorePair.Where(t => t.v[game.now_Player].innerResult == null || t.score < 100 ||
                t.score >= 100 && noPlayerRange.Select(player => t.v[game.now_Player].innerResult[player].score)
                .Count(result => result >= 100.0 / (nest / game.max_Player + 1)) == 0);
                if (filtered.Count() == 0)
                {
                    bestPattern = scorePair.Select(t => new {
                        v = t.v,
                        score = t.score *
                        //スコアに内部スコアを掛ける
                        t.v[game.now_Player].innerResult[game.now_Player].score / noPlayerRange.Select(player => t.v[game.now_Player].innerResult[player].score).Sum()
                    }).OrderByDescending(t => t.score).First().v;
                }
                else
                {
                    bestPattern = filtered.OrderByDescending(t => t.score).First().v;
                }
            }
            else
            {
                for (int i = 0; i < game.max_Player; i++)
                {
                    bestPattern[i] = new resultSet();
                }
            }

            //自分の手数を追加
            bestPattern[game.now_Player].score += myPatternNum;

            //詰みの場合ほかの人がボーナス
            if (nest != 0 && myPatternNum == 0 && game.handCard[game.now_Player].Count() > 0)
            {
                for (int i = 0; i < game.max_Player; i++)
                {
                    if (i == game.now_Player) continue;
                    bestPattern[i].score += 100.0 / ((nest - 1) / game.max_Player + 1);

                }
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
            GameManager newGame = new GameManager(game, game.handCard);
            superGame = game;
            int step = 1;
            //if (game.initiation[0] == true && game.initiation[1] == true) step = 4;
            resultSet[] bestPat = getScore(newGame, step: step);
            Console.WriteLine(string.Join("/", bestPat.Select(t => t.score.ToString())));

            return bestPat[game.now_Player];
        }
    }
}
