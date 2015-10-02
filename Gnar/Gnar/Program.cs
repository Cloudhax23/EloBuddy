using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;


namespace Gnar
{
    class Program
    {
        public static Spell.Skillshot Q;
        public static Spell.Skillshot E;
        public static Spell.Skillshot R;
        public static Spell.Skillshot WB;

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }
        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        public static Menu menu, ComboMenu;

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            Bootstrap.Init(null);

            Q = new Spell.Skillshot(SpellSlot.Q, 1100, SkillShotType.Linear);
            WB = new Spell.Skillshot(SpellSlot.W, 475, SkillShotType.Linear);
            E = new Spell.Skillshot(SpellSlot.E, 475, SkillShotType.Circular);
            R = new Spell.Skillshot(SpellSlot.R, 490,SkillShotType.Circular);

            menu = MainMenu.AddMenu("Gnar", "Gnar");

            ComboMenu = menu.AddSubMenu("Combo", "Combo");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.AddSeparator();
            ComboMenu.Add("useQCombo", new CheckBox("Use Q"));
            ComboMenu.Add("useECombo", new CheckBox("Use E"));
            ComboMenu.Add("useWBCombo", new CheckBox("Use W"));
            ComboMenu.Add("useRCombo", new CheckBox("Use R"));
            ComboMenu.Add("useRComboSlider", new Slider("Subtract Ult Push by: ", 20, 0, 100));

            Game.OnTick += Game_OnTick;
        }

        private static void Game_OnTick(EventArgs args)
        {
            Orbwalker.ForcedTarget = null;
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo)) Combo();
        }
        
        public static void Combo()
        {
            var useQ = ComboMenu["useQCombo"].Cast<CheckBox>().CurrentValue;
            var useW = ComboMenu["useWBCombo"].Cast<CheckBox>().CurrentValue;
            var useE = ComboMenu["useECombo"].Cast<CheckBox>().CurrentValue;
            var useR = ComboMenu["useRCombo"].Cast<CheckBox>().CurrentValue;

            foreach (var target in HeroManager.Enemies.Where(o => o.IsValidTarget(1400) && !o.IsDead && !o.IsZombie))
            {
                if (useQ && Q.IsReady() && Q.GetPrediction(target).HitChance >= HitChance.Medium)
                {
                    Q.Cast(target);
                }
                if (useW && WB.IsReady() && WB.GetPrediction(target).HitChance >= HitChance.Medium && target.IsValidTarget(WB.Range))
                {
                    WB.Cast(target);
                }
                if (useE && E.IsReady() && target.IsValidTarget(700))
                {
                    E.Cast(Game.CursorPos);
                }
            if (Ult.isChecked(ComboMenu, "useRCombo") && R.IsReady())
            {
                AIHeroClient priorityTarget = null;
                foreach (var enemy in ObjectManager.Get<AIHeroClient>().Where(a => a.IsEnemy).Where(a => !a.IsDead).Where(a => R.IsInRange(a)))
                {
                    if (priorityTarget == null)
                    {
                        priorityTarget = enemy;
                    }

                    if (!Ult.IsUltable(priorityTarget))
                        return;

                }

                if (priorityTarget != null && priorityTarget.IsValid && Ult.IsUltable(priorityTarget))
                {
                    R.Cast(priorityTarget);
                }
        }
            }
        }
    }
}
