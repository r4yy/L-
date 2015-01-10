﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;


namespace Anivia
{
    class Program
    {
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        private static Obj_AI_Hero target { get { return ObjectManager.Player; } }

        private static Menu myMenu;
        private static Orbwalking.Orbwalker Orbwalker;

        private static List<Spell> SpellList = new List<Spell>();
        private static Spell Q, W, E, R;
        //private static SpellSlot Ignite;
        private static GameObject qObj, rObj;

        //private static bool packetCast;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        public static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.BaseSkinName != "Anivia") return;
            Menu();

            Q = new Spell(SpellSlot.Q, 1100f);
            //W = new Spell(SpellSlot.W, 1000f);
            E = new Spell(SpellSlot.E, 650f);
            R = new Spell(SpellSlot.R, 625f);

            Q.SetSkillshot(0.25f, 110f, 850f, false, SkillshotType.SkillshotLine);
            E.SetTargetted(0.25f, 1200f);
            R.SetSkillshot(0.25f, 400f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            //SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            //Ignite = Player.GetSpellSlot("summonordot");

            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            GameObject.OnCreate += OnCreate;
            GameObject.OnDelete += OnDelete;
            Game.PrintChat("Bzzzzzzt");
        }



        private static void OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.Name.Contains("FlashFrost_mis"))
            {
                qObj = sender;
            }
            if (sender.Name.Contains("cryo_storm"))
            {
                rObj = sender;
            }

        }

        private static void OnDelete(GameObject sender, EventArgs args)
        {
            if (sender.Name.Contains("FlashFrost_mis"))
            {
                qObj = null;
            }
            if (sender.Name.Contains("cryo_storm"))
            {
                rObj = null;
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            drawRanges();
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead) return;
            Obj_AI_Hero target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (myMenu.Item("comboMode").GetValue<KeyBind>().Active)
            {
                Combo();
            }

            if (myMenu.Item("harassMode").GetValue<KeyBind>().Active)
            {
                Harass();
            }

            if (myMenu.Item("UseEOnlyChilled").GetValue<bool>())
            {
                if (target.HasBuff("chilled"))
                {
                    castE(target);
                }
            }
        }

        private static void Combo()
        {
            if (target == null) return;

            detonateQ();
            TurnOffR();

            if (target != null)
            {
                if (myMenu.Item("UseECombo").GetValue<bool>()) { castE(target); }
                if (myMenu.Item("UseRCombo").GetValue<bool>()) { castR(target); }
                if (myMenu.Item("UseQCombo").GetValue<bool>()) { castQ(target); }
            }
        }
        private static void Harass()
        {
            if (target == null) return;
            if (Player.Mana / Player.MaxMana * 100 > myMenu.Item("ManaHarass").GetValue<Slider>().Value && target != null)
            {
                if (myMenu.Item("UseQCombo").GetValue<bool>()) { castQ(target); detonateQ(); }
                if (myMenu.Item("UseQCombo").GetValue<bool>()) { castE(target); }
            }
        }

        private static void TurnOffR()
        {
            if (rObj != null && Player.Position.Distance(rObj.Position) < R.Range)
            {
                if (rObj.Position.Distance(target.Position) < 400)
                {
                    R.Cast();
                }
            }
        }
        private static void detonateQ()
        {
            if (qObj != null && Player.Position.Distance(qObj.Position) < 1300)
            {
                if (Q.Width > qObj.Position.Distance(target.Position))
                {
                    Q.Cast();
                }
            }
        }

        private static void castQ(Obj_AI_Hero unit)
        {
            if (unit.IsValidTarget(Q.Range) && Q.IsReady() && qObj == null && myMenu.Item("UseQCombo").GetValue<bool>())
            {
                Q.CastIfHitchanceEquals(unit, HitChance.Medium, false);
            }
        }

        private static void castE(Obj_AI_Hero unit)
        {
            if (unit.IsValidTarget(E.Range) && E.IsReady() && myMenu.Item("UseECombo").GetValue<bool>())
            {
                E.Cast(unit);
            }
        }

        private static void castR(Obj_AI_Hero unit)
        {
            if (target.IsValidTarget(R.Range) && R.IsReady() && myMenu.Item("UseRCombo").GetValue<bool>())
            {
                R.CastIfHitchanceEquals(target, HitChance.Low, false);
            }
        }
        private static void Menu()
        {
            myMenu = new Menu("Articuno", "Articuno", true);

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            myMenu.AddSubMenu(targetSelectorMenu);

            myMenu.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(myMenu.SubMenu("Orbwalking"));

            myMenu.AddSubMenu(new Menu("Combo", "Combo"));
            myMenu.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            //myMenu.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            myMenu.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            myMenu.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use R").SetValue(true));
            myMenu.SubMenu("Combo").AddItem(new MenuItem("UseIgnite", "Use Ignite").SetValue(true));

            myMenu.AddSubMenu(new Menu("Harass", "Harass"));
            myMenu.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
            myMenu.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));
            myMenu.SubMenu("Harass").AddItem(new MenuItem("UseRHarass", "Use R").SetValue(true));
            myMenu.SubMenu("Harass").AddItem(new MenuItem("ManaHarass", "Harass if mana >").SetValue(new Slider(0)));

            myMenu.AddSubMenu(new Menu("LaneClear", "LaneClear"));
            myMenu.SubMenu("LaneClear").AddItem(new MenuItem("UseQLC", "Use Q").SetValue(true));
            myMenu.SubMenu("LaneClear").AddItem(new MenuItem("UseRLC", "Use R").SetValue(false));
            myMenu.SubMenu("LaneClear").AddItem(new MenuItem("ManaLC", "Harass if mana >").SetValue(new Slider(0)));

            myMenu.AddSubMenu(new Menu("JungleClear", "JungleClear"));
            myMenu.SubMenu("JungleClear").AddItem(new MenuItem("UseQJC", "Use Q").SetValue(true));
            myMenu.SubMenu("JungleClear").AddItem(new MenuItem("UseEJC", "Use E").SetValue(true));
            myMenu.SubMenu("JungleClear").AddItem(new MenuItem("UseRJC", "Use R").SetValue(true));
            myMenu.SubMenu("JungleClear").AddItem(new MenuItem("ManaJC", "Harass if mana >").SetValue(new Slider(0)));

            myMenu.AddSubMenu(new Menu("Misc", "Misc"));
            myMenu.SubMenu("Misc").AddItem(new MenuItem("PacketCast", "Use PacketCast").SetValue(true));
            myMenu.SubMenu("Misc").AddItem(new MenuItem("AutoE", "Auto use E when enemy is chilled").SetValue(true));

            myMenu.AddSubMenu(new Menu("Drawings", "Drawings"));
            //myMenu.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q Range").SetValue(new Circle(true, System.Drawing.Color.FromArgb(35, 105, 105, 105))));
            //myMenu.SubMenu("Drawings").AddItem(new MenuItem("WRange", "W Range").SetValue(new Circle(false, System.Drawing.Color.FromArgb(35, 105, 105, 105))));
            //myMenu.SubMenu("Drawings").AddItem(new MenuItem("ERange", "E Range").SetValue(new Circle(false, System.Drawing.Color.FromArgb(35, 105, 105, 105))));
            //myMenu.SubMenu("Drawings").AddItem(new MenuItem("RRange", "R Range").SetValue(new Circle(false, System.Drawing.Color.FromArgb(35, 105, 105, 105))));

            myMenu.AddToMainMenu();
        }
        private static void drawRanges()
        {
            throw new NotImplementedException();
        }
    }
}