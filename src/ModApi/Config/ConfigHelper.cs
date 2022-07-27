using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Paris;
using Paris.Engine.Context;
using Paris.Engine.Menu.Control;
using Paris.Engine.System.Localisation;
using Paris.Game.Menu;
using Paris.Game.Menu.Control;
using Paris.System.FSM;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Paris.Engine;
using Paris.Engine.Graphics;
using Paris.Engine.Controller;

namespace ModLoader.Config
{
    internal class ConfigHelper : IConfigHelper
    {
        IModHelper Helper;

        static MainMenu menu;
        static int modMenuIndex;
        static int modMenuState;
        static bool initialized = false;

        internal static List<OptionsmenuEntry> Options = new List<OptionsmenuEntry>();

        public ConfigHelper(IModHelper helper)
        {
            Helper = helper;
            Init();
        }

        public T LoadConfig<T>() where T : class
        {
            return Helper.Content.LoadJson<T>("config.json", (T) Activator.CreateInstance(typeof(T)), true);
        }

        public void SaveConfig<T>(T config) where T : class
        {
            Helper.Content.SaveJson(config, "config.json");
        }

        public void SetOptionsMenuEntry(string id, string title, Action<IOptionsMenuChange> onchange, Func<string> currentValue, params string[] choices)
        {
            Options.RemoveAll(o => o.Id == id);
            Options.Add(new OptionsmenuEntry(id,title,choices, onchange, currentValue, Helper));
        }

        internal static void Init()
        {
            if (initialized)
                return;

            Harmony harmony = new Harmony("TMNTModApi.ConfigHelper");

            harmony.Patch(
                  original: typeof(MainMenu).GetMethod("StateOptions", BindingFlags.Public | BindingFlags.Instance),
                  prefix: new HarmonyMethod(typeof(ConfigHelper).GetMethod(nameof(StateOptions), BindingFlags.NonPublic | BindingFlags.Static))
                  );

            harmony.Patch(
                  original: typeof(MainMenu).GetMethod("AddStates", BindingFlags.NonPublic | BindingFlags.Instance),
                  postfix: new HarmonyMethod(typeof(ConfigHelper).GetMethod(nameof(AddStates), BindingFlags.NonPublic | BindingFlags.Static))
                  );


            harmony.Patch(
                original: typeof(Enum).GetMethod(nameof(Enum.GetNames), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static),
                postfix: new HarmonyMethod(typeof(ConfigHelper), nameof(GetEnumNames))
                );

            harmony.Patch(
               original: typeof(MainMenu).GetMethod("Accept", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance),
               prefix: new HarmonyMethod(typeof(ConfigHelper), nameof(Accept))
               );

            harmony.Patch(
               original: typeof(MainMenu).GetMethod("Cancel", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance),
               postfix: new HarmonyMethod(typeof(ConfigHelper), nameof(Cancel))
               );

            initialized = true;
        }

