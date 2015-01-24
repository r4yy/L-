using System;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace Anivia
{
    class Program
    {
        public static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        public static Spell Q, W, E, R;
        public static Orbwalking.Orbwalker Orbwalker;
        public static SpellSlot IgniteSlot;
        public static GameObject QGameObject, RGameObject;
        public static bool SelfUlt;
        public static Menu MyMenu;

        public static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Anivia")
                return;

            IgniteSlot = Player.GetSpellSlot("SummonerDot");

            #region SpellInfos

            Q = new Spell(SpellSlot.Q, 1100f);
            Q.SetSkillshot(0.25f, 110f, 850f, false, SkillshotType.SkillshotLine);

            //W = new Spell(SpellSlot.W, 1000f);
            //W.SetSkillshot(W.Instance.SData.SpellCastTime, W.Instance.SData.LineWidth, W.Instance.SData.MissileSpeed, false, SkillshotType.SkillshotLine);

            E = new Spell(SpellSlot.E, 650f);
            E.SetTargetted(E.Instance.SData.SpellCastTime, E.Instance.SData.MissileSpeed);

            R = new Spell(SpellSlot.R, 625f);
            R.SetSkillshot(0.25f, 200, float.MaxValue, false, SkillshotType.SkillshotCircle);

            #endregion

            #region Menu

            MyMenu = new Menu("Articuno - The icing bird", "Anivia", true);

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            MyMenu.AddSubMenu(targetSelectorMenu);

            MyMenu.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(MyMenu.SubMenu("Orbwalking"));

            MyMenu.AddSubMenu(new Menu("Combo", "Combo"));
            MyMenu.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            //MyMenu.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            MyMenu.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            MyMenu.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use R").SetValue(true));

            MyMenu.AddSubMenu(new Menu("Harass", "Harass"));
            MyMenu.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
            MyMenu.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));
            MyMenu.SubMenu("Harass").AddItem(new MenuItem("ManaHarass", "Harass if mana >").SetValue(new Slider(0)));

            MyMenu.AddSubMenu(new Menu("LaneClear", "LaneClear"));
            MyMenu.SubMenu("LaneClear").AddItem(new MenuItem("UseQLC", "Use Q").SetValue(true));
            MyMenu.SubMenu("LaneClear").AddItem(new MenuItem("UseRLC", "Use R").SetValue(false));
            MyMenu.SubMenu("LaneClear").AddItem(new MenuItem("ManaLC", "Clear if mana >").SetValue(new Slider(0)));

            MyMenu.AddSubMenu(new Menu("JungleClear", "JungleClear"));
            MyMenu.SubMenu("JungleClear").AddItem(new MenuItem("UseQJC", "Use Q").SetValue(true));
            MyMenu.SubMenu("JungleClear").AddItem(new MenuItem("UseEJC", "Use E").SetValue(true));
            MyMenu.SubMenu("JungleClear").AddItem(new MenuItem("UseRJC", "Use R").SetValue(true));
            MyMenu.SubMenu("JungleClear").AddItem(new MenuItem("ManaJC", "Clear if mana >").SetValue(new Slider(0)));

            MyMenu.AddSubMenu(new Menu("Misc", "Misc"));
            MyMenu.SubMenu("Misc").AddItem(new MenuItem("PacketCast", "Use PacketCast").SetValue(true));
            MyMenu.SubMenu("Misc").AddItem(new MenuItem("UseEChilled", "E only if chilled").SetValue(true));
            MyMenu.SubMenu("Misc").AddItem(new MenuItem("AutoIgnite", "Auto Ignite if killable").SetValue(true));

            MyMenu.AddSubMenu(new Menu("Drawings", "Drawings"));
            MyMenu.SubMenu("Drawings")
                .AddItem(new MenuItem("QRange", "Q Range").SetValue(new Circle(false, Color.Black, Q.Range)));
            //MyMenu.SubMenu("Drawings").AddItem(new MenuItem("WRange", "W Range").SetValue(new Circle(false, System.Drawing.Color.FromArgb(35, 105, 105, 105))));
            MyMenu.SubMenu("Drawings")
                .AddItem(new MenuItem("ERange", "E Range").SetValue(new Circle(false, Color.Cyan)));
            MyMenu.SubMenu("Drawings")
                .AddItem(new MenuItem("RRange", "R Range").SetValue(new Circle(false, Color.Cyan)));

            MyMenu.AddToMainMenu();

            #endregion

            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            //Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
            //AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Game.PrintChat("Anivia by r4yy - Successfully loaded!");
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.SData.Name.Contains("cryo_storm"))
            {
                SelfUlt = true;
            }
            else
            {
                SelfUlt = false;
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            DetonateQ();
            StopR();

            if (MyMenu.Item("AutoIgnite").GetValue<bool>())
                CastIgnite();
            
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;
                    //case Orbwalking.OrbwalkingMode.LaneClear:
                    //    LC();
                    //    JC();
                    //    break;
                case Orbwalking.OrbwalkingMode.None:
                    break;
            }
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target == null || target.IsInvulnerable) return;
            
            var useQ = MyMenu.Item("UseQCombo").GetValue<bool>();
            var useE = MyMenu.Item("UseECombo").GetValue<bool>();
            var useR = MyMenu.Item("UseRCombo").GetValue<bool>();
            var useEChilled = MyMenu.Item("UseEChilled").GetValue<bool>();

            if (useR && R.IsReady())
            {
                CastR(target);
            }

            if (useEChilled)
            {
                if (target.HasBuff("Chilled") && useE && E.IsReady())
                {
                    CastE(target);
                }
            }
            else if (useE && E.IsReady())
            {
                CastE(target);
            }

            if (useQ && Q.IsReady())
            {
                CastQ(target);
            }
        }
        private static void Harass()
        {
            if (Player.Mana / Player.MaxMana * 100 < MyMenu.Item("ManaHarass").GetValue<Slider>().Value) 
                return;

            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target == null || target.IsInvulnerable) return;

            var useQ = MyMenu.Item("UseQHarass").GetValue<bool>();
            var useE = MyMenu.Item("UseEHarass").GetValue<bool>();
            var useEChilled = MyMenu.Item("UseEChilled").GetValue<bool>();

            if (useQ && Q.IsReady())
            {
                CastQ(target);
            }

            if (useEChilled)
            {
                if (target.HasBuff("Chilled") && useE && E.IsReady())
                {
                    CastE(target);
                }
            }
            else if (useE && E.IsReady())
            {
                CastE(target);
            }
        }

        private static void CastQ(Obj_AI_Base unit)
        {
            if (unit.IsValidTarget(Q.Range) && QGameObject == null)
            {
                Q.CastIfHitchanceEquals(unit, HitChance.High);
            }
            
        }
        private static void CastE(Obj_AI_Base unit)
        {
            if (unit.IsValidTarget(E.Range))
            {
                E.CastOnUnit(unit);
            }
        }
        private static void CastR(Obj_AI_Base unit)
        {
            if (unit.IsValidTarget(R.Range) && RGameObject == null)
            {
                R.Cast(unit,false,true);
            }
        }

        private static void DetonateQ()
        {
            var enemies = ObjectManager.Get<Obj_AI_Hero>().FindAll(enemy => enemy.IsValidTarget());
            Game.PrintChat("Enemies: " + enemies);
            foreach (var enemy in enemies.Where(enemy => QGameObject != null && QGameObject.Position.Distance(enemy.ServerPosition) < 150)) {
                Game.PrintChat("Enemy: " + enemy);
                Q.Cast();
            }
        }
        private static void StopR()
        {
            if (SelfUlt) return;
            var enemies = ObjectManager.Get<Obj_AI_Hero>().FindAll(enemy => enemy.IsValidTarget());

            foreach (var enemy in enemies.Where(enemy => RGameObject != null && RGameObject.Position.Distance(enemy.ServerPosition) > 400))
                R.Cast();
        }

        private static void CastIgnite()
        {
            if (IgniteSlot == SpellSlot.Unknown && Player.Spellbook.CanUseSpell(IgniteSlot) != SpellState.Ready)
                return;

            var enemies = ObjectManager.Get<Obj_AI_Hero>().FindAll(enemy => enemy.IsValidTarget());

            foreach (var enemy in enemies.Where(enemy => enemy.Health < 55 + Player.Level * 20))
            {
                Player.Spellbook.CastSpell(IgniteSlot, enemy);
            }
        }

        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (sender != null && sender.IsValid && sender.Name.Contains("FlashFrost_mis"))
            {
                QGameObject = sender;
            }

            if (sender != null && sender.IsValid && sender.Name.Contains("cryo_storm"))
            {
                RGameObject = sender;
            }
        }
        private static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            if (sender != null && sender.IsValid && sender.Name.Contains("FlashFrost_mis"))
            {
                QGameObject = null;
            }

            if (sender != null && sender.IsValid && sender.Name.Contains("cryo_storm"))
            {
                RGameObject = null;
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;

            var drawQ = MyMenu.Item("QRange").GetValue<Circle>();
            var drawE = MyMenu.Item("ERange").GetValue<Circle>();
            var drawR = MyMenu.Item("RRange").GetValue<Circle>();

            if (drawQ.Active && Q.IsReady())
            {
                Utility.DrawCircle(Player.Position, Q.Range, drawQ.Color);
            }

            if (drawE.Active && E.IsReady())
            {
                Utility.DrawCircle(Player.Position, E.Range, drawE.Color);
            }

            if (drawR.Active && R.IsReady())
            {
                Utility.DrawCircle(Player.Position, R.Range, drawR.Color);
            }
        }
    }
}
