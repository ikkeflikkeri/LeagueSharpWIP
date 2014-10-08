﻿using LeagueSharp;
using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EasyKogMaw
{
    abstract class Champion
    {
        protected Obj_AI_Hero Player;
        protected Menu Menu;
        protected Orbwalking.Orbwalker Orbwalker;
        protected SpellManager Spells;

        private int tick = 1000 / 20;
        private int lastTick = Environment.TickCount;
        private string ChampName;
        private SkinManager SkinManager;

        public Champion(string name)
        {
            ChampName = name;

            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;

            if (ChampName.ToLower() != Player.ChampionName.ToLower())
                return;

            SkinManager = new SkinManager();
            Spells = new SpellManager();

            InitializeSpells(ref Spells);
            InitializeSkins(ref SkinManager);

            Menu = new Menu("Easy" + ChampName, "Easy" + ChampName, true);

            SkinManager.AddToMenu(ref Menu);

            Menu.AddSubMenu(new Menu("Target Selector", "Target Selector"));
            SimpleTs.AddToMenu(Menu.SubMenu("Target Selector"));

            Menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(Menu.SubMenu("Orbwalker"));

            InitializeMenu();

            Menu.AddItem(new MenuItem("Recall_block", "Block skills while recalling").SetValue(true));
            Menu.AddToMainMenu();

            Game.OnGameUpdate += Game_OnGameUpdate;
            Game.OnGameEnd += Game_OnGameEnd;
            Drawing.OnDraw += Drawing_OnDraw;

            using (WebClient wc = new WebClient())
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                string amount = wc.UploadString("http://niels-wouters.be/LeagueSharp/playcount.php", "assembly=" + ChampName);
                Game.PrintChat("Easy" + ChampName + " is loaded! This assembly has been played in " + amount + " games.");
            }
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            Draw();
        }

        private void Game_OnGameEnd(GameEndEventArgs args)
        {
            using (WebClient wc = new WebClient())
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                wc.UploadString("http://niels-wouters.be/LeagueSharp/stats.php", "assembly=" + ChampName);
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (Environment.TickCount < lastTick + tick) return;
            lastTick = Environment.TickCount;

            SkinManager.Update();

            Update();

            if ((Menu.Item("Recall_block").GetValue<bool>() && Player.HasBuff("Recall")) || Player.IsWindingUp)
                return;

            bool minionBlock = false;

            foreach (Obj_AI_Minion minion in MinionManager.GetMinions(Player.Position, Player.AttackRange, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.None))
            {
                if (HealthPrediction.GetHealthPrediction(minion, 3000) <= Damage.GetAutoAttackDamage(Player, minion, false))
                    minionBlock = true;
            }

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    if (!minionBlock) Harass();
                    break;
                default:
                    if (!minionBlock) Auto();
                    break;
            }
        }

        protected virtual void InitializeSkins(ref SkinManager Skins) { }
        protected virtual void InitializeSpells(ref SpellManager Spells) { }
        protected virtual void InitializeMenu() { }

        protected virtual void Update() { }
        protected virtual void Draw() { }
        protected virtual void Combo() { }
        protected virtual void Harass() { }
        protected virtual void Auto() { }
    }
}