        internal static void AddStates(MainMenu __instance)
        {
            FSM fsm = (FSM)typeof(MainMenu).GetField("_fsm", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            List<FSM.State> states = (List<FSM.State>) typeof(FSM).GetField("_states", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(fsm);
            modMenuState = states.Count;
            fsm.AddState(new FSM.State((f,d) => StateModOptions(__instance, f, d)));
            menu = __instance;            
        }

        internal static void StateOptions(MainMenu __instance, FSM.StateStep stateStep, float deltaTime)
        {
            Options options = (Options)typeof(MainMenu).GetField("_options", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            SelectionMenuControl selection = (SelectionMenuControl)typeof(MainMenu).GetField("_optionsSelection", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);

            if (stateStep == FSM.StateStep.Constructor)
            {
                options.SetupOptions(Paris.Game.Menu.Options.MenuSchemes.MainMenu, selection);
                options.ButtonConfigSelected = new Action<SelectionItem>((s) => __instance.GetType().GetMethod("ButtonConfigSelected", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[1] { s })) ;
                options.ResetToDefaultsSelected = new Action<SelectionItem>((s) => __instance.GetType().GetMethod("ResetToDefaultsSelected", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[1] { s }));
            }
            menu = __instance;
        }

        public static void StateModOptions(MainMenu __instance, FSM.StateStep stateStep, float deltaTime)
        {
            switch (stateStep)
            {
                case Paris.System.FSM.FSM.StateStep.Constructor:
                    {
                        GameMenu menu = (GameMenu)typeof(MainMenu).GetField("_menu", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);

                        typeof(LocManager).GetMethod("AddLocToLanguage", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(LocManager.Singleton, new object[] { LocManager.Singleton.CurrentLanguage, "mnuModOptions", "ModOptions" });
                        SelectionMenuControl selection = (SelectionMenuControl)typeof(MainMenu).GetField("_optionsSelection", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
                        Options options = (Options)typeof(MainMenu).GetField("_options", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);

                        LoadModOptions(selection, options);

                        MenuTitleHeader mainTitle = ((MenuTitleHeader)typeof(MainMenu).GetField("_mainTitle", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance));
                        FSM fsm = (FSM)typeof(MainMenu).GetField("_fsm", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);

                        ((AnimatedObjectControl)typeof(MainMenu).GetField("_portrait", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance)).Visible = false;
                        ((ButtonDisplay)typeof(MainMenu).GetField("_buttonDisplay", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance)).ActiveButtons = MenuButtons.Cancel;
                        ((GameMenu)typeof(MainMenu).GetField("_menu", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance)).Focus = (MenuObject)selection;
                        mainTitle.Title.ID = "mnuModOptions";
                        mainTitle.TitleOverride = "ModOptions";
                        typeof(MainMenu).GetField("_prevAlignment", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, mainTitle.Alignment);
                        mainTitle.Alignment = Renderer.Alignment.Centered;
                        selection.Parent.Visible = true;
                        selection.ResetSelection();
                        selection.CursorColor = GameColors.GetMenuSelectionColor(selection.Selection);
                        options.HaveChanged = false;
                        options.Show((MenuController)(typeof(MainMenu).GetField("_controller", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance)), selection);
                        break;
                    }
                case Paris.System.FSM.FSM.StateStep.Tick:
                    {
                        MenuTitleHeader mainTitle = ((MenuTitleHeader)typeof(MainMenu).GetField("_mainTitle", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance));

                        SelectionMenuControl selection = (SelectionMenuControl)typeof(MainMenu).GetField("_optionsSelection", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
                        mainTitle.TitleOverride = "ModOptions";

                        if (!selection.HasChanged)
                            break;
                        if (selection.SelectedItem.SelectCallback != null)
                        {
                            ((ButtonDisplay)typeof(MainMenu).GetField("_buttonDisplay", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance)).ActiveButtons = MenuButtons.Accept | MenuButtons.Cancel;
                            break;
                        }
                    ((ButtonDisplay)typeof(MainMenu).GetField("_buttonDisplay", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance)).ActiveButtons = MenuButtons.Cancel;
                        break;
                    }
                case Paris.System.FSM.FSM.StateStep.Destructor:
                    {
                        FSM fsm = (FSM)typeof(MainMenu).GetField("_fsm", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
                        Options options = (Options)typeof(MainMenu).GetField("_options", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
                        SelectionMenuControl selection = (SelectionMenuControl)typeof(MainMenu).GetField("_optionsSelection", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
                        MenuTitleHeader mainTitle = ((MenuTitleHeader)typeof(MainMenu).GetField("_mainTitle", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance));

                        if (fsm.NextState == (byte)9 || fsm.NextState == (byte)7)
                            break;

                        selection.Parent.Visible = false;
                        ((ButtonDisplay)typeof(MainMenu).GetField("_buttonDisplay", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance)).ActiveButtons = MenuButtons.Accept | MenuButtons.Cancel;
                        ((AnimatedObjectControl)typeof(MainMenu).GetField("_portrait", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance)).Visible = true;
                        mainTitle.Alignment = (Renderer.Alignment)typeof(MainMenu).GetField("_prevAlignment", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
                        break;
                    }
            }
        }

        public static void Cancel(MainMenu __instance, ref bool __result)
        {
            FSM fsm = (FSM)typeof(MainMenu).GetField("_fsm", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            SelectionMenuControl main = (SelectionMenuControl)typeof(MainMenu).GetField("_mainMenuSelection", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);

            if (fsm.CurrentState == (byte)modMenuState)
            {
                fsm.ChangeState(2);
                __result = true;
            }

        }

        public static bool Accept(MainMenu __instance)
        {
            menu = __instance;

            FSM fsm = (FSM)typeof(MainMenu).GetField("_fsm", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            SelectionMenuControl main = (SelectionMenuControl)typeof(MainMenu).GetField("_mainMenuSelection", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);

            if (fsm.CurrentState == 2)
                if (main.Selection == modMenuIndex + 1)
                    ContextManager.Singleton.Exit(1f, Color.Black);
                else if (main.Selection == modMenuIndex)
                {
                    fsm.ChangeState((byte)modMenuState);
                    return false;
                }
            return true;
        }

        public static void LoadModOptions(SelectionMenuControl selectionControl, Options options)
        {
            try
            {
                selectionControl.Clear();
                List<string> modTitles = new List<string>();
                foreach (var o in Options.OrderBy(p => p.Id))
                {
                    typeof(LocManager).GetMethod("AddLocToLanguage", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(LocManager.Singleton, new object[] { LocManager.Singleton.CurrentLanguage, o.Helper.Manifest.Id, o.Helper.Manifest.Name });

                    if (!modTitles.Contains(o.Helper.Manifest.Id))
                    {
                        var title = AddOptionsItem(selectionControl, new LocID(o.Helper.Manifest.Id), options);
                        title.Disabled = true;
                        modTitles.Add(o.Helper.Manifest.Id);
                    }

                    typeof(LocManager).GetMethod("AddLocToLanguage", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(LocManager.Singleton, new object[] { LocManager.Singleton.CurrentLanguage, o.Id, o.Name });
                    var select = AddOptionsItem(selectionControl, new LocID(o.Id), options);

                    foreach (string s in o.Choices)
                    {
                        typeof(LocManager).GetMethod("AddLocToLanguage", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(LocManager.Singleton, new object[] { LocManager.Singleton.CurrentLanguage, o.Id + "." + s.Replace(" ", "_"), s });
                        select.AddOption(new LocID(o.Id + "." + s.Replace(" ", "_")));
                    }

                    select.OptionChangedCallback = new SelectionItem.SelectionCallback((i) => o.Change((i as OptionSelectionItem).Value));
                    select.Value = o.Current();

                }

                selectionControl.RefreshPositioning();
            }
            catch (Exception ex)
            {
                
            }
        }

        public static  OptionSelectionItem AddOptionsItem(SelectionMenuControl selectionControl, LocID locID, Options options)
        {
            SelectionMenuControl selectionMenuControl = selectionControl;
            OptionSelectionItem optionSelectionItem = new OptionSelectionItem(locID, MainMenu.MENU_SELECTED_FONT, 340, GameColors.TextSelected, GameColors.TextDeselected, GameColors.TextDisabled);
            optionSelectionItem.UnselectedFont = MainMenu.MENU_UNSELECTED_FONT;
            optionSelectionItem.DisabledFont = MainMenu.MENU_DISABLED_FONT;
            optionSelectionItem.ArrowLeftTexture = (Texture2D) typeof(Options).GetField("_arrowLeftTexture", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(options);
            optionSelectionItem.ArrowRightTexture = (Texture2D)typeof(Options).GetField("_arrowRightTexture", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(options);
            optionSelectionItem.ArrowLeftColor = GameColors.ArrowLeftColor;
            optionSelectionItem.ArrowRightColor = GameColors.ArrowRightColor;
            optionSelectionItem.OutlineSelectedColor = Color.Black;
            optionSelectionItem.OutlineUnselectedColor = Color.Black;
            optionSelectionItem.OutlineDisabledColor = Color.Black;
            return (OptionSelectionItem)selectionMenuControl.Add((SelectionItem)optionSelectionItem);
        }

        public static void GetEnumNames(Type enumType, ref string[] __result)
        {
            if (enumType.Name == "MainMenuItems")
            {
                var list = new List<string>(__result);
                string name = "ModOptions";
                list.Remove("Exit");
                modMenuIndex = list.Count;
                list.Add("ModOptions");
                list.Add("Exit");

                typeof(LocManager).GetMethod("AddLocToLanguage", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(LocManager.Singleton, new object[] { LocManager.Singleton.CurrentLanguage, "mnu" + name, name });
                __result = list.ToArray();
            }
        }
    }
}
