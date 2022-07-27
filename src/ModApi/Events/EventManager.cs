using System;
using System.Reflection;
using HarmonyLib;
using Microsoft.Xna.Framework;
using ModLoader.Content;
using Paris.Engine;
using Paris.Engine.Context;
using Paris.Engine.Controller;

namespace ModLoader.Events
{
    internal class EventManager : IEventHelper
    {
        internal static EventManager _singleton;
        public static EventManager Singleton
        {
            get
            {
                if (_singleton == null)
                    _singleton = new EventManager();
                return _singleton;
            }
        }

        public event EventHandler<GameInitializedEventArgs> GameInitialized;

        public event EventHandler<RequestingAssetEventArgs> RequestingAsset;

        public event EventHandler<AssetLoadedEventArgs> AssetLoaded;

        public event EventHandler<UpdateTickEventArgs> UpdateTick;

        public event EventHandler<UpdateTickEventArgs> UpdateTicked;

        public event EventHandler<DrawEventArgs> BeforeDraw;

        public event EventHandler<DrawEventArgs> AfterDraw;

        public event EventHandler<ConsoleInputReceivedEventArgs> ConsoleInputReceived;

        public event EventHandler<ContextSwitchedEventArgs> ContextSwitched;

        internal void Init()
        {
            try
            {
                var contentManager = new ApiContentManager(ContextManager.Singleton.GlobalContentManager.ServiceProvider, ContextManager.Singleton.GlobalContentManager.RootDirectory);
                ContextManager.Singleton.GlobalContentManager = contentManager;

                if (typeof(ContextManager).GetField("_game", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ContextManager.Singleton) is Game game)
                    game.Content = contentManager;
                else
                    GameInitialized += (o, e) => e.Game.Content = contentManager;
            }
            catch (Exception e)
            {
                TMNTModApi.Singleton.modHelper.Console.Error("1:" + e.Message);
                TMNTModApi.Singleton.modHelper.Console.Trace(e.StackTrace);
            }

            try
            {

                Harmony harmony = new Harmony("TMNTModApi.EventManager");

                harmony.Patch(
                   original: typeof(ContextManager).GetMethod("StartNewContentManager", BindingFlags.NonPublic | BindingFlags.Instance),
                   postfix: new HarmonyMethod(GetType().GetMethod(nameof(StartNewContentManager), BindingFlags.NonPublic | BindingFlags.Static))
                   );

                harmony.Patch(
                    original: typeof(Paris.Paris).GetMethod("Initialize", BindingFlags.NonPublic | BindingFlags.Instance),
                    postfix: new HarmonyMethod(GetType().GetMethod(nameof(Initialize), BindingFlags.NonPublic | BindingFlags.Static))
                    );
            
                harmony.Patch(
                    original: typeof(Paris.Paris).GetMethod("Tick", BindingFlags.NonPublic | BindingFlags.Instance),
                    prefix: new HarmonyMethod(GetType().GetMethod(nameof(Tick), BindingFlags.NonPublic | BindingFlags.Static))
                    );

                harmony.Patch(
                    original: typeof(Paris.Paris).GetMethod("Tick", BindingFlags.NonPublic | BindingFlags.Instance),
                    postfix: new HarmonyMethod(GetType().GetMethod(nameof(Tick2), BindingFlags.NonPublic | BindingFlags.Static))
                    );
            
                harmony.Patch(
                    original: typeof(ContextManager).GetMethod("SwitchToContext", new Type[] { typeof(string), typeof(float), typeof(Color), typeof(bool), typeof(bool), typeof(BaseController), typeof(bool) }),
                    prefix: new HarmonyMethod(GetType().GetMethod(nameof(SwitchToContext), BindingFlags.NonPublic | BindingFlags.Static))
                    );

                harmony.Patch(
                    original: typeof(Paris.Paris).GetMethod("Draw", BindingFlags.NonPublic | BindingFlags.Instance),
                    prefix: new HarmonyMethod(GetType().GetMethod(nameof(BeforeDrawPatch), BindingFlags.NonPublic | BindingFlags.Static))
                    );


                harmony.Patch(
                    original: typeof(Paris.Paris).GetMethod("Draw", BindingFlags.NonPublic | BindingFlags.Instance),
                    postfix: new HarmonyMethod(GetType().GetMethod(nameof(AfterDrawPatch), BindingFlags.NonPublic | BindingFlags.Static))
                    );


            }
            catch(Exception e)
            {
                TMNTModApi.Singleton.modHelper.Console.Error("2:" + e.Message);
                TMNTModApi.Singleton.modHelper.Console.Trace(e.StackTrace);
            }

        }

        internal static void BeforeDrawPatch(GameTime i_gameTime)
        {
            Singleton.BeforeDraw?.Invoke(null, new DrawEventArgs(i_gameTime));
        }

        internal static void AfterDrawPatch(GameTime i_gameTime)
        {
            Singleton.AfterDraw?.Invoke(null, new DrawEventArgs(i_gameTime));
        }

        internal static void StartNewContentManager(ContextManager __instance, ref ParisContentManager __result)
        {
            if (!(__instance.GlobalContentManager is ApiContentManager))
                __instance.GlobalContentManager = new ApiContentManager(__instance.GlobalContentManager.ServiceProvider, __instance.GlobalContentManager.RootDirectory);

            
            if (!(__result is ApiContentManager))
                __result = new ApiContentManager(__result.ServiceProvider, __result.RootDirectory);

        }

        internal static void SwitchToContext(ref string newContext)
        {
            string context = null;
            Singleton.ContextSwitched?.Invoke(null, new ContextSwitchedEventArgs(newContext, (s) => context = s));

            if (context != null)
            {
                newContext = context;
                SwitchToContext(ref newContext);
            }
        }

        internal static void Tick(Paris.Paris __instance, float deltaTime)
        {
            Singleton.UpdateTick?.Invoke(null, new UpdateTickEventArgs(__instance, deltaTime));
        }

        internal static void Tick2(Paris.Paris __instance, float deltaTime)
        {
            Singleton.UpdateTicked?.Invoke(null, new UpdateTickEventArgs(__instance, deltaTime));
        }


        internal static void Initialize(Paris.Paris __instance)
        {
            Singleton.GameInitialized?.Invoke(null, new GameInitializedEventArgs(__instance));
        }
        
        internal static void TriggerConsoleEvent(string input) => Singleton.ConsoleInputReceived?.Invoke(null, new ConsoleInputReceivedEventArgs(input));

        internal static object TriggerRequestingAssetEvent(string assetName, Type type)
        {
            object result = null;
            Singleton.RequestingAsset?.Invoke(null, new RequestingAssetEventArgs(assetName, type, (o) => result = o));

            return result;
        }

        internal static T TriggerAssetLoadedEvent<T>(string assetName, T asset)
        {
            T result = asset;
            Singleton.AssetLoaded?.Invoke(null,new AssetLoadedEventArgs(assetName,asset, (o) => result = (T) o));

            return result;
        }
    }
}
