
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
using System;

namespace Gnar
{
    static class Ult
    {
        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        public static bool isChecked(Menu obj, String value)
        {
            return obj[value].Cast<CheckBox>().CurrentValue;
        }

        public static int getSliderValue(Menu obj, String value)
        {
            return obj[value].Cast<Slider>().CurrentValue;
        }

        public static bool IsUltable(AIHeroClient target)
        {

            if (target.HasBuffOfType(BuffType.SpellImmunity) || target.HasBuffOfType(BuffType.SpellShield) || _Player.IsDashing()) return false;

            var posicao = Program._Player.Position.Extend(target.Position, Program._Player.Distance(target) - getSliderValue(Program.ComboMenu, "useRComboSlider")).To3D();
            for (int i = 0; i < 480 - getSliderValue(Program.ComboMenu, "useRComboSlider"); i += 10)
            {
                var cPos = _Player.Position.Extend(posicao, _Player.Distance(posicao) + i).To3D();
                if (cPos.ToNavMeshCell().CollFlags.HasFlag(CollisionFlags.Wall) || cPos.ToNavMeshCell().CollFlags.HasFlag(CollisionFlags.Building))
                {
                    return true;
                }
            }
            return false;
        }
    }
}