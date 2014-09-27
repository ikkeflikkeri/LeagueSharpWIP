using LeagueSharp;
using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyCassiopeia
{
    class Cassiopeia : Champion
    {
        public Cassiopeia() : base("Cassiopeia")
        {

        }

        protected override void InitializeSkins(ref SkinManager Skins)
        {
            Skins.Add("Cassiopeia");
            Skins.Add("Desperada Cassiopeia");
            Skins.Add("Siren Cassiopeia");
            Skins.Add("Mythic Cassiopeia");
            Skins.Add("Jade Fang Cassiopeia");
        }

        protected override void InitializeSpells()
        {
            Spell Q = new Spell(SpellSlot.Q, 850);
            Q.SetSkillshot(0.65f, 75f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            Spell W = new Spell(SpellSlot.W, 850);
            W.SetSkillshot(0.65f, 75f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            Spell E = new Spell(SpellSlot.E, 700);

            Spell R = new Spell(SpellSlot.R, 825);

            Spells.Add("Q", Q);
            Spells.Add("W", W);
            Spells.Add("E", E);
            Spells.Add("R", R);
        }

        protected override void CreateMenu()
        {
            Menu.AddSubMenu(new Menu("Combo", "Combo"));
            Menu.SubMenu("Combo").AddItem(new MenuItem("Combo_q", "Use Q").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("Combo_w", "Use W").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("Combo_e", "Use E").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("Combo_r", "Use R").SetValue(true));

            Menu.AddSubMenu(new Menu("Harass", "Harass"));
            Menu.SubMenu("Harass").AddItem(new MenuItem("Harass_q", "Use Q").SetValue(true));
            Menu.SubMenu("Harass").AddItem(new MenuItem("Harass_w", "Use W").SetValue(false));
            Menu.SubMenu("Harass").AddItem(new MenuItem("Harass_e", "Use E").SetValue(true));

            Menu.AddSubMenu(new Menu("Auto", "Auto"));
            Menu.SubMenu("Auto").AddItem(new MenuItem("Auto_q", "Use Q").SetValue(false));
            Menu.SubMenu("Auto").AddItem(new MenuItem("Auto_w", "Use W").SetValue(false));
            Menu.SubMenu("Auto").AddItem(new MenuItem("Auto_e", "Use E").SetValue(false));
            Menu.SubMenu("Auto").AddItem(new MenuItem("Auto_executee", "Use E to execute poisoned minions").SetValue(true));

            Menu.AddSubMenu(new Menu("Drawing", "Drawing"));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("Drawing_q", "Q Range").SetValue(new Circle(true, Color.FromArgb(100, 0, 255, 0))));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("Drawing_w", "W Range").SetValue(new Circle(true, Color.FromArgb(100, 0, 255, 0))));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("Drawing_e", "E Range").SetValue(new Circle(true, Color.FromArgb(100, 0, 255, 0))));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("Drawing_r", "R Range").SetValue(new Circle(true, Color.FromArgb(100, 0, 255, 0))));
        }

        protected override void Combo()
        {
            if (Menu.Item("Combo_q").GetValue<bool>()) Cast("Q", SimpleTs.DamageType.Magical, true);
            if (Menu.Item("Combo_w").GetValue<bool>()) CastW();
            if (Menu.Item("Combo_e").GetValue<bool>()) CastE();
        }
        protected override void Harass()
        {
            if (Menu.Item("Harass_q").GetValue<bool>()) Cast("Q", SimpleTs.DamageType.Magical, true);
            if (Menu.Item("Harass_w").GetValue<bool>()) CastW();
            if (Menu.Item("Harass_e").GetValue<bool>()) CastE();
        }
        protected override void LaneClear()
        {
            if (Spells["E"].IsReady())
            {
                Obj_AI_Base minion = MinionManager.GetMinions(Player.Position, Spells["E"].Range).Where(min => HealthPrediction.GetHealthPrediction(min, 500) < (DamageLib.getDmg(min, DamageLib.SpellType.E) * 0.7f) && min.HasBuffOfType(BuffType.Poison)).FirstOrDefault();
                if (minion != null) Spells["E"].CastOnUnit(minion, false);
            }
            
            if (Spells["Q"].IsReady())
            {
                var minions = MinionManager.GetMinions(Player.Position, Spells["Q"].Range).Where(min => !min.HasBuffOfType(BuffType.Poison)).ToArray();
                if (minions.Length > 1)
                {
                    var position = MinionManager.GetBestCircularFarmLocation(minions.Select(minion => minion.ServerPosition.To2D()).ToList(), Spells["Q"].Width * 2, Spells["Q"].Range);
                    Spells["Q"].Cast(position.Position, true);
                }
            }
            
            if (Spells["W"].IsReady())
            {
                var minions = MinionManager.GetMinions(Player.Position, Spells["W"].Range).Where(min => !min.HasBuffOfType(BuffType.Poison)).ToArray();
                if (minions.Length > 1)
                {
                    var position = MinionManager.GetBestCircularFarmLocation(minions.Select(minion => minion.ServerPosition.To2D()).ToList(), Spells["W"].Width * 2, Spells["W"].Range);
                    Spells["W"].Cast(position.Position, true);
                }
            }
        }
        protected override void Auto()
        {
            if (Menu.Item("Auto_q").GetValue<bool>()) Cast("Q", SimpleTs.DamageType.Magical, true);
            if (Menu.Item("Auto_w").GetValue<bool>()) CastW();
            if (Menu.Item("Auto_e").GetValue<bool>()) CastE();

            if (Menu.Item("Auto_executee").GetValue<bool>() && Spells["E"].IsReady())
            {
                Obj_AI_Base minion = MinionManager.GetMinions(Player.Position, Spells["E"].Range).Where(min => HealthPrediction.GetHealthPrediction(min, 500) < (DamageLib.getDmg(min, DamageLib.SpellType.E) * 0.7f) && min.HasBuffOfType(BuffType.Poison)).FirstOrDefault();
                if (minion != null) Spells["E"].CastOnUnit(minion, false);
            }
        }

        protected override void Update()
        {
        }

        protected override void Drawing()
        {
            DrawCircle("Drawing_q", "Q");
            DrawCircle("Drawing_w", "W");
            DrawCircle("Drawing_e", "E");
            DrawCircle("Drawing_r", "R");
        }

        private void CastW()
        {
            if (!Spells["W"].IsReady()) return;

            Obj_AI_Hero target = SimpleTs.GetTarget(Spells["W"].Range, SimpleTs.DamageType.Magical);
            if (target == null || (target.HasBuffOfType(BuffType.Poison) && Spells["W"].GetPrediction(target).CastPosition.To2D().Distance(Player) < Spells["W"].Range - 200)) return;

            if (target.IsValidTarget(Spells["W"].Range) && Spells["W"].GetPrediction(target).Hitchance >= HitChance.High)
                Spells["W"].Cast(target, true);
        }
        private void CastE()
        {
            if (!Spells["E"].IsReady()) return;

            Obj_AI_Hero target = SimpleTs.GetTarget(Spells["E"].Range, SimpleTs.DamageType.Magical);
            if (target == null || !target.HasBuffOfType(BuffType.Poison)) return;

            Spells["E"].CastOnUnit(target, false);
        }
    }
}
