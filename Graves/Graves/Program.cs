using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;
using System.Drawing;


namespace Graves
{
    class Program
    {
        public static Spell.Skillshot Q;
        public static Spell.Skillshot W;
        public static Spell.Skillshot E;
        public static Spell.Skillshot R;
        private static Vector3 mousePos { get { return Game.CursorPos; } }

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }
        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        public static Menu menu, ComboMenu, DrawingsMenu, FarmMenu;

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            Bootstrap.Init(null);

            Q = new Spell.Skillshot(SpellSlot.Q, (int)720f, SkillShotType.Linear, (int)0.25f, (int)2000f, (int)(15f * (float)Math.PI / 180));
            W = new Spell.Skillshot(SpellSlot.W, (int)850f, SkillShotType.Circular, (int)0.25f, (int)1650f, (int)250f);
            E = new Spell.Skillshot(SpellSlot.E, (uint)_Player.GetAutoAttackRange(), SkillShotType.Circular); // Fluxy Vayne Q modify
            R = new Spell.Skillshot(SpellSlot.R, (int)1100f, SkillShotType.Linear, (int)0.25f, (int)2100f, (int)100f);

            menu = MainMenu.AddMenu("Graves", "Graves");
            menu.AddGroupLabel("First plugin I made! Credits to VnHarry and Fluxy");

            ComboMenu = menu.AddSubMenu("Combo", "Combo");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.AddSeparator();
            ComboMenu.Add("useQCombo", new CheckBox("Use Q"));
            ComboMenu.Add("useWCombo", new CheckBox("Use W"));
            ComboMenu.Add("useECombo", new CheckBox("Use E"));
            ComboMenu.Add("useRCombo", new CheckBox("Use R"));

            DrawingsMenu = menu.AddSubMenu("Drawings", "Drawings");
            DrawingsMenu.Add("DrawQ", new CheckBox("Draw Q"));
            DrawingsMenu.Add("DrawW", new CheckBox("Draw W"));
            DrawingsMenu.Add("DrawE", new CheckBox("Draw E"));
            DrawingsMenu.Add("DrawR", new CheckBox("Draw R"));

            FarmMenu = menu.AddSubMenu("Farm", "Farm");
            FarmMenu.AddGroupLabel("Farm Settings");
            FarmMenu.AddSeparator();
            FarmMenu.AddGroupLabel("LaneClear");
            FarmMenu.Add("useQ", new CheckBox("Use Q"));

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (DrawingsMenu["DrawQ"].Cast<CheckBox>().CurrentValue)
            {
                Drawing.DrawCircle(_Player.Position, Q.Range, System.Drawing.Color.Crimson);
            }

            if (DrawingsMenu["DrawW"].Cast<CheckBox>().CurrentValue)
            {
                Drawing.DrawCircle(_Player.Position, W.Range, System.Drawing.Color.Crimson);
            }

            if (DrawingsMenu["DrawE"].Cast<CheckBox>().CurrentValue)
            {
                Drawing.DrawCircle(_Player.Position, E.Range, System.Drawing.Color.Crimson);
            }

            if (DrawingsMenu["DrawR"].Cast<CheckBox>().CurrentValue)
            {
                Drawing.DrawCircle(_Player.Position, R.Range, System.Drawing.Color.Crimson);
            }

        }

        private static void Game_OnTick(EventArgs args)
        {
            Orbwalker.ForcedTarget = null;
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo)) Combo();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear)) LaneClear();
        }

        public static float RDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Physical,
                (float)(new[] { 250, 400, 550 }[Program.R.Level] + 1.5 * _Player.FlatPhysicalDamageMod));
        }
        public static void Combo()
        {
            var useQ = ComboMenu["useQCombo"].Cast<CheckBox>().CurrentValue;
            var useW = ComboMenu["useWCombo"].Cast<CheckBox>().CurrentValue;
            var useE = ComboMenu["useECombo"].Cast<CheckBox>().CurrentValue;
            var useR = ComboMenu["useRCombo"].Cast<CheckBox>().CurrentValue;

            foreach (var target in HeroManager.Enemies.Where(o => o.IsValidTarget(1200) && !o.IsDead && !o.IsZombie))
            {
                if (useQ && Q.IsReady() && Q.GetPrediction(target).HitChance >= HitChance.Medium)
                {
                    Q.Cast(target);
                }
                if (useW && W.IsReady() && W.GetPrediction(target).HitChance >= HitChance.Medium && target.IsValidTarget(W.Range))
                {
                    W.Cast(target);
                }
                if (useE && E.IsReady() && target.IsValidTarget(700))
                {
                    E.Cast(mousePos);
                }
                if (useR && R.IsReady() && R.GetPrediction(target).HitChance >= HitChance.High && target.Health <= RDamage(target) && target.IsValidTarget(R.Range))
                {
                    R.Cast(target);
                }
            }
        }
        public static void LaneClear()
        {
            var minions = ObjectManager.Get<Obj_AI_Minion>().Where(m => m.IsValidTarget(Q.Range));
            var useQ = FarmMenu["useQ"].Cast<CheckBox>().CurrentValue;
            if (useQ && Q.IsReady() && Q.IsInRange(minions.FirstOrDefault().Position) && Q.GetPrediction(minions.FirstOrDefault()).HitChance >= HitChance.Medium)
            {
                Q.Cast(minions.FirstOrDefault().Position);
            }
        }
    }
}

