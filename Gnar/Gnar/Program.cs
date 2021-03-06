﻿using System;
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
using EloBuddy.SDK.Rendering;
using SharpDX;


namespace Gnar
{
    class Program
    {
        public static Spell.Skillshot Q;
        public static Spell.Skillshot Q2;
        public static Spell.Skillshot E;
        public static Spell.Skillshot E2;
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

        public static string Model = ObjectManager.Player.BaseSkinName; 

        public static bool IsMiniGnar()
        {
            return _Player.BaseSkinName == "Gnar";
        }

        public static bool IsMegaGnar()
        {
            return _Player.BaseSkinName == "gnarbig";
        }

        private static Vector3 mousePos { get { return Game.CursorPos; } }

        public static Menu menu, ComboMenu, HarassMenu, LastHitMenu, LaneClearMenu, MiscMenu;

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            Bootstrap.Init(null);

            Q = new Spell.Skillshot(SpellSlot.Q, 1125, SkillShotType.Linear,250,1200,60);
            Q2 = new Spell.Skillshot(SpellSlot.Q, 1125, SkillShotType.Linear, 250, 1200, 80);
            WB = new Spell.Skillshot(SpellSlot.W, 475, SkillShotType.Linear);
            E = new Spell.Skillshot(SpellSlot.E, 475, SkillShotType.Circular,50,1000,150);
            E2 = new Spell.Skillshot(SpellSlot.E, 475, SkillShotType.Circular, 50, 1000, 200);
            R = new Spell.Skillshot(SpellSlot.R, 490,SkillShotType.Circular);

            menu = MainMenu.AddMenu("Gnar", "Gnar");
            // Combo
            ComboMenu = menu.AddSubMenu("Combo", "Combo");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.AddSeparator();
            ComboMenu.Add("useQCombo", new CheckBox("Use Q"));
            ComboMenu.Add("useQ2Combo", new CheckBox("Use Q Big"));
            ComboMenu.Add("useECombo", new CheckBox("Use E"));
            ComboMenu.Add("useE2Combo", new CheckBox("Use E Big"));
            ComboMenu.Add("useWBCombo", new CheckBox("Use W"));
            ComboMenu.Add("useRCombo", new CheckBox("Use R"));
            ComboMenu.Add("useRComboSlider", new Slider("Subtract Ult Push by: ", 20, 0, 100));
            // Harass 
            HarassMenu = menu.AddSubMenu("Harass", "Harass");
            HarassMenu.AddGroupLabel("Harass Settings");
            HarassMenu.AddSeparator();
            HarassMenu.Add("useQHarass", new CheckBox("Use Q"));
            // LastHit
            LastHitMenu = menu.AddSubMenu("LastHit", "LastHit");
            LastHitMenu.AddGroupLabel("LastHit Settings");
            LastHitMenu.AddSeparator();
            LastHitMenu.Add("useQLastHit", new CheckBox("Use Q"));
            LastHitMenu.Add("useWLastHit", new CheckBox("Use W"));
            // LaneClear
            LaneClearMenu = menu.AddSubMenu("LaneClear", "LaneClear");
            LaneClearMenu.AddGroupLabel("LaneClear Settings");
            LaneClearMenu.AddSeparator();
            LaneClearMenu.Add("useQLaneClear", new CheckBox("Use Q"));
            LaneClearMenu.Add("useWLaneClear", new CheckBox("Use W"));
            // Misc
            MiscMenu = menu.AddSubMenu("Misc", "Misc");
            MiscMenu.AddGroupLabel("Misc Settings");
            MiscMenu.AddSeparator();
            MiscMenu.Add("useR", new CheckBox("Auto R"));
            MiscMenu.Add("useW", new CheckBox("Auto W to Interrupt"));
            MiscMenu.Add("useQW", new CheckBox("Auto Q if enemy has 2 stacks of W"));
            MiscMenu.Add("R", new Slider("When X enemy is stunnable by R", 2, 5, 0));

            Game.OnTick += Game_OnTick;
            Game.OnUpdate += Game_OnUpdate;
        }

