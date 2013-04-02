﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Battleship.mbc.accolades
{
    public class Slow : AccoladeProcessor
    {
        int cnt = 0;
        int diff = 0;

        private void ResetCounters()
        {
            cnt = 0;
            diff = 0;
        }

        public override RoundLog.RoundAccolade Process(RoundLog.RoundActivity a)
        {
            if (a.action != RoundLog.RoundAction.ShotAndHit && a.action != RoundLog.RoundAction.ShotAndMiss)
                return RoundLog.RoundAccolade.None;

            if (a.action == RoundLog.RoundAction.ShotAndHit)
            {
                diff++;
                if (diff > BattleshipConfig.GetGlobalDefault().GetConfigValue<int>("accolade_slow_diff", 4))
                    ResetCounters();
            }
            else
            {
                cnt++;
                diff = 0;
            }

            if (cnt > BattleshipConfig.GetGlobalDefault().GetConfigValue<int>("accolade_slow_cnt", 25))
            {
                ResetCounters();
                return RoundLog.RoundAccolade.Slow;
            }

            return RoundLog.RoundAccolade.None;
        }
    }
}