        private static void Game_OnTick(EventArgs args)
        {
            Orbwalker.ForcedTarget = null;
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo)) Combo();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass)) Harass();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit)) LastHit();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear)) LaneClear();
        }

         private static void Game_OnUpdate(EventArgs args)
        {
            if (_Player.CountEnemiesInRange(R.Range) >= Ult.getSliderValue(Program.MiscMenu, "R")) 
             {
                 var useR = MiscMenu["useR"].Cast<CheckBox>().CurrentValue;
                 var useQ = MiscMenu["useQW"].Cast<CheckBox>().CurrentValue;
                 if (useR && R.IsReady())
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

                     if (priorityTarget != null && priorityTarget.IsValid && Ult.IsUltable(priorityTarget) && R.GetPrediction(priorityTarget).HitChance >= HitChance.Medium)
                     {
                         R.Cast(priorityTarget);
                     }
                 }
                 foreach (var target in HeroManager.Enemies.Where(o => o.IsValidTarget(1400) && !o.IsDead && !o.IsZombie))
                 {
                     if (useQ && Q.IsReady() && Q.GetPrediction(target).HitChance >= HitChance.Medium && target.GetBuffCount("gnarwproc") == 2)
                     {
                         Q.Cast(target);
                     }
                 }
             }
        }

         static void OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs args)
         {
             var useW = MiscMenu["useW"].Cast<CheckBox>().CurrentValue;
             if (useW && sender.IsValidTarget(WB.Range))
             {
                 WB.Cast(args.Sender);
             }
         }

        public static void Combo()
        {
            var useQ = ComboMenu["useQCombo"].Cast<CheckBox>().CurrentValue;
            var useQ2 = ComboMenu["useQ2Combo"].Cast<CheckBox>().CurrentValue;
            var useW = ComboMenu["useWBCombo"].Cast<CheckBox>().CurrentValue;
            var useE = ComboMenu["useECombo"].Cast<CheckBox>().CurrentValue;
            var useE2 = ComboMenu["useE2Combo"].Cast<CheckBox>().CurrentValue;
            var useR = ComboMenu["useRCombo"].Cast<CheckBox>().CurrentValue;

            foreach (var target in HeroManager.Enemies.Where(o => o.IsValidTarget(1400) && !o.IsDead && !o.IsZombie))
            {
                if (useQ2 && Q2.IsReady() && IsMegaGnar() && Q2.GetPrediction(target).HitChance >= HitChance.Medium)
                {
                    Q2.Cast(target);
                }
                if (useQ && Q.IsReady() && IsMiniGnar() && Q.GetPrediction(target).HitChance >= HitChance.Medium)
                {
                    Q.Cast(target);
                }
                if (useW && WB.IsReady() && WB.GetPrediction(target).HitChance >= HitChance.Medium && target.IsValidTarget(WB.Range) && !target.HasBuffOfType(BuffType.Stun))
                {
                    WB.Cast(target);
                }
                if (useE && E.IsReady() && target.IsValidTarget(700) && IsMiniGnar())
                {
                    E.Cast(mousePos);
                }
                if (useE2 && E2.IsReady() && target.IsValidTarget(700) && IsMegaGnar())
                {
                    E2.Cast(mousePos);
                }
                if (useR && R.IsReady() && _Player.CountEnemiesInRange(R.Range) >= Ult.getSliderValue(Program.MiscMenu, "R"))
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

                if (priorityTarget != null && priorityTarget.IsValid && Ult.IsUltable(priorityTarget) && !priorityTarget.HasBuffOfType(BuffType.Stun) && R.GetPrediction(priorityTarget).HitChance >= HitChance.Medium)
                    {
                    R.Cast(priorityTarget);
                    }
                }
            }
        }
        public static void Harass()
        {
            var useQ = HarassMenu["useQHarass"].Cast<CheckBox>().CurrentValue;
            foreach (var target in HeroManager.Enemies.Where(o => o.IsValidTarget(1400) && !o.IsDead && !o.IsZombie))
            {
                if (useQ && Q.IsReady() && Q.GetPrediction(target).HitChance >= HitChance.Medium)
                {
                    Q.Cast(target);
                }
            }
        }
        public static void LastHit()
        {
            var useQ = LastHitMenu["useQLastHit"].Cast<CheckBox>().CurrentValue;
            var useW = LastHitMenu["useWLastHit"].Cast<CheckBox>().CurrentValue;
            var minions = ObjectManager.Get<Obj_AI_Minion>().Where(x => x.IsEnemy && x.IsValidTarget(1100)).OrderBy(x => x.Health).FirstOrDefault();
            if (useQ && _Player.GetSpellDamage(minions, SpellSlot.Q) >= minions.Health && !minions.IsDead && Q.GetPrediction(minions).HitChance >= HitChance.Medium)
             {
                 Q.Cast(minions);
             }
             if (useW && _Player.GetSpellDamage(minions, SpellSlot.W) >= minions.Health && !minions.IsDead && WB.GetPrediction(minions).HitChance >= HitChance.Medium)
             {
                 WB.Cast(minions);
             }
        }
        public static void LaneClear()
        {
            var useQ = LastHitMenu["useQLaneClear"].Cast<CheckBox>().CurrentValue;
            var useW = LastHitMenu["useWLaneClear"].Cast<CheckBox>().CurrentValue;
            var minions = ObjectManager.Get<Obj_AI_Minion>().Where(x => x.IsEnemy && x.IsValidTarget(1100)).OrderBy(x => x.Health).FirstOrDefault();
            if (useQ && !minions.IsDead && Q.GetPrediction(minions).HitChance >= HitChance.Medium)
            {
                Q.Cast(minions);
            }
            if (useW && !minions.IsDead && Q.GetPrediction(minions).HitChance >= HitChance.Medium)
            {
                WB.Cast(minions);
            }
        }
    }
}
